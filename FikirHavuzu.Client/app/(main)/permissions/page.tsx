'use client';
import React, { useEffect, useState, useRef } from 'react';
import { useRouter } from 'next/navigation';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Toast } from 'primereact/toast';
import { Checkbox } from 'primereact/checkbox';
import api from '../../../utils/api';
import { useTranslation } from 'react-i18next';

const PermissionManagePage = () => {
    const { t } = useTranslation();
    const router = useRouter();
    const toast = useRef<Toast>(null);
    const [users, setUsers] = useState<any[]>([]);
    const [permissions, setPermissions] = useState<any[]>([]);
    const [loading, setLoading] = useState(true);
    const [currentUserId, setCurrentUserId] = useState(0);

    // Track unsaved changes: map of userId -> new permissionIds
    const [pendingChanges, setPendingChanges] = useState<{ [key: number]: number[] }>({});
    const [saving, setSaving] = useState<{ [key: number]: boolean }>({});

    const fetchPermissions = async () => {
        setLoading(true);
        try {
            const response = await api.get('/permissions');
            setUsers(response.data.users);
            setPermissions(response.data.permissions);
            setCurrentUserId(response.data.currentUserId);
            setPendingChanges({});
        } catch (error: any) {
            console.error("Yetkiler yüklenirken hata:", error);
            toast.current?.show({ severity: 'error', summary: t('permissions.unauthorizedTitle'), detail: t('permissions.unauthorizedDetail') });
            if (error.response?.status === 403) {
                router.push('/');
            }
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchPermissions();
    }, []);

    const onCheckboxChange = (userId: number, permId: number, checked: boolean) => {
        const currentUserData = users.find(u => u.id === userId);
        if (!currentUserData) return;

        // Use pending changes if exist, otherwise use original
        const currentPerms = pendingChanges[userId] || currentUserData.permissionIds;
        
        let newPerms;
        if (checked) {
            newPerms = [...currentPerms, permId];
        } else {
            newPerms = currentPerms.filter((id: number) => id !== permId);
        }

        setPendingChanges({
            ...pendingChanges,
            [userId]: newPerms
        });
    };

    const handleUpdate = async (userId: number) => {
        const newPerms = pendingChanges[userId];
        if (!newPerms) return; // No changes

        setSaving({ ...saving, [userId]: true });
        try {
            const response = await api.put(`/permissions/${userId}`, { permissionIds: newPerms });
            toast.current?.show({ severity: 'success', summary: t('permissions.success'), detail: t('permissions.updateSuccess') });
            
            // Update local state to reflect saved changes
            const updatedUsers = users.map(u => {
                if (u.id === userId) {
                    return { ...u, permissionIds: newPerms };
                }
                return u;
            });
            setUsers(updatedUsers);
            
            // Remove from pending
            const newPending = { ...pendingChanges };
            delete newPending[userId];
            setPendingChanges(newPending);
            
        } catch (error: any) {
            console.error("Güncelleme hatası", error);
            toast.current?.show({ severity: 'error', summary: t('permissions.errorTitle'), detail: t('permissions.updateErrorDetail') });
        } finally {
            setSaving({ ...saving, [userId]: false });
        }
    };

    const userInfoBodyTemplate = (rowData: any) => {
        return (
            <div>
                <div className="font-semibold text-900">
                    {rowData.firstName} {rowData.lastName}
                    {rowData.id === currentUserId && <span className="bg-gray-100 text-gray-600 px-1 py-0 ml-1 border-round text-xs font-semibold">({t('permissions.you')})</span>}
                </div>
                <div className="text-500 font-mono text-xs">{rowData.registrationNumber}</div>
                <div className="text-500 text-xs mt-1">{rowData.email}</div>
            </div>
        );
    };

    const permissionBodyTemplate = (rowData: any, perm: any) => {
        const currentPerms = pendingChanges[rowData.id] || rowData.permissionIds;
        const hasPerm = currentPerms.includes(perm.id);
        const isOwnPermissionManagement = (rowData.id === currentUserId && perm.name === "PermissionManagement");

        return (
            <div className="flex justify-content-center">
                <Checkbox 
                    onChange={e => onCheckboxChange(rowData.id, perm.id, e.checked || false)} 
                    checked={hasPerm} 
                    disabled={isOwnPermissionManagement || saving[rowData.id]} 
                    tooltip={isOwnPermissionManagement ? t("permissions.cannotRemoveOwnPerm") : ""}
                />
            </div>
        );
    };

    const actionBodyTemplate = (rowData: any) => {
        const hasChanges = !!pendingChanges[rowData.id];
        const isSaving = saving[rowData.id];
        
        return (
            <div className="flex justify-content-end">
                <Button 
                    label={t("permissions.update")} 
                    icon={isSaving ? "pi pi-spin pi-spinner" : "pi pi-check"} 
                    severity={hasChanges ? "success" : "secondary"} 
                    size="small"
                    disabled={!hasChanges || isSaving}
                    onClick={() => handleUpdate(rowData.id)}
                    className="py-1 px-3"
                />
            </div>
        );
    };

    return (
        <div className="grid">
            <Toast ref={toast} />
            <div className="col-12">
                <div className="mb-4">
                    <h3 className="font-bold mb-1" style={{ color: 'var(--text-color)', letterSpacing: '-0.02em' }}>{t('permissions.pageTitle')}</h3>
                    <p className="m-0 text-600" style={{ fontSize: '0.95rem' }}>{t('permissions.pageDesc')}</p>
                </div>

                <div className="card p-0 overflow-hidden shadow-2 border-round-xl">
                    <DataTable value={users} loading={loading} emptyMessage={t("permissions.noRecords")}>
                        <Column header={t("permissions.regNoAndEmp")} body={userInfoBodyTemplate} ></Column>
                        
                        {permissions.map((perm) => (
                            <Column 
                                key={perm.id} 
                                header={<div className="text-center w-full">{t('permName.' + perm.name, perm.name)}</div>} 
                                body={(rowData) => permissionBodyTemplate(rowData, perm)} 
                                align="center"
                            ></Column>
                        ))}
                        
                        <Column header={t("permissions.action")} body={actionBodyTemplate} align="right"></Column>
                    </DataTable>
                </div>
            </div>
        </div>
    );
};

export default PermissionManagePage;
