'use client';
import React, { useEffect, useState, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { useRouter } from 'next/navigation';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dropdown } from 'primereact/dropdown';
import { Toast } from 'primereact/toast';
import api from '../../../../utils/api';
import { Avatar } from 'primereact/avatar';

const IdeasListPage = () => { 
    const { t } = useTranslation();
    const router = useRouter();
    const toast = useRef<Toast>(null);
    const [ideas, setIdeas] = useState([]);
    const [loading, setLoading] = useState(true);
    const [currentUserId, setCurrentUserId] = useState(0);
    const [isSuperAdmin, setIsSuperAdmin] = useState(false);
    
    // Filters and Sorts
    const [filter, setFilter] = useState('all');
    const [sort, setSort] = useState('date_desc');

    const sortOptions = [
        { label: t('ideasList.sortNewest'), value: 'date_desc' },
        { label: t('ideasList.sortOldest'), value: 'date_asc' },
        { label: t('ideasList.sortScoreDesc'), value: 'score_desc' },
        { label: t('ideasList.sortScoreAsc'), value: 'score_asc' },
        { label: t('ideasList.sortTitleAsc'), value: 'title_asc' },
        { label: t('ideasList.sortTitleDesc'), value: 'title_desc' }
    ];

    const fetchIdeas = async () => {
        setLoading(true);
        try {
            const response = await api.get(`/ideas/list?filter=${filter}&sort=${sort}`);
            setIdeas(response.data.ideas);
            setCurrentUserId(response.data.currentUserId);
            setIsSuperAdmin(response.data.isSuperAdmin);
        } catch (error: any) {
            console.error(t('ideasList.fetchErrorLog'), error);
            toast.current?.show({ severity: 'error', summary: t('ideasList.unauthorizedToastTitle'), detail: t('ideasList.unauthorizedToastDesc') });
            if (error.response?.status === 403) {
                router.push('/');
            }
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchIdeas();
    }, [filter, sort]);

    const formatDate = (dateStr: string) => {
        const d = new Date(dateStr);
        return d.toLocaleDateString('tr-TR', { day: '2-digit', month: '2-digit', year: 'numeric' });
    };

    const getInitials = (name: string) => {
        if (!name || name === t('ideasList.hiddenUser')) return "U";
        const parts = name.split(' ');
        if (parts.length >= 2) return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
        return name[0].toUpperCase();
    };

    const authorBodyTemplate = (rowData: any) => {
        const isHidden = rowData.authorName?.includes('Gizli');
        const isMyIdea = rowData.isOwner;

        if (isHidden) {
            return <span className="bg-gray-100 text-gray-700 px-2 py-1 border-round text-sm font-semibold border-1 border-gray-300 white-space-nowrap"><i className="pi pi-eye-slash mr-1"></i> {t('ideasList.anonymous')}</span>;
        }

        return (
            <div className="flex align-items-center gap-2">
                <Avatar 
                    image={rowData.authorProfilePictureUrl ? `${api.defaults.baseURL?.replace('/api', '') || ''}${rowData.authorProfilePictureUrl}?t=${Date.now()}` : undefined} 
                    label={!rowData.authorProfilePictureUrl ? getInitials(rowData.authorName) : undefined} 
                    shape="circle" className="font-bold bg-blue-100 text-blue-600 flex-shrink-0" 
                />
                <div>
                    <div className="font-semibold" style={{ color: 'var(--text-color)' }}>
                        {rowData.authorName} 
                        {isMyIdea && <span className="bg-gray-100 text-gray-600 px-1 py-0 ml-1 border-round" style={{ fontSize: '0.7rem' }}>{t('ideasList.you')}</span>}
                    </div>
                </div>
            </div>
        );
    };

    const titleBodyTemplate = (rowData: any) => (
        <span className="font-semibold" style={{ color: 'var(--text-color)' }}>{rowData.title}</span>
    );

    const statusBodyTemplate = (rowData: any) => {
        switch (rowData.status) {
            case 'Implemented':
                return <span className="bg-orange-100 text-orange-700 px-2 py-1 border-round text-sm font-semibold border-1 border-orange-300 white-space-nowrap"><i className="pi pi-star-fill mr-1"></i> {t('ideasList.statusGlittering')}</span>;
            case 'Approved':
                return <span className="bg-green-100 text-green-800 px-2 py-1 border-round text-sm font-semibold border-1 border-green-300 white-space-nowrap">{t('ideasList.statusApproved')}</span>;
            case 'Rejected':
                return <span className="bg-red-100 text-red-800 px-2 py-1 border-round text-sm font-semibold border-1 border-red-300 white-space-nowrap">{t('ideasList.statusRejected')}</span>;
            default: // Pending
                return <span className="bg-orange-100 text-orange-800 px-2 py-1 border-round text-sm font-semibold border-1 border-orange-300 white-space-nowrap">{t('ideasList.statusPending')}</span>;
        }
    };

    
    const handleReopen = async (id: number) => {
        try {
            const res = await api.post(`/ideas/${id}/reopen`);
            toast.current?.show({ severity: 'success', summary: t('toast.success'), detail: t('ideasList.reopenSuccess') });
            fetchIdeas();
        } catch (error: any) {
            toast.current?.show({ severity: 'error', summary: t('toast.error'), detail: t('toast.genericError') });
        }
    };

    const actionBodyTemplate = (rowData: any) => {
        const isMyIdea = rowData.isOwner;
        const isPending = rowData.status === 'Pending';
        const canReopen = isSuperAdmin && (rowData.status === 'Approved' || rowData.status === 'Rejected');

        return (
            <div className="flex justify-content-end gap-2 flex-nowrap">
                {canReopen && (
                    <Button onClick={() => { if(window.confirm(t('ideasList.reopenConfirm'))) handleReopen(rowData.id); }} icon="pi pi-sync" severity="warning" outlined size="small" className="py-1 px-2" tooltip={t('ideasList.reopen')} />
                )}
                <Button onClick={() => router.push(`/ideas/details/${rowData.id}`)} label={isMyIdea || !isPending ? t('ideasList.btnReview') : t('ideasList.btnReviewRate')} severity={isMyIdea || !isPending ? "secondary" : "success"} outlined={isMyIdea || !isPending} size="small" className="py-1 px-3" />
            </div>
        );
    };

    return (
        <div className="grid">
            <Toast ref={toast} />
            <div className="col-12">
                <div className="flex flex-wrap justify-content-between align-items-center mb-4 gap-3">
                    <div>
                        <h3 className="font-bold mb-1" style={{ color: 'var(--text-color)', letterSpacing: '-0.02em' }}>{t('ideasList.pageTitle')}</h3>
                        <p className="m-0 text-600" style={{ fontSize: '0.95rem' }}>{t('ideasList.pageDesc')}</p>
                    </div>
                    <div className="flex flex-wrap align-items-center gap-2">
                        <Dropdown value={sort} options={sortOptions} onChange={(e) => setSort(e.value)} className="w-auto p-inputtext-sm" />
                        <div className="p-buttonset">
                            <Button label={t('ideasList.filterAll')} severity={filter === 'all' ? 'success' : 'secondary'} outlined={filter !== 'all'} onClick={() => setFilter('all')} size="small" />
                            <Button label={t('ideasList.filterPending')} severity={filter === 'pending' ? 'success' : 'secondary'} outlined={filter !== 'pending'} onClick={() => setFilter('pending')} size="small" />
                            <Button label={t('ideasList.filterApproved')} severity={filter === 'approved' ? 'success' : 'secondary'} outlined={filter !== 'approved'} onClick={() => setFilter('approved')} size="small" />
                            <Button label={t('ideasList.filterRejected')} severity={filter === 'rejected' ? 'success' : 'secondary'} outlined={filter !== 'rejected'} onClick={() => setFilter('rejected')} size="small" />
                        </div>
                    </div>
                </div>

                <div className="card p-0 overflow-hidden shadow-2 border-round-xl">
                    <DataTable value={ideas} loading={loading} emptyMessage={t('ideasList.emptyMessage')}>
                        <Column header={t('ideasList.colAuthor')} body={authorBodyTemplate} ></Column>
                        <Column field="title" header={t('ideasList.colTitle')} body={titleBodyTemplate} ></Column>
                        <Column field="categoryName" header={t('ideasList.colCategory')} body={(rowData) => <span className="text-600 white-space-nowrap">{t(rowData.categoryName === 'Hizmet Geliştirme' ? 'category.service' : rowData.categoryName === 'Ürün İyileştirme' ? 'category.product' : rowData.categoryName === 'Süreç & Verimlilik' ? 'category.process' : rowData.categoryName ? rowData.categoryName : 'category.other')}</span>}></Column>
                        <Column field="createdAt" header={t('ideasList.colDate')} body={(rowData) => <span className="text-500 text-sm">{formatDate(rowData.createdAt)}</span>}></Column>
                        <Column header={t('ideasList.colStatus')} body={statusBodyTemplate}></Column>
                        <Column header={t('ideasList.colActions')} body={actionBodyTemplate} align="right" ></Column>
                    </DataTable>
                </div>
            </div>
        </div>
    );
};

export default IdeasListPage;
