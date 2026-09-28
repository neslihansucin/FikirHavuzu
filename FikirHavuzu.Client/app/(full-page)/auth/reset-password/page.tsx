'use client';
import { useRouter, useSearchParams } from 'next/navigation';
import React, { useState, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { Button } from 'primereact/button';
import { Password } from 'primereact/password';
import { Toast } from 'primereact/toast';
import api from '../../../../utils/api';

const ResetPasswordPage = () => {
    const { t } = useTranslation();
    const router = useRouter();
    const searchParams = useSearchParams();
    const toast = useRef<Toast>(null);
    
    const email = searchParams.get('email');
    const token = searchParams.get('token');

    const [newPassword, setNewPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [loading, setLoading] = useState(false);
    const [isMounted, setIsMounted] = React.useState(false);

    React.useEffect(() => {
        setIsMounted(true);
    }, []);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        
        if (!email || !token) {
            toast.current?.show({ severity: 'error', summary: t('toast.error'), detail: t('auth.invalidToken') });
            return;
        }

        setLoading(true);
        try {
            await api.post('/auth/reset-password', {
                email,
                token,
                newPassword,
                confirmPassword
            });
            toast.current?.show({ severity: 'success', summary: t('toast.success'), detail: t('auth.passwordChanged') });
            setTimeout(() => router.push('/auth/login'), 3000);
        } catch (error: any) {
            toast.current?.show({ severity: 'error', summary: t('toast.error'), detail: t('toast.genericError') });
        } finally {
            setLoading(false);
        }
    };

    if (!isMounted) return null;

    return (
        <div className="surface-ground flex align-items-center justify-content-center min-h-screen min-w-screen overflow-hidden">
            <Toast ref={toast} />
            <div className="flex flex-column align-items-center justify-content-center w-full md:w-6 lg:w-4 px-3">
                <div style={{ borderRadius: '56px', padding: '0.3rem', background: 'linear-gradient(180deg, var(--green-500) 10%, rgba(33, 150, 243, 0) 30%)' }} className="w-full">
                    <div className="w-full surface-card py-8 px-5 sm:px-8" style={{ borderRadius: '53px' }}>
                        <div className="text-center mb-5">
                            <div className="text-900 text-3xl font-bold mb-3">🔑 {t('auth.resetPasswordTitle')}</div>
                            <span className="text-600 font-medium">{t('auth.resetPasswordDesc')}</span>
                        </div>

                        {!email || !token ? (
                            <div className="text-center">
                                <i className="pi pi-exclamation-circle text-red-500 mb-3" style={{ fontSize: '3rem' }}></i>
                                <p className="text-700 m-0">{t('auth.invalidToken')}</p>
                                <Button label={t('auth.backToLogin')} className="p-button-text mt-4" onClick={() => router.push('/auth/login')} />
                            </div>
                        ) : (
                            <form onSubmit={handleSubmit} className="flex flex-column gap-4 mt-4">
                                <div>
                                    <label htmlFor="newPassword" className="block text-900 font-medium text-sm mb-2">{t('auth.newPassword')}</label>
                                    <Password 
                                        id="newPassword" 
                                        value={newPassword} 
                                        onChange={(e) => setNewPassword(e.target.value)} 
                                        toggleMask 
                                        feedback={false}
                                        inputClassName="w-full p-inputtext-lg"
                                        className="w-full"
                                        required 
                                        onInvalid={(e: any) => e.target.setCustomValidity(t('validation.required'))} 
                                        onInput={(e: any) => e.target.setCustomValidity('')}
                                    />
                                </div>
                                <div>
                                    <label htmlFor="confirmPassword" className="block text-900 font-medium text-sm mb-2">{t('auth.confirmPassword')}</label>
                                    <Password 
                                        id="confirmPassword" 
                                        value={confirmPassword} 
                                        onChange={(e) => setConfirmPassword(e.target.value)} 
                                        toggleMask 
                                        feedback={false}
                                        inputClassName="w-full p-inputtext-lg"
                                        className="w-full"
                                        required 
                                        onInvalid={(e: any) => e.target.setCustomValidity(t('validation.required'))} 
                                        onInput={(e: any) => e.target.setCustomValidity('')}
                                    />
                                </div>

                                <Button 
                                    label={t('auth.savePassword')} 
                                    className="w-full p-button-success p-button-lg mt-2" 
                                    type="submit" 
                                    loading={loading} 
                                />
                            </form>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default ResetPasswordPage;
