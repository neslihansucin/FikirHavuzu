'use client';
import { useRouter } from 'next/navigation';
import React, { useState, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { Button } from 'primereact/button';
import { InputText } from 'primereact/inputtext';
import { Toast } from 'primereact/toast';
import api from '../../../../utils/api';

const ForgotPasswordPage = () => {
    const { t } = useTranslation();
    const router = useRouter();
    const toast = useRef<Toast>(null);
    const [identifier, setIdentifier] = useState('');
    const [loading, setLoading] = useState(false);
    const [isMounted, setIsMounted] = React.useState(false);

    React.useEffect(() => {
        setIsMounted(true);
    }, []);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        try {
            const isEmail = identifier.includes('@');
            const payload = {
                email: isEmail ? identifier : null,
                registrationNumber: !isEmail ? identifier : null,
                baseUrl: window.location.origin
            };
            
            await api.post('/auth/forgot-password', payload);
            toast.current?.show({ severity: 'success', summary: t('toast.success'), detail: t('auth.resetLinkSent') });
            setTimeout(() => router.push('/auth/login'), 3000);
        } catch (error: any) {
            toast.current?.show({ severity: 'error', summary: t('toast.error'), detail: t('auth.resetError') });
        } finally {
            setLoading(false);
        }
    };

    if (!isMounted) return null;

    return (
        <div className="surface-ground flex align-items-center justify-content-center min-h-screen min-w-screen overflow-hidden">
            <Toast ref={toast} />
            <div className="flex flex-column align-items-center justify-content-center w-full md:w-6 lg:w-4 px-3">
                <div style={{ borderRadius: '56px', padding: '0.3rem', background: 'linear-gradient(180deg, var(--orange-500) 10%, rgba(33, 150, 243, 0) 30%)' }} className="w-full">
                    <div className="w-full surface-card py-8 px-5 sm:px-8" style={{ borderRadius: '53px' }}>
                        <div className="text-center mb-5">
                            <div className="text-900 text-3xl font-bold mb-3">🔒 {t('auth.forgotPassword')}</div>
                            <span className="text-600 font-medium">{t('auth.forgotPasswordDesc')}</span>
                        </div>

                        <form onSubmit={handleSubmit} className="flex flex-column gap-4 mt-4">
                            <div>
                                <label htmlFor="identifier" className="block text-900 font-medium text-sm mb-2">{t('auth.emailOrRegNo')}</label>
                                <InputText 
                                    id="identifier" 
                                    type="text" 
                                    className="w-full p-inputtext-lg" 
                                    value={identifier} 
                                    onChange={(e) => setIdentifier(e.target.value)} 
                                    required 
                                    onInvalid={(e: any) => e.target.setCustomValidity(t('validation.required'))} 
                                    onInput={(e: any) => e.target.setCustomValidity('')}
                                />
                            </div>

                            <Button 
                                label={t('auth.sendResetLink')} 
                                className="w-full p-button-warning p-button-lg" 
                                type="submit" 
                                loading={loading} 
                            />
                            
                            <Button 
                                label={t('auth.backToLogin')}
                                icon="pi pi-arrow-left" 
                                type="button"
                                className="p-button-text w-full mt-2" 
                                onClick={() => router.push('/auth/login')} 
                            />
                        </form>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default ForgotPasswordPage;
