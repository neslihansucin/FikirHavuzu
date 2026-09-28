'use client';
import React, { useEffect, useState, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { useRouter } from 'next/navigation';
import { Button } from 'primereact/button';
import { Toast } from 'primereact/toast';
import { InputText } from 'primereact/inputtext';
import { Password } from 'primereact/password';
import api from '../../../utils/api';

const ProfilePage = () => {
    const { t } = useTranslation();
    const router = useRouter();
    const toast = useRef<Toast>(null);

    const [profile, setProfile] = useState<any>(null);
    const [loading, setLoading] = useState(true);

    // Form fields
    const [phoneNumber, setPhoneNumber] = useState('');
    const [selectedFile, setSelectedFile] = useState<File | null>(null);
    const [removePhoto, setRemovePhoto] = useState(false);
    const [isSubmittingProfile, setIsSubmittingProfile] = useState(false);

    // Password form fields
    const [currentPassword, setCurrentPassword] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [isSubmittingPassword, setIsSubmittingPassword] = useState(false);

    const fetchProfile = async () => {
        setLoading(true);
        try {
            const res = await api.get('/auth/profile');
            setProfile(res.data);
            setPhoneNumber(res.data.phoneNumber || '');
            
            // update local storage context if needed
            const userStr = localStorage.getItem('user');
            if (userStr) {
                const userObj = JSON.parse(userStr);
                userObj.profilePictureUrl = res.data.profilePictureUrl;
                localStorage.setItem('user', JSON.stringify(userObj));
                // Custom event to tell topbar to update avatar
                window.dispatchEvent(new Event('profile-updated'));
            }
        } catch (error: any) {
            console.error("Profil alınamadı", error);
            if (error.response?.status === 401) router.push('/auth/login');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchProfile();
    }, []);

    const handleProfileSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsSubmittingProfile(true);

        const formData = new FormData();
        if (phoneNumber) formData.append('PhoneNumber', phoneNumber);
        if (removePhoto) formData.append('RemoveProfilePhoto', 'true');
        if (selectedFile) formData.append('ProfilePhoto', selectedFile);

        try {
            const res = await api.post('/auth/profile', formData, {
                headers: { 'Content-Type': 'multipart/form-data' }
            });
            toast.current?.show({ severity: 'success', summary: t('toast.success', 'Başarılı'), detail: res.data.message });
            setSelectedFile(null);
            setRemovePhoto(false);
            
            const fileInput = document.getElementById('photoInput') as HTMLInputElement;
            if (fileInput) fileInput.value = '';
            
            fetchProfile();
        } catch (error: any) {
            toast.current?.show({ severity: 'error', summary: t('toast.error', 'Hata'), detail: t('profile.updateError', 'Profil güncellenemedi.') });
        } finally {
            setIsSubmittingProfile(false);
        }
    };

    const handlePasswordSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!newPassword || !confirmPassword) return;

        setIsSubmittingPassword(true);
        try {
            const res = await api.post('/auth/change-password', { newPassword, confirmPassword });
            toast.current?.show({ severity: 'success', summary: t('toast.success', 'Başarılı'), detail: res.data.message });
            setCurrentPassword('');
            setNewPassword('');
            setConfirmPassword('');
        } catch (error: any) {
            toast.current?.show({ severity: 'error', summary: t('toast.error', 'Hata'), detail: t('profile.passwordError', 'Şifre değiştirilemedi.') });
        } finally {
            setIsSubmittingPassword(false);
        }
    };

    if (loading) return <div>{t("profile.loading")}</div>;
    if (!profile) return <div>{t("profile.notFound")}</div>;

    const maskedTc = profile.tcNo && profile.tcNo.length >= 11
        ? profile.tcNo.substring(0, 2) + "*******" + profile.tcNo.substring(profile.tcNo.length - 2)
        : "***";

    const baseUrl = api.defaults.baseURL?.replace('/api', '') || '';

    return (
        <div className="grid">
            <Toast ref={toast} />
            
            <div className="col-12">
                <div className="mb-4">
                    <h3 className="font-bold mb-1 text-900" style={{ letterSpacing: '-0.02em' }}>👤 {t("profile.title")}</h3>
                    <p className="m-0 text-600 text-sm">{t("profile.subtitle")}</p>
                </div>
            </div>

            <div className="col-12 lg:col-4">
                <div className="card p-4 shadow-2 border-round-xl text-center mb-4">
                    <div className="relative inline-block mb-3">
                        {profile.profilePictureUrl && !removePhoto ? (
                            <img src={`${baseUrl}${profile.profilePictureUrl}?t=${Date.now()}`} alt="Avatar" className="border-circle border-3 border-primary" style={{ width: '88px', height: '88px', objectFit: 'cover' }} />
                        ) : (
                            <div className="border-circle flex align-items-center justify-content-center font-bold text-3xl bg-primary-100 text-primary" style={{ width: '88px', height: '88px' }}>
                                {profile.firstName?.[0]}{profile.lastName?.[0]}
                            </div>
                        )}
                    </div>
                    <h5 className="font-bold mb-1 text-900">{profile.firstName} {profile.lastName}</h5>
                    <span className="text-500 text-sm block mb-2">{t("profile.regNoLabel")}: {profile.registrationNumber}</span>
                    
                    <div className="mb-3">
                        <span className="bg-yellow-100 text-yellow-800 px-3 py-1 border-round-xl text-sm font-semibold border-1 border-yellow-400 white-space-nowrap">
                            {profile.badgeIcon} {t(profile.badgeTitle === 'İnovasyon Lideri' ? 'badges.innovationLeader' : profile.badgeTitle === 'Pırıltılı İnovatör' ? 'badges.glitteringInnovator' : profile.badgeTitle === 'İnovatif Düşünür' ? 'badges.innovativeThinker' : profile.badgeTitle === 'Fikir Kaşifi' ? 'badges.ideaExplorer' : profile.badgeTitle)} ({profile.innovationPoints} {t("profile.points")})
                        </span>
                    </div>

                    <div className="flex justify-content-around text-center pt-3 border-top-1 surface-border">
                        <div>
                            <div className="font-bold text-xl text-900">{profile.totalIdeasCount}</div>
                            <span className="text-500" style={{ fontSize: '0.72rem' }}>{t("profile.stats.myIdeas")}</span>
                        </div>
                        <div>
                            <div className="font-bold text-xl text-green-500">{profile.approvedIdeasCount}</div>
                            <span className="text-500" style={{ fontSize: '0.72rem' }}>{t("profile.stats.approved")}</span>
                        </div>
                        <div>
                            <div className="font-bold text-xl text-orange-500">{profile.implementedIdeasCount}</div>
                            <span className="text-500" style={{ fontSize: '0.72rem' }}>{t("profile.stats.implemented")}</span>
                        </div>
                        <div>
                            <div className="font-bold text-xl text-yellow-500">{profile.innovationPoints}</div>
                            <span className="text-500" style={{ fontSize: '0.72rem' }}>{t("profile.stats.points")}</span>
                        </div>
                    </div>
                </div>

                <div className="card p-4 shadow-2 border-round-xl">
                    <h6 className="font-bold mb-3 text-900"><i className="pi pi-shield text-primary mr-2"></i>{t("profile.roles")}</h6>
                    {(!profile.permissions || profile.permissions.length === 0) ? (
                        <p className="text-500 text-sm m-0">{t("profile.noRolesDesc")}</p>
                    ) : (
                        <div className="flex flex-wrap gap-2">
                            {profile.permissions.map((perm: string) => (
                                <span key={perm} className="bg-gray-100 text-gray-700 px-2 py-1 border-round text-sm font-semibold border-1 border-gray-300 white-space-nowrap">{t('permName.' + perm, perm)}</span>
                            ))}
                        </div>
                    )}
                </div>
            </div>

            <div className="col-12 lg:col-8">
                <div className="card p-4 shadow-2 border-round-xl mb-4">
                    <form onSubmit={handleProfileSubmit}>
                        <h6 className="font-bold mb-3 text-900"><i className="pi pi-info-circle text-primary mr-2"></i>{t("profile.basicInfo")}</h6>
                        
                        <div className="formgrid grid mb-4">
                            <div className="col-12 md:col-6 mb-3">
                                <label className="text-500 text-sm font-semibold block mb-1">{t("profile.name")}</label>
                                <InputText value={profile.firstName} className="w-full p-inputtext-sm bg-gray-100" readOnly />
                            </div>
                            <div className="col-12 md:col-6 mb-3">
                                <label className="text-500 text-sm font-semibold block mb-1">{t("profile.surname")}</label>
                                <InputText value={profile.lastName} className="w-full p-inputtext-sm bg-gray-100" readOnly />
                            </div>
                            <div className="col-12 md:col-6 mb-3">
                                <label className="text-500 text-sm font-semibold block mb-1">{t("profile.email")}</label>
                                <InputText value={profile.email} className="w-full p-inputtext-sm bg-gray-100" readOnly />
                            </div>
                            <div className="col-12 md:col-6 mb-3">
                                <label className="text-500 text-sm font-semibold block mb-1">{t("profile.tcNo")}</label>
                                <InputText value={maskedTc} className="w-full p-inputtext-sm bg-gray-100 font-mono" readOnly />
                            </div>
                            <div className="col-12 md:col-6 mb-3">
                                <label className="text-700 text-sm font-semibold block mb-1">{t("profile.phone")}</label>
                                <InputText 
                                    value={phoneNumber} 
                                    onChange={(e) => setPhoneNumber(e.target.value.replace(/[^0-9]/g, ''))} 
                                    className="w-full p-inputtext-sm" 
                                    maxLength={11} 
                                    disabled={isSubmittingProfile}
                                />
                            </div>
                            <div className="col-12 md:col-6 mb-3">
                                <label className="text-700 text-sm font-semibold block mb-1">{t("profile.photoLabel")}</label>
                                <input 
                                    type="file" 
                                    id="photoInput"
                                    className="p-inputtext p-component p-inputtext-sm w-full" 
                                    accept="image/*"
                                    onChange={(e) => {
                                        if (e.target.files && e.target.files[0]) {
                                            setSelectedFile(e.target.files[0]);
                                            setRemovePhoto(false);
                                        }
                                    }}
                                    disabled={isSubmittingProfile}
                                />
                                {profile.profilePictureUrl && !removePhoto && !selectedFile && (
                                    <div className="mt-2">
                                        <Button type="button" label={t("profile.removePhoto")} outlined size="small" onClick={() => setRemovePhoto(true)} className="py-1 px-2 text-xs" />
                                    </div>
                                )}
                                {removePhoto && (
                                    <div className="mt-2 bg-yellow-50 text-yellow-800 p-2 border-round text-xs border-1 border-yellow-200">
                                        <i className="pi pi-exclamation-triangle mr-1"></i> {t("profile.photoWillBeDeleted")}
                                        <Button type="button" label={t("profile.undo")} link onClick={() => setRemovePhoto(false)} className="p-0 ml-2 text-xs text-red-500" />
                                    </div>
                                )}
                            </div>
                        </div>
                        <div className="text-right border-top-1 surface-border pt-3">
                            <Button type="submit" label={t("profile.updateInfo")} severity="success" loading={isSubmittingProfile} size="small" />
                        </div>
                    </form>
                </div>

                <div className="card p-4 shadow-2 border-round-xl">
                    <form onSubmit={handlePasswordSubmit}>
                        <h6 className="font-bold mb-3 text-900"><i className="pi pi-key text-primary mr-2"></i>{t("profile.security")}</h6>
                        <p className="text-500 text-xs mb-3">{t("profile.securityDesc")}</p>

                        <div className="formgrid grid mb-3">
                            <div className="col-12 md:col-6 mb-3">
                                <label className="text-700 text-sm font-semibold block mb-1">{t("profile.currentPassword")}</label>
                                <Password value={currentPassword} onChange={(e) => setCurrentPassword(e.target.value)} feedback={false} toggleMask className="w-full" inputClassName="w-full p-inputtext-sm" disabled={isSubmittingPassword} />
                            </div>
                        </div>
                        <div className="formgrid grid mb-4">
                            <div className="col-12 md:col-6 mb-3">
                                <label className="text-700 text-sm font-semibold block mb-1">{t("profile.newPassword")}</label>
                                <Password value={newPassword} onChange={(e) => setNewPassword(e.target.value)} feedback={false} toggleMask className="w-full" inputClassName="w-full p-inputtext-sm" disabled={isSubmittingPassword} />
                            </div>
                            <div className="col-12 md:col-6 mb-3">
                                <label className="text-700 text-sm font-semibold block mb-1">{t("profile.confirmPassword")}</label>
                                <Password value={confirmPassword} onChange={(e) => setConfirmPassword(e.target.value)} feedback={false} toggleMask className="w-full" inputClassName="w-full p-inputtext-sm" disabled={isSubmittingPassword} />
                            </div>
                        </div>
                        
                        <div className="bg-gray-50 border-1 border-gray-200 p-2 border-round-md text-xs text-600 mb-3">
                            <i className="pi pi-shield text-green-500 mr-1"></i> {t("profile.passwordRules")}
                        </div>

                        <div className="text-right border-top-1 surface-border pt-3">
                            <Button type="submit" label={t("profile.changePasswordBtn")} severity="success" loading={isSubmittingPassword} size="small" />
                        </div>
                    </form>
                </div>
            </div>
        </div>
    );
};

export default ProfilePage;
