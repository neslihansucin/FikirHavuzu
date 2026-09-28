'use client';
import React, { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useRouter } from 'next/navigation';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dropdown } from 'primereact/dropdown';
import api from '../../../../utils/api';

const WithdrawnIdeasPage = () => {
    const { t } = useTranslation();
    const router = useRouter();
    const [ideas, setIdeas] = useState([]);
    const [loading, setLoading] = useState(true);
    
    // Sort
    const [sort, setSort] = useState('date_desc');

    const sortOptions = [
        { label: t('withdrawn.sortNewest'), value: 'date_desc' },
        { label: t('withdrawn.sortOldest'), value: 'date_asc' },
        { label: t('withdrawn.sortTitleAZ'), value: 'title_asc' },
        { label: t('withdrawn.sortTitleZA'), value: 'title_desc' }
    ];

    const fetchIdeas = async () => {
        setLoading(true);
        try {
            const response = await api.get(`/ideas/withdrawn?sort=${sort}`);
            setIdeas(response.data);
        } catch (error: any) {
            console.error("Geri çekilen fikirler yüklenirken hata:", error);
            if (error.response?.status === 403) {
                router.push('/');
            }
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchIdeas();
    }, [sort]);

    const formatDate = (dateStr: string) => {
        const d = new Date(dateStr);
        const lang = typeof window !== 'undefined' ? (localStorage.getItem('i18nextLng') || 'tr') : 'tr';
        return d.toLocaleDateString(lang === 'en' ? 'en-US' : 'tr-TR', { day: '2-digit', month: '2-digit', year: 'numeric' });
    };

    const authorBodyTemplate = (rowData: any) => {
        return (
            <div>
                <div className="font-semibold text-900">{rowData.authorName}</div>
                {/* We don't have registrationNumber in this DTO, so we skip it to match API */}
            </div>
        );
    };

    const titleBodyTemplate = (rowData: any) => (
        <span className="font-semibold text-900">{rowData.title}</span>
    );

    const actionBodyTemplate = (rowData: any) => {
        return (
            <div className="flex justify-content-end">
                <Button onClick={() => router.push(`/ideas/details/${rowData.id}`)} label={t("withdrawn.btnReview")} severity="secondary" outlined size="small" className="py-1 px-3" />
            </div>
        );
    };

    return (
        <div className="grid">
            <div className="col-12">
                <div className="flex flex-wrap justify-content-between align-items-center mb-4 gap-3">
                    <div>
                        <h3 className="font-bold mb-1" style={{ color: 'var(--text-color)', letterSpacing: '-0.02em' }}>📦 {t("withdrawn.title")}</h3>
                        <p className="m-0 text-600" style={{ fontSize: '0.95rem' }}>{t("withdrawn.desc")}</p>
                    </div>
                    <div>
                        <Dropdown value={sort} options={sortOptions} onChange={(e) => setSort(e.value)} className="w-auto p-inputtext-sm" />
                    </div>
                </div>

                <div className="card p-0 overflow-hidden shadow-2 border-round-xl">
                    <DataTable value={ideas} loading={loading} emptyMessage={t("withdrawn.emptyMessage")}>
                        <Column header={t('withdrawn.colAuthor')} body={authorBodyTemplate} ></Column>
                        <Column field="title" header={t("withdrawn.colTitle")} body={titleBodyTemplate} ></Column>
                        <Column field="categoryName" header={t('withdrawn.colCategory')} body={(rowData) => <span className="text-600 white-space-nowrap">{t(rowData.categoryName === 'Hizmet Geliştirme' ? 'category.service' : rowData.categoryName === 'Ürün İyileştirme' ? 'category.product' : rowData.categoryName === 'Süreç & Verimlilik' ? 'category.process' : rowData.categoryName ? rowData.categoryName : 'category.other')}</span>}></Column>
                        <Column field="createdAt" header={t("withdrawn.colDate")} body={(rowData) => <span className="text-500 text-sm">{formatDate(rowData.createdAt)}</span>}></Column>
                        <Column header={t('withdrawn.colStatus')} body={() => <span className="bg-gray-100 text-gray-700 px-2 py-1 border-round text-sm font-semibold border-1 border-gray-300 white-space-nowrap">{t("withdrawn.statusWithdrawn")}</span>}></Column>
                        <Column header={t("withdrawn.colActions")} body={actionBodyTemplate} align="right" ></Column>
                    </DataTable>
                </div>
            </div>
        </div>
    );
};

export default WithdrawnIdeasPage;
