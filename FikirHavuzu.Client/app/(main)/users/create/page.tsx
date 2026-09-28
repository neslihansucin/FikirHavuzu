'use client';
import React, { useState, useEffect, useRef } from 'react';
import { useRouter } from 'next/navigation';
import { InputText } from 'primereact/inputtext';
import { Button } from 'primereact/button';
import { Toast } from 'primereact/toast';
import api from '../../../../utils/api';
import { useTranslation } from 'react-i18next';

const CreateUserPage = () => {
    const { t } = useTranslation();
    const router = useRouter();
    const toast = useRef<Toast>(null);
    
    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
    const [tcNo, setTcNo] = useState('');
    const [email, setEmail] = useState('');
    const [phoneNumber, setPhoneNumber] = useState('');
    const [registrationNumber, setRegistrationNumber] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);

    useEffect(() => {
        const fetchRegNo = async () => {
            try {
                const res = await api.get('/users/next-registration-number');
                setRegistrationNumber(res.data.registrationNumber);
            } catch (error) {
                console.error("Sicil no alınamadı", error);
            }
        };
        fetchRegNo();
    }, []);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        
        if (!firstName || !lastName || !tcNo || !email || !phoneNumber) {
            toast.current?.show({ severity: 'warn', summary: t('createUser.missingInfo'), detail: t('createUser.missingInfoDetail') });
            return;
        }

        if (tcNo.length !== 11) {
            toast.current?.show({ severity: 'warn', summary: t('createUser.invalidTcSummary'), detail: t('createUser.invalidTcDetail') });
            return;
        }

        setIsSubmitting(true);
        try {
            const baseUrl = window.location.origin;
            const response = await api.post('/users', {
                firstName,
                lastName,
                tcNo,
                email,
                phoneNumber,
                baseUrl
            });
            
            toast.current?.show({ severity: 'success', summary: t('toast.success'), detail: t('createUser.addSuccess') });
            setTimeout(() => {
                router.push('/users');
            }, 1500);
        } catch (error: any) {
            console.error("Çalışan eklenirken hata:", error);
            toast.current?.show({ severity: 'error', summary: t('toast.error'), detail: t('createUser.addError') });
            setIsSubmitting(false);
        }
    };

    return (
        <div className="grid">
            <Toast ref={toast} />
            
            <div className="col-12">
                <div className="card p-4 shadow-2 border-round-xl">
                    <div className="mb-4">
                        <h4 className="font-bold mb-1 text-900" style={{ letterSpacing: '-0.01em' }}>👤 {t('createUser.title')}</h4>
                        <p className="m-0 text-600 text-sm">{t('createUser.desc')}</p>
                    </div>

                    <form onSubmit={handleSubmit}>
                        <div className="formgrid grid">
                            <div className="col-12 md:col-6 mb-3">
                                <label className="text-700 text-sm font-semibold mb-1 block">{t('createUser.name')}</label>
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
                                <label className="text-700 text-sm font-semibold mb-1 block">{t('createUser.surname')}</label>
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
                                <label className="text-700 text-sm font-semibold mb-1 block">{t('createUser.regNo')}</label>
                                <InputText 
                                    value={registrationNumber} 
                                    readOnly 
                                    className="w-full p-inputtext-sm font-mono bg-gray-100" 
                                />
                            </div>
                            <div className="col-12 md:col-6 mb-3">
                                <label className="text-700 text-sm font-semibold mb-1 block">{t('createUser.tcNoFull')}</label>
                                <InputText 
                                    value={tcNo} 
                                    onChange={(e) => setTcNo(e.target.value.replace(/[^0-9]/g, ''))} 
                                    className="w-full p-inputtext-sm font-mono" 
                                    maxLength={11}
                                    placeholder={t('createUser.tcPlaceholder')}
                                    required
                                            onInvalid={(e: any) => e.target.setCustomValidity(t('validation.required'))}
                                            onInput={(e: any) => e.target.setCustomValidity('')} 
                                    disabled={isSubmitting}
                                />
                            </div>

                            <div className="col-12 md:col-6 mb-4">
                                <label className="text-700 text-sm font-semibold mb-1 block">{t('createUser.emailLabel')}</label>
                                <InputText 
                                    type="email"
                                    value={email} 
                                    onChange={(e) => setEmail(e.target.value)} 
                                    className="w-full p-inputtext-sm" 
                                    placeholder={t('createUser.emailPlaceholder')}
                                    required
                                            onInvalid={(e: any) => e.target.setCustomValidity(t('validation.required'))}
                                            onInput={(e: any) => e.target.setCustomValidity('')} 
                                    disabled={isSubmitting}
                                />
                            </div>
                            <div className="col-12 md:col-6 mb-4">
                                <label className="text-700 text-sm font-semibold mb-1 block">{t('createUser.phoneLabel')}</label>
                                <InputText 
                                    value={phoneNumber} 
                                    onChange={(e) => setPhoneNumber(e.target.value.replace(/[^0-9]/g, ''))} 
                                    className="w-full p-inputtext-sm font-mono" 
                                    maxLength={11}
                                    placeholder={t('createUser.phonePlaceholder')}
                                    required
                                            onInvalid={(e: any) => e.target.setCustomValidity(t('validation.required'))}
                                            onInput={(e: any) => e.target.setCustomValidity('')} 
                                    disabled={isSubmitting}
                                />
                            </div>

                            <div className="col-12 flex flex-wrap justify-content-end gap-2 pt-3 border-top-1 surface-border">
                                <Button 
                                    type="button" 
                                    label={t('createUser.cancel')} 
                                    severity="secondary" 
                                    outlined 
                                    onClick={() => router.push('/users')} 
                                    disabled={isSubmitting} 
                                    size="small"
                                />
                                <Button 
                                    type="submit" 
                                    label={t('createUser.saveBtn')} 
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

export default CreateUserPage;
