'use client';
import React, { useState, useEffect, useRef } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { InputText } from 'primereact/inputtext';
import { Button } from 'primereact/button';
import { Toast } from 'primereact/toast';
import { InputSwitch } from 'primereact/inputswitch';
import { Checkbox } from 'primereact/checkbox';
import api from '../../../../../utils/api';
import { useTranslation } from 'react-i18next';

const EditUserPage = () => {
    const { t } = useTranslation();
    const router = useRouter();
    const params = useParams();
    const userId = params.id;
    const toast = useRef<Toast>(null);
    
    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
    const [tcNo, setTcNo] = useState('');
    const [email, setEmail] = useState('');
    const [phoneNumber, setPhoneNumber] = useState('');
    const [registrationNumber, setRegistrationNumber] = useState('');
    const [isActive, setIsActive] = useState(true);
    const [userProfilePictureUrl, setUserProfilePictureUrl] = useState('');
    
    const [stats, setStats] = useState({ total: 0, approved: 0, implemented: 0, points: 0 });
    const [isSuperAdmin, setIsSuperAdmin] = useState(false);
    const [allPermissions, setAllPermissions] = useState<any[]>([]);
    const [userPermissionIds, setUserPermissionIds] = useState<number[]>([]);
    
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [loading, setLoading] = useState(true);
    const [isMe, setIsMe] = useState(false);

    useEffect(() => {
        const fetchUserData = async () => {
            try {
                const res = await api.get(`/users/${userId}`);
                const data = res.data;
                
                setFirstName(data.firstName);
                setLastName(data.lastName);
                setTcNo(data.tcNo || '');
                setEmail(data.email);
                setPhoneNumber(data.phoneNumber || '');
                setRegistrationNumber(data.registrationNumber);
                setIsActive(data.isActive);
                setUserProfilePictureUrl(data.profilePictureUrl || '');
                
                setStats(data.stats);
                setIsSuperAdmin(data.isSuperAdmin);
                setAllPermissions(data.allPermissions);
                setUserPermissionIds(data.permissionIds);
                
                const userStr = localStorage.getItem('user');
                if (userStr) {
                    const localUser = JSON.parse(userStr);
                    setIsMe(localUser.id === parseInt(userId as string));
                }
            } catch (error: any) {
                console.error("Kullanıcı bilgileri alınamadı", error);
                toast.current?.show({ severity: 'error', summary: t('usersEdit.errorDetail'), detail: t('usersEdit.errorLoadDetail') });
                if (error.response?.status === 403) router.push('/');
            } finally {
                setLoading(false);
            }
        };
        
        if (userId) fetchUserData();
    }, [userId]);

    const handlePermissionChange = (e: any, permId: number) => {
        let _permissionIds = [...userPermissionIds];
        if (e.checked)
            _permissionIds.push(permId);
        else
            _permissionIds = _permissionIds.filter(id => id !== permId);
        setUserPermissionIds(_permissionIds);
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        
        if (!firstName || !lastName || !tcNo || !email || !registrationNumber) {
            toast.current?.show({ severity: 'warn', summary: t('usersEdit.missingInfo'), detail: t('usersEdit.missingInfoDetail') });
            return;
        }

        if (tcNo.length !== 11) {
            toast.current?.show({ severity: 'warn', summary: t('usersEdit.invalidTc'), detail: t('usersEdit.invalidTcDetail') });
            return;
        }

        setIsSubmitting(true);
        try {
            const response = await api.put(`/users/${userId}`, {
                firstName,
                lastName,
                tcNo,
                email,
                phoneNumber,
                registrationNumber,
                isActive,
                permissionIds: userPermissionIds
            });
            
            toast.current?.show({ severity: 'success', summary: t('usersEdit.success'), detail: t('usersEdit.updateSuccess') });
            setTimeout(() => {
                router.push('/users');
            }, 1500);
        } catch (error: any) {
            console.error("Çalışan güncellenirken hata:", error);
            toast.current?.show({ severity: 'error', summary: t('usersEdit.errorDetail'), detail: t('usersEdit.updateErrorDetail') });
            setIsSubmitting(false);
        }
    };

    if (loading) return <div>{t('usersEdit.loading')}</div>;

    return (
        <div className="grid">
            <Toast ref={toast} />
            
            <div className="col-12">
                <div className="card p-4 shadow-2 border-round-xl">
                    <div className="mb-4 flex align-items-center gap-4">
                        <div className="relative inline-block">
                            {userProfilePictureUrl ? (
                                <img src={`${api.defaults.baseURL?.replace('/api', '') || ''}${userProfilePictureUrl}?t=${Date.now()}`} alt="Avatar" className="border-circle border-2 border-primary" style={{ width: '80px', height: '80px', objectFit: 'cover' }} />
                            ) : (
                                <div className="border-circle flex align-items-center justify-content-center border-2 border-primary" style={{ width: '80px', height: '80px', backgroundColor: 'var(--primary-100)', color: 'var(--primary-color)', fontSize: '2rem', fontWeight: 'bold' }}>
                                    {firstName?.[0]}{lastName?.[0]}
                                </div>
                            )}
                        </div>
                        <div>
                            <h4 className="font-bold mb-1 text-900" style={{ letterSpacing: '-0.01em' }}>{t('usersEdit.pageTitle')}</h4>
                            <p className="m-0 text-600 text-sm">
                                <strong className="text-800">{firstName} {lastName}</strong>
                                {isMe && <span className="bg-gray-100 text-gray-600 px-1 py-0 mx-1 border-round text-xs font-semibold">({t('usersEdit.you')})</span>}
                                ({registrationNumber}) {t('usersEdit.userAccountDetails')}
                            </p>
                        </div>
                    </div>

                    {/* Employee Innovation & Productivity Widget */}
                    <div className="p-3 border-round-xl mb-4 bg-gray-50 border-1 border-gray-200">
                        <div className="flex align-items-center justify-content-between mb-3">
                            <span className="text-500 font-semibold text-xs uppercase" style={{ letterSpacing: '0.04em' }}>{t('usersEdit.innovationSummary')}</span>
                            {stats.points > 0 ? (
                                <span className="bg-yellow-100 text-yellow-800 px-2 py-1 border-round-md text-xs font-bold border-1 border-yellow-400 white-space-nowrap">{stats.points} {t('usersEdit.points')}</span>
                            ) : (
                                <span className="bg-gray-200 text-gray-600 px-2 py-1 border-round-md text-xs font-semibold white-space-nowrap">{t('usersEdit.zeroPoints')}</span>
                            )}
                        </div>
                        <div className="grid text-center">
                            <div className="col-4">
                                <div className="p-2 bg-white border-round-md border-1 border-gray-200">
                                    <span className="text-500 block text-xs mb-1">{t('usersEdit.totalIdeas')}</span>
                                    <span className="font-bold text-lg text-900">{stats.total}</span>
                                </div>
                            </div>
                            <div className="col-4">
                                <div className="p-2 bg-white border-round-md border-1 border-gray-200">
                                    <span className="text-500 block text-xs mb-1">{t('usersEdit.approvedIdeas')}</span>
                                    <span className="font-bold text-lg text-green-500">{stats.approved}</span>
                                </div>
                            </div>
                            <div className="col-4">
                                <div className="p-2 bg-white border-round-md border-1 border-gray-200">
                                    <span className="text-500 block text-xs mb-1">{t('usersEdit.implementedIdeas')}</span>
                                    <span className="font-bold text-lg text-orange-500">{stats.implemented}</span>
                                </div>
                            </div>
                        </div>
                    </div>

                    <form onSubmit={handleSubmit}>
                        <div className="formgrid grid">
                            <div className="col-12 md:col-6 mb-3">
                                <label className="text-700 text-sm font-semibold mb-1 block">{t('usersEdit.firstName')}</label>
                                <InputText 
                                    value={firstName} 
                                    onChange={(e) => setFirstName(e.target.value.replace(/[^a-zA-ZğüşıöçĞÜŞİÖÇ\s]/g, ''))} 
                                    className="w-full p-inputtext-sm" 
                                    required
                                            onInvalid={(e: any) => e.target.setCustomValidity(t('validation.required'))}
                                            onInput={(e: any) => e.target.setCustomValidity('')} 
                                    disabled={isSubmitting}
                                />
                            </div>
                            <div className="col-12 md:col-6 mb-3">
                                <label className="text-700 text-sm font-semibold mb-1 block">{t('usersEdit.lastName')}</label>
                                <InputText 
                                    value={lastName} 
                                    onChange={(e) => setLastName(e.target.value.replace(/[^a-zA-ZğüşıöçĞÜŞİÖÇ\s]/g, ''))} 
                                    className="w-full p-inputtext-sm" 
                                    required
                                            onInvalid={(e: any) => e.target.setCustomValidity(t('validation.required'))}
                                            onInput={(e: any) => e.target.setCustomValidity('')} 
                                    disabled={isSubmitting}
                                />
                            </div>

                            <div className="col-12 md:col-6 mb-3">
                                <label className="text-700 text-sm font-semibold mb-1 block">{t('usersEdit.regNo')}</label>
                                <InputText 
                                    value={registrationNumber} 
                                    onChange={(e) => setRegistrationNumber(e.target.value)} 
                                    className="w-full p-inputtext-sm font-mono" 
                                    required
                                            onInvalid={(e: any) => e.target.setCustomValidity(t('validation.required'))}
                                            onInput={(e: any) => e.target.setCustomValidity('')}
                                    disabled={isSubmitting}
                                />
                            </div>
                            <div className="col-12 md:col-6 mb-3">
                                <label className="text-700 text-sm font-semibold mb-1 block">{t('usersEdit.tcNoLabel')}</label>
                                <InputText 
                                    value={tcNo} 
                                    onChange={(e) => setTcNo(e.target.value.replace(/[^0-9]/g, ''))} 
                                    className="w-full p-inputtext-sm font-mono" 
                                    maxLength={11}
                                    placeholder={t("usersEdit.tcNoPlaceholder")}
                                    required
                                            onInvalid={(e: any) => e.target.setCustomValidity(t('validation.required'))}
                                            onInput={(e: any) => e.target.setCustomValidity('')} 
                                    disabled={isSubmitting}
                                />
                            </div>

                            <div className="col-12 md:col-6 mb-3">
                                <label className="text-700 text-sm font-semibold mb-1 block">{t('usersEdit.email')}</label>
                                <InputText 
                                    type="email"
                                    value={email} 
                                    onChange={(e) => setEmail(e.target.value)} 
                                    className="w-full p-inputtext-sm" 
                                    required
                                            onInvalid={(e: any) => e.target.setCustomValidity(t('validation.required'))}
                                            onInput={(e: any) => e.target.setCustomValidity('')} 
                                    disabled={isSubmitting}
                                />
                            </div>
                            <div className="col-12 md:col-6 mb-3">
                                <label className="text-700 text-sm font-semibold mb-1 block">{t('usersEdit.phone')}</label>
                                <InputText 
                                    value={phoneNumber} 
                                    onChange={(e) => setPhoneNumber(e.target.value.replace(/[^0-9]/g, ''))} 
                                    className="w-full p-inputtext-sm font-mono" 
                                    maxLength={11}
                                    placeholder={t("usersEdit.phonePlaceholder")}
                                    disabled={isSubmitting}
                                />
                            </div>

                            <div className="col-12 mb-3">
                                <label className="text-700 text-sm font-semibold mb-2 block">{t('usersEdit.accountStatus')}</label>
                                <div className="flex align-items-center gap-2">
                                    <InputSwitch checked={isActive} onChange={(e) => setIsActive(e.value ?? false)} disabled={isSubmitting || isMe} />
                                    <span className="text-sm text-700">{isActive ? t('usersEdit.activeAccount') : t('usersEdit.passiveAccount')}</span>
                                    {isMe && <span className="text-xs text-orange-500 ml-2">{t('usersEdit.cannotDeactivateSelf')}</span>}
                                </div>
                            </div>

                            {isSuperAdmin && (
                                <div className="col-12 mt-2 pt-3 border-top-1 surface-border">
                                    <label className="text-900 text-sm font-bold mb-3 block"><i className="pi pi-shield mr-2"></i>{t('usersEdit.roleAuthDefinition')}</label>
                                    <div className="flex flex-column gap-3">
                                        {allPermissions.map((perm) => (
                                            <div key={perm.id} className="flex align-items-center">
                                                <Checkbox 
                                                    inputId={`perm_${perm.id}`} 
                                                    value={perm.id} 
                                                    onChange={(e) => handlePermissionChange(e, perm.id)} 
                                                    checked={userPermissionIds.includes(perm.id)} 
                                                    disabled={isSubmitting || (isMe && perm.name === 'PermissionManagement')} 
                                                />
                                                <label htmlFor={`perm_${perm.id}`} className="ml-2 text-sm text-700 flex align-items-center">
                                                    <span className="font-bold mr-1">{t('permName.' + perm.name, perm.name)}</span>
                                                    {isMe && perm.name === 'PermissionManagement' && <span className="text-xs text-orange-500 ml-2">{t('usersEdit.cannotRemoveOwnPerm')}</span>}
                                                </label>
                                            </div>
                                        ))}
                                    </div>
                                </div>
                            )}

                            <div className="col-12 flex flex-wrap justify-content-end gap-2 pt-3 border-top-1 surface-border mt-3">
                                <Button 
                                    type="button" 
                                    label={t("usersEdit.cancel")} 
                                    severity="secondary" 
                                    outlined 
                                    onClick={() => router.push('/users')} 
                                    disabled={isSubmitting} 
                                    size="small"
                                />
                                <Button 
                                    type="submit" 
                                    label={t("usersEdit.saveChanges")} 
                                    severity="success" 
                                    loading={isSubmitting} 
                                    size="small"
                                />
                            </div>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    );
};

export default EditUserPage;
