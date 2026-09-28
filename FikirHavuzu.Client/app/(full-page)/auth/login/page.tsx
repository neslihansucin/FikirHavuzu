'use client';
import { useRouter } from 'next/navigation';
import React, { useContext, useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Button } from 'primereact/button';
import { Dropdown } from 'primereact/dropdown';
import { Password } from 'primereact/password';
import { InputText } from 'primereact/inputtext';
import { Message } from 'primereact/message';
import { LayoutContext } from '../../../../layout/context/layoutcontext';
import { classNames } from 'primereact/utils';
import api from '../../../../utils/api';

const LoginPage = () => {
    const { t, i18n } = useTranslation();
    const [registrationNumber, setRegistrationNumber] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);
    const [isMounted, setIsMounted] = useState(false);
    
    const { layoutConfig } = useContext(LayoutContext);
    const router = useRouter();

    useEffect(() => {
        setIsMounted(true);
    }, []);

    const languages = [
        { name: 'TR', code: 'tr' },
        { name: 'EN', code: 'en' }
    ];

    const changeLanguage = (e: any) => {
        i18n.changeLanguage(e.value);
    };

    const handleLogin = async () => {
        if (!registrationNumber || !password) {
            setError(t('auth.missingInfo'));
            return;
        }

        try {
            setLoading(true);
            setError('');
            const response = await api.post('/auth/login', { registrationNumber, password });
            
            localStorage.setItem('token', response.data.token);
            localStorage.setItem('user', JSON.stringify(response.data.user));
            
            router.push('/');
        } catch (err: any) {
            setError(err.response?.data?.message || t('auth.loginFailed'));
        } finally {
            setLoading(false);
        }
    };

    const containerClassName = classNames('surface-ground flex align-items-center justify-content-center min-h-screen min-w-screen overflow-hidden', { 'p-input-filled': layoutConfig.inputStyle === 'filled' });

    if (!isMounted) return null;

    return (
        <div className={containerClassName}>
            <div className="absolute top-0 right-0 p-4">
                <Dropdown 
                    value={i18n.language?.substring(0, 2) || 'tr'} 
                    options={languages} 
                    optionLabel="name" 
                    optionValue="code"
                    onChange={changeLanguage} 
                    className="w-auto"
                />
            </div>
            <div className="flex flex-column align-items-center justify-content-center">
                <div className="text-center mb-5">
                    <div className="text-900 text-4xl font-bold mb-3">💡 {t('auth.appTitle')}</div>
                    <span className="text-600 font-medium">{t('auth.appSubtitle')}</span>
                </div>
                <div
                    style={{
                        borderRadius: '56px',
                        padding: '0.3rem',
                        background: 'linear-gradient(180deg, var(--primary-color) 10%, rgba(33, 150, 243, 0) 30%)'
                    }}
                >
                    <div className="w-full surface-card py-8 px-5 sm:px-8" style={{ borderRadius: '53px' }}>
                        <div className="text-center mb-5">
                            <div className="text-900 text-2xl font-medium mb-3">{t('auth.loginTitle')}</div>
                        </div>

                        <div>
                            {error && <Message severity="error" text={error} className="w-full mb-4" />}
                            
                            <label htmlFor="sicilno" className="block text-900 text-xl font-medium mb-2">
                                {t('auth.regNo')}
                            </label>
                            <InputText 
                                id="sicilno" 
                                type="text" 
                                placeholder={t('auth.regNoPlaceholder')} 
                                value={registrationNumber}
                                onChange={(e) => setRegistrationNumber(e.target.value)}
                                className="w-full md:w-30rem mb-5" 
                                style={{ padding: '1rem' }} 
                            />

                            <label htmlFor="password" className="block text-900 font-medium text-xl mb-2">
                                {t('auth.password')}
                            </label>
                            <Password 
                                inputId="password" 
                                value={password} 
                                onChange={(e) => setPassword(e.target.value)} 
                                placeholder={t('auth.password')} 
                                toggleMask 
                                feedback={false}
                                className="w-full mb-5" 
                                inputClassName="w-full p-3 md:w-30rem"
                                onKeyDown={(e) => e.key === 'Enter' && handleLogin()}
                            />

                            <div className="flex align-items-center justify-content-between mb-5 gap-5">
                                <div className="flex align-items-center"></div>
                                <a className="font-medium no-underline ml-2 text-right cursor-pointer" style={{ color: 'var(--primary-color)' }} onClick={() => router.push('/auth/forgot-password')}>
                                    {t('auth.forgotPassword')}
                                </a>
                            </div>

                            <Button 
                                label={t('auth.loginBtn')} 
                                className="w-full p-3 text-xl" 
                                onClick={handleLogin}
                                loading={loading}
                            />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default LoginPage;