'use client';
import React, { useEffect, useState, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { useRouter } from 'next/navigation';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dropdown } from 'primereact/dropdown';
import { Dialog } from 'primereact/dialog';
import { Toast } from 'primereact/toast';
import { ProgressSpinner } from 'primereact/progressspinner';
import api from '../../../../utils/api';
import Link from 'next/link';

const MyIdeasPage = () => { 
    const { t } = useTranslation();
    const router = useRouter();
    const toast = useRef<Toast>(null);
    const [ideas, setIdeas] = useState([]);
    const [loading, setLoading] = useState(true);
    
    // Filters and Sorts
    const [filter, setFilter] = useState('all');
    const [sort, setSort] = useState('date_desc');
    
    // Withdraw Modal
    const [withdrawDialogVisible, setWithdrawDialogVisible] = useState(false);
    const [selectedIdea, setSelectedIdea] = useState<any>(null);
    const [isWithdrawing, setIsWithdrawing] = useState(false);

    const sortOptions = [
        { label: t('myIdeas.sortNewest'), value: 'date_desc' },
        { label: t('myIdeas.sortOldest'), value: 'date_asc' },
        { label: t('myIdeas.sortTitleAsc'), value: 'title_asc' },
        { label: t('myIdeas.sortTitleDesc'), value: 'title_desc' }
    ];

    const fetchIdeas = async () => {
        setLoading(true);
        try {
            const response = await api.get(`/ideas/my?filter=${filter}&sort=${sort}`);
            setIdeas(response.data);
        } catch (error) {
            console.error(t('myIdeas.fetchErrorLog'), error);
            toast.current?.show({ severity: 'error', summary: t('myIdeas.error'), detail: t('myIdeas.fetchErrorToast') });
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchIdeas();
    }, [filter, sort]);

    const handlePublish = async (id: number) => {
        try {
            const response = await api.post(`/ideas/${id}/publish`);
            toast.current?.show({ severity: 'success', summary: t('myIdeas.success'), detail: response.data.message });
            fetchIdeas();
        } catch (error: any) {
            toast.current?.show({ severity: 'error', summary: t('myIdeas.error'), detail: t('myIdeas.genericError') });
        }
    };

    const confirmWithdraw = (idea: any) => {
        setSelectedIdea(idea);
        setWithdrawDialogVisible(true);
    };

    const handleWithdraw = async () => {
        if (!selectedIdea) return;
        setIsWithdrawing(true);
        try {
            const response = await api.post(`/ideas/${selectedIdea.id}/withdraw`);
            toast.current?.show({ severity: 'success', summary: t('myIdeas.success'), detail: response.data.message });
            setWithdrawDialogVisible(false);
            fetchIdeas();
        } catch (error: any) {
            toast.current?.show({ severity: 'error', summary: t('myIdeas.error'), detail: t('myIdeas.genericError') });
        } finally {
            setIsWithdrawing(false);
        }
    };

    const formatDate = (dateStr: string) => {
        const d = new Date(dateStr);
        return d.toLocaleString('tr-TR', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' });
    };

    const statusBodyTemplate = (rowData: any) => {
        switch (rowData.status) {
            case 'Draft':
                return <span className="bg-gray-100 text-gray-700 px-2 py-1 border-round text-sm font-semibold border-1 border-gray-300 white-space-nowrap"><i className="pi pi-pencil mr-1"></i> {t('myIdeas.statusDraft')}</span>;
            case 'Implemented':
                return <span className="bg-orange-100 text-orange-700 px-2 py-1 border-round text-sm font-semibold border-1 border-orange-300 white-space-nowrap"><i className="pi pi-star-fill mr-1"></i> {t('myIdeas.statusGlittering')}</span>;
            case 'Approved':
                return <span className="bg-green-100 text-green-800 px-2 py-1 border-round text-sm font-semibold border-1 border-green-300 white-space-nowrap">{t('myIdeas.statusApproved')}</span>;
            case 'Rejected':
                return <span className="bg-red-100 text-red-800 px-2 py-1 border-round text-sm font-semibold border-1 border-red-300 white-space-nowrap">{t('myIdeas.statusRejected')}</span>;
            case 'Withdrawn':
                return <span className="bg-gray-100 text-gray-700 px-2 py-1 border-round text-sm font-semibold border-1 border-gray-300 white-space-nowrap">{t('myIdeas.statusWithdrawn')}</span>;
            default: // Pending or UnderReview
                return <span className="bg-orange-100 text-orange-800 px-2 py-1 border-round text-sm font-semibold border-1 border-orange-300 white-space-nowrap">{t('myIdeas.statusPending')}</span>;
        }
    };

    const actionBodyTemplate = (rowData: any) => {
        if (rowData.status === 'Draft') {
            return (
                <div className="flex gap-2 justify-content-end">
                    <Button onClick={() => router.push(`/ideas/edit/${rowData.id}`)} icon="pi pi-pencil" label={t('myIdeas.btnEdit')} severity="secondary" outlined size="small" className="py-1 px-2" />
                    <Button onClick={() => handlePublish(rowData.id)} icon="pi pi-send" label={t('myIdeas.btnSubmit')} severity="success" size="small" className="py-1 px-2" />
                    <Button onClick={() => confirmWithdraw(rowData)} icon="pi pi-refresh" label={t('myIdeas.btnWithdraw')} severity="danger" outlined size="small" className="py-1 px-2" />
                </div>
            );
        } else if (rowData.status === 'Pending') {
            return (
                <div className="flex gap-2 justify-content-end">
                    <Button onClick={() => router.push(`/ideas/edit/${rowData.id}`)} icon="pi pi-pencil" label={t('myIdeas.btnEdit')} severity="secondary" outlined size="small" className="py-1 px-2" />
                    <Button onClick={() => confirmWithdraw(rowData)} icon="pi pi-refresh" label={t('myIdeas.btnWithdraw')} severity="danger" outlined size="small" className="py-1 px-2" />
                    <Button onClick={() => router.push(`/ideas/details/${rowData.id}`)} label={t('myIdeas.btnDetail')} severity="secondary" outlined size="small" className="py-1 px-2" />
                </div>
            );
        } else {
            return (
                <div className="flex justify-content-end">
                    <Button onClick={() => router.push(`/ideas/details/${rowData.id}`)} label={t('myIdeas.btnViewDetail')} severity="secondary" outlined size="small" className="py-1 px-2" />
                </div>
            );
        }
    };

    const titleBodyTemplate = (rowData: any) => (
        <span className="font-semibold" style={{ color: 'var(--text-color)' }}>{rowData.title}</span>
    );

    return (
        <div className="grid">
            <Toast ref={toast} />
            <div className="col-12">
                <div className="flex flex-wrap justify-content-between align-items-center mb-4 gap-3">
                    <div>
                        <h3 className="font-bold mb-1" style={{ color: 'var(--text-color)', letterSpacing: '-0.02em' }}>{t('myIdeas.pageTitle')}</h3>
                        <p className="m-0 text-600" style={{ fontSize: '0.95rem' }}>{t('myIdeas.pageDesc')}</p>
                    </div>
                    <div className="flex flex-wrap align-items-center gap-2">
                        <Dropdown value={sort} options={sortOptions} onChange={(e) => setSort(e.value)} className="w-auto p-inputtext-sm" />
                        <div className="p-buttonset">
                            <Button label={t('myIdeas.filterAll')} severity={filter === 'all' ? 'success' : 'secondary'} outlined={filter !== 'all'} onClick={() => setFilter('all')} size="small" />
                            <Button label={t('myIdeas.filterDraft')} severity={filter === 'draft' ? 'success' : 'secondary'} outlined={filter !== 'draft'} onClick={() => setFilter('draft')} size="small" />
                            <Button label={t('myIdeas.filterPending')} severity={filter === 'pending' ? 'success' : 'secondary'} outlined={filter !== 'pending'} onClick={() => setFilter('pending')} size="small" />
                            <Button label={t('myIdeas.filterApproved')} severity={filter === 'approved' ? 'success' : 'secondary'} outlined={filter !== 'approved'} onClick={() => setFilter('approved')} size="small" />
                            <Button label={t('myIdeas.filterRejected')} severity={filter === 'rejected' ? 'success' : 'secondary'} outlined={filter !== 'rejected'} onClick={() => setFilter('rejected')} size="small" />
                        </div>
                        <Link href="/ideas/create">
                            <Button label={t('myIdeas.newIdeaBtn')} icon="pi pi-plus" severity="success" size="small" />
                        </Link>
                    </div>
                </div>

                <div className="card p-0 overflow-hidden shadow-2 border-round-xl">
                    <DataTable value={ideas} loading={loading} emptyMessage={t('myIdeas.emptyMessage')}>
                        <Column field="title" header={t('myIdeas.colTitle')} body={titleBodyTemplate} ></Column>
                        <Column field="categoryName" header={t('myIdeas.colCategory')} body={(rowData) => <span className="text-600 white-space-nowrap">{t(rowData.categoryName === 'Hizmet Geliştirme' ? 'category.service' : rowData.categoryName === 'Ürün İyileştirme' ? 'category.product' : rowData.categoryName === 'Süreç & Verimlilik' ? 'category.process' : rowData.categoryName ? rowData.categoryName : 'category.other')}</span>}></Column>
                        <Column field="createdAt" header={t('myIdeas.colDate')} body={(rowData) => <span className="text-500 text-sm">{formatDate(rowData.createdAt)}</span>}></Column>
                        <Column header={t('myIdeas.colStatus')} body={statusBodyTemplate}></Column>
                        <Column header={t('myIdeas.colActions')} body={actionBodyTemplate} align="right" ></Column>
                    </DataTable>
                </div>
            </div>

            <Dialog visible={withdrawDialogVisible} onHide={() => setWithdrawDialogVisible(false)} style={{ width: '450px' }} header={false} closable={false} className="p-0 border-round-xl">
                <div className="text-center pt-4 pb-2 px-4">
                    <span className="text-4xl block mb-2">✨</span>
                    <h5 className="font-bold mb-2 text-900">{t('myIdeas.withdrawTitle')}</h5>
                    <p className="text-600 mb-4" style={{ fontSize: '0.9rem' }}>
                        {t('myIdeas.withdrawConfirmText1')}<strong className="text-900">{selectedIdea?.title}</strong>{t('myIdeas.withdrawConfirmText2')}
                    </p>
                    <div className="p-3 border-round-md text-left text-sm mb-4 bg-gray-50 border-1 border-gray-200 text-600">
                        <i className="pi pi-star-fill text-yellow-500 mr-2"></i> {t('myIdeas.withdrawInfoTitle')} <br />
                        <small className="text-500 block mt-2"><em>{t('myIdeas.withdrawInfoDesc')}</em></small>
                    </div>
                    <div className="flex justify-content-center gap-2">
                        <Button label={t('myIdeas.btnCancelWithdraw')} severity="secondary" outlined onClick={() => setWithdrawDialogVisible(false)} className="px-3 py-2 text-sm" disabled={isWithdrawing} />
                        <Button label={t('myIdeas.btnConfirmWithdraw')} severity="danger" onClick={handleWithdraw} className="px-3 py-2 text-sm font-medium" loading={isWithdrawing} />
                    </div>
                </div>
            </Dialog>
        </div>
    );
};

export default MyIdeasPage;
