'use client';
import React, { useEffect, useState, useRef } from 'react';
import { useRouter } from 'next/navigation';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Toast } from 'primereact/toast';
import { Avatar } from 'primereact/avatar';
import api from '../../../utils/api';
import Link from 'next/link';
import { useTranslation } from 'react-i18next';

const UserListPage = () => {
    const { t } = useTranslation();
    const router = useRouter();
    const toast = useRef<Toast>(null);
    const [users, setUsers] = useState<any[]>([]);
    const [loading, setLoading] = useState(true);
    const [currentUserId, setCurrentUserId] = useState(0);
    const [isSuperAdmin, setIsSuperAdmin] = useState(false);

    const fetchUsers = async () => {
        setLoading(true);
        try {
            const response = await api.get('/users');
            setUsers(response.data.users);
            setCurrentUserId(response.data.currentUserId);
            setIsSuperAdmin(response.data.isSuperAdmin);
        } catch (error: any) {
            console.error("Kullanıcılar yüklenirken hata:", error);
            toast.current?.show({ severity: 'error', summary: t('users.unauthorizedTitle'), detail: t('users.unauthorizedDetail') });
            if (error.response?.status === 403) {
                router.push('/');
            }
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchUsers();
    }, []);

    const activeContributorsCount = users.filter(u => u.stats.total > 0).length;
    const approvedContributorsCount = users.filter(u => u.stats.approved > 0 || u.stats.implemented > 0).length;
    const totalOrganizationPoints = users.reduce((sum, u) => sum + u.stats.points, 0);
    const participationRate = users.length > 0 ? Math.round((activeContributorsCount / users.length) * 100) : 0;

    const getInitials = (firstName: string, lastName: string) => {
        const first = firstName ? firstName[0] : '';
        const last = lastName ? lastName[0] : '';
        return (first + last).toUpperCase() || 'U';
    };

    const userInfoBodyTemplate = (rowData: any) => {
        return (
            <div className="flex align-items-center gap-2">
                <Avatar 
                    image={rowData.profilePictureUrl ? `${api.defaults.baseURL?.replace('/api', '') || ''}${rowData.profilePictureUrl}?t=${Date.now()}` : undefined} 
                    label={!rowData.profilePictureUrl ? getInitials(rowData.firstName, rowData.lastName) : undefined} 
                    shape="circle" size="large" className="bg-blue-100 text-blue-600 font-bold flex-shrink-0" 
                />
                <div>
                    <div className="font-semibold text-900 line-height-2">
                        {rowData.firstName} {rowData.lastName}
                        {rowData.id === currentUserId && <span className="white-space-nowrap bg-gray-100 text-gray-600 px-1 py-0 ml-1 border-round text-xs font-semibold">({t('users.you')})</span>}
                    </div>
                    <div className="text-500" style={{ fontSize: '0.75rem' }}>{rowData.email}</div>
                </div>
            </div>
        );
    };

    const roleBodyTemplate = (rowData: any) => {
        const perms = rowData.permissions || [];
        const isUserSuperAdmin = perms.some((p: any) => p.name.includes("PermissionManagement") || p.name.includes("Yetki"));
        const isUserHR = perms.some((p: any) => p.name.includes("UserManagement") || p.name.includes("Kullanıcı"));
        const isUserEvaluator = perms.some((p: any) => p.name.includes("IdeaEvaluation") || p.name.includes("Değerlendirme"));

        if (isUserSuperAdmin) {
            return <span className="white-space-nowrap badge font-bold px-2 py-1 border-round text-xs text-white" style={{ backgroundColor: '#1E293B' }}><i className="pi pi-shield mr-1"></i> {t('users.sysAdmin')}</span>;
        } else if (isUserHR && isUserEvaluator) {
            return (
                <div className="flex flex-column gap-1">
                    <span className="white-space-nowrap bg-green-100 text-green-800 px-2 py-1 border-round text-xs font-semibold">{t('users.hr')}</span>
                    <span className="white-space-nowrap bg-orange-100 text-orange-800 px-2 py-1 border-round text-xs font-semibold">{t('users.coordinator')}</span>
                </div>
            );
        } else if (isUserHR) {
            return <span className="white-space-nowrap bg-green-100 text-green-800 px-2 py-1 border-round text-xs font-semibold"><i className="pi pi-users mr-1"></i> {t('users.hr')}</span>;
        } else if (isUserEvaluator) {
            return <span className="white-space-nowrap bg-orange-100 text-orange-800 px-2 py-1 border-round text-xs font-semibold"><i className="pi pi-check-circle mr-1"></i> {t('users.coordinator')}</span>;
        } else {
            return <span className="white-space-nowrap bg-gray-100 text-gray-700 px-2 py-1 border-round text-xs font-semibold">{t('users.staff')}</span>;
        }
    };

    const statusBodyTemplate = (rowData: any) => {
        if (rowData.isActive) {
            return <span className="white-space-nowrap bg-green-100 text-green-800 px-2 py-1 border-round text-xs font-semibold">{t('users.active')}</span>;
        } else {
            return <span className="white-space-nowrap bg-red-100 text-red-800 px-2 py-1 border-round text-xs font-semibold">{t('users.passive')}</span>;
        }
    };

    const actionBodyTemplate = (rowData: any) => {
        const perms = rowData.permissions || [];
        const isUserSuperAdmin = perms.some((p: any) => p.name.includes("PermissionManagement") || p.name.includes("Yetki"));
        const canEdit = isSuperAdmin || !isUserSuperAdmin;

        if (canEdit) {
            return <Button label={t("users.edit")} severity="secondary" outlined size="small" className="py-1 px-3" onClick={() => router.push(`/users/edit/${rowData.id}`)} />;
        } else {
            return <span className="white-space-nowrap bg-gray-100 text-gray-600 px-2 py-1 border-round text-xs font-semibold flex align-items-center justify-content-center" title="Bu hesap sadece {t('users.sysAdmin')} tarafından düzenlenebilir."><i className="pi pi-lock mr-1"></i> {t('users.protected')}</span>;
        }
    };

    return (
        <div className="grid">
            <Toast ref={toast} />
            <div className="col-12">
                <div className="flex flex-wrap justify-content-between align-items-center mb-4 gap-3">
                    <div>
                        <h3 className="font-bold mb-1" style={{ color: 'var(--text-color)', letterSpacing: '-0.02em' }}>{t('users.pageTitle')}</h3>
                        <p className="m-0 text-600" style={{ fontSize: '0.95rem' }}>{t('users.pageDesc')}</p>
                    </div>
                    <div>
                        <Link href="/users/create">
                            <Button label={t("users.addEmployee")} icon="pi pi-user-plus" severity="success" />
                        </Link>
                    </div>
                </div>

                {/* HR Innovation Analytics Bar */}
                <div className="grid mb-4">
                    <div className="col-12 md:col-6 xl:col-3">
                        <div className="card mb-0 shadow-1 border-round-xl">
                            <div className="flex justify-content-between mb-3">
                                <div>
                                    <span className="block text-500 font-semibold text-xs uppercase mb-3" style={{ letterSpacing: '0.04em' }}>{t('users.totalEmployees')}</span>
                                    <div className="text-900 font-bold text-xl">{users.length}</div>
                                </div>
                                <div className="flex align-items-center justify-content-center bg-blue-100 border-round" style={{ width: '2.5rem', height: '2.5rem' }}>
                                    <i className="pi pi-users text-blue-500 text-xl" />
                                </div>
                            </div>
                            <span className="text-500 text-xs">{t('users.registeredStaff')}</span>
                        </div>
                    </div>
                    <div className="col-12 md:col-6 xl:col-3">
                        <div className="card mb-0 shadow-1 border-round-xl">
                            <div className="flex justify-content-between mb-3">
                                <div>
                                    <span className="block text-500 font-semibold text-xs uppercase mb-3" style={{ letterSpacing: '0.04em' }}>{t('users.activeContributors')}</span>
                                    <div className="text-blue-500 font-bold text-xl">{activeContributorsCount}</div>
                                </div>
                                <div className="flex align-items-center justify-content-center bg-blue-100 border-round" style={{ width: '2.5rem', height: '2.5rem' }}>
                                    <i className="pi pi-star text-blue-500 text-xl" />
                                </div>
                            </div>
                            <span className="text-500 text-xs">{t('users.participationRate', { rate: participationRate })}</span>
                        </div>
                    </div>
                    <div className="col-12 md:col-6 xl:col-3">
                        <div className="card mb-0 shadow-1 border-round-xl">
                            <div className="flex justify-content-between mb-3">
                                <div>
                                    <span className="block text-500 font-semibold text-xs uppercase mb-3" style={{ letterSpacing: '0.04em' }}>{t('users.positiveIdeas')}</span>
                                    <div className="text-green-500 font-bold text-xl">{approvedContributorsCount}</div>
                                </div>
                                <div className="flex align-items-center justify-content-center bg-green-100 border-round" style={{ width: '2.5rem', height: '2.5rem' }}>
                                    <i className="pi pi-check-circle text-green-500 text-xl" />
                                </div>
                            </div>
                            <span className="text-500 text-xs">{t('users.approvedStaff')}</span>
                        </div>
                    </div>
                    <div className="col-12 md:col-6 xl:col-3">
                        <div className="card mb-0 shadow-1 border-round-xl">
                            <div className="flex justify-content-between mb-3">
                                <div>
                                    <span className="block text-500 font-semibold text-xs uppercase mb-3" style={{ letterSpacing: '0.04em' }}>{t('users.corpInnovationScore')}</span>
                                    <div className="text-orange-500 font-bold text-xl">{totalOrganizationPoints}</div>
                                </div>
                                <div className="flex align-items-center justify-content-center bg-orange-100 border-round" style={{ width: '2.5rem', height: '2.5rem' }}>
                                    <i className="pi pi-trophy text-orange-500 text-xl" />
                                </div>
                            </div>
                            <span className="text-500 text-xs">{t('users.totalValueGenerated')}</span>
                        </div>
                    </div>
                </div>

                <div className="card p-0 overflow-hidden shadow-2 border-round-xl">
                    <DataTable value={users} loading={loading} emptyMessage={t("users.noRecords")} size="small">
                        <Column field="registrationNumber" header={t("users.regNo")} body={(rowData) => <span className="font-mono font-bold text-900">{rowData.registrationNumber}</span>}></Column>
                        <Column header={t("users.empInfo")} body={userInfoBodyTemplate}></Column>
                        <Column header={t("users.roleTitle")} body={roleBodyTemplate}></Column>
                        <Column header={t("users.ideaCount")} body={(rowData) => <span className="font-semibold">{rowData.stats?.total > 0 ? <span className="white-space-nowrap bg-gray-100 text-gray-800 px-2 py-1 border-round text-sm">{rowData.stats.total}</span> : <span className="text-500">-</span>}</span>} align="center"></Column>
                        <Column header={t("users.positiveIdeaCount")} body={(rowData) => <span className="font-semibold">{rowData.stats?.approved > 0 ? <span className="white-space-nowrap bg-green-100 text-green-800 px-2 py-1 border-round text-sm">{rowData.stats.approved}</span> : <span className="text-500">-</span>}</span>} align="center"></Column>
                        <Column header={t("users.innovationScore")} body={(rowData) => <span className="font-bold">{rowData.stats?.points > 0 ? <span className="white-space-nowrap bg-yellow-100 text-yellow-800 px-2 py-1 border-round text-sm border-1 border-yellow-400">{rowData.stats.points} {t('users.points')}</span> : <span className="text-500">0</span>}</span>} align="center"></Column>
                        <Column header={t("users.status")} body={statusBodyTemplate}></Column>
                        <Column header={t("users.actions")} body={actionBodyTemplate} align="right"></Column>
                    </DataTable>
                </div>
            </div>
        </div>
    );
};

export default UserListPage;
