'use client';
import React, { useEffect, useState, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { useRouter } from 'next/navigation';
import { InputText } from 'primereact/inputtext';
import { Dropdown } from 'primereact/dropdown';
import { InputTextarea } from 'primereact/inputtextarea';
import { FileUpload } from 'primereact/fileupload';
import { Button } from 'primereact/button';
import { Toast } from 'primereact/toast';
import api from '../../../../utils/api';

const CreateIdeaPage = () => { 
    const { t } = useTranslation();
    const router = useRouter();
    const toast = useRef<Toast>(null);
    const [categories, setCategories] = useState([]);
    
    // Form States
    const [title, setTitle] = useState('');
    const [categoryId, setCategoryId] = useState<number | null>(null);
    const [intendedBenefit, setIntendedBenefit] = useState('');
    const [description, setDescription] = useState('');
    const [files, setFiles] = useState<File[]>([]);
    const [isSubmitting, setIsSubmitting] = useState(false);

    useEffect(() => {
        const fetchCategories = async () => {
            try {
                const response = await api.get('/ideas/categories');
                const mapped = response.data.map((c: any) => ({ label: c.name, value: c.id }));
                setCategories(mapped);
            } catch (error) {
                console.error(t('createIdea.fetchError'), error);
                toast.current?.show({ severity: 'error', summary: t('createIdea.error'), detail: t('createIdea.fetchErrorToast') });
            }
        };
        fetchCategories();
    }, []);

    const onFileSelect = (e: any) => {
        setFiles(e.files);
    };

    const onFileRemove = (e: any) => {
        setFiles(files.filter(f => f.name !== e.file.name));
    };
    
    const onFileClear = () => {
        setFiles([]);
    };

    const handleSubmit = async (submitAction: 'draft' | 'publish') => {
        if (!title || !categoryId || !intendedBenefit || !description) {
            toast.current?.show({ severity: 'warn', summary: t('createIdea.missingInfo'), detail: t('createIdea.missingInfoDesc') });
            return;
        }

        setIsSubmitting(true);
        try {
            const formData = new FormData();
            formData.append('title', title);
            formData.append('categoryId', categoryId.toString());
            formData.append('intendedBenefit', intendedBenefit);
            formData.append('description', description);
            formData.append('submitAction', submitAction);
            
            files.forEach((file) => {
                formData.append('files', file);
            });

            const response = await api.post('/ideas', formData, {
                headers: { 'Content-Type': 'multipart/form-data' }
            });

            toast.current?.show({ severity: 'success', summary: t('createIdea.success'), detail: t('createIdea.submitSuccess') });
            
            setTimeout(() => {
                router.push('/ideas/my');
            }, 1500);

        } catch (error: any) {
            console.error("Fikir eklenirken hata:", error);
            const msg = error.response?.data?.message || t('createIdea.systemError');
            toast.current?.show({ severity: 'error', summary: t('createIdea.error'), detail: msg });
            setIsSubmitting(false);
        }
    };

    return (
        <div className="grid">
            <Toast ref={toast} />
            
            <div className="col-12">
                <div className="card p-4 shadow-2 border-round-xl">
                    <div className="mb-4">
                        <h4 className="font-bold mb-1 text-900" style={{ letterSpacing: '-0.01em' }}>{t('createIdea.pageTitle')}</h4>
                        <p className="m-0 text-600" style={{ fontSize: '0.9rem' }}>{t('createIdea.pageDesc')}</p>
                    </div>

                    <div className="formgrid grid">
                        <div className="col-12 mb-3">
                            <label className="text-700 text-sm font-semibold mb-1 block">{t('createIdea.titleLabel')}</label>
                            <InputText 
                                value={title} 
                                onChange={(e) => setTitle(e.target.value)} 
                                className="w-full p-inputtext-sm" 
                                placeholder={t('createIdea.titlePlaceholder')} 
                                disabled={isSubmitting}
                            />
                            <small className="text-500 block mt-1" style={{ fontSize: '0.8rem' }}>{t('createIdea.titleHelp')}</small>
                        </div>

                        <div className="col-12 mb-3">
                            <label className="text-700 text-sm font-semibold mb-1 block">{t('createIdea.categoryLabel')}</label>
                            <Dropdown 
                                value={categoryId} 
                                onChange={(e) => setCategoryId(e.value)} 
                                options={categories} 
                                optionLabel="label" 
                                placeholder={t('createIdea.categoryPlaceholder')} 
                                className="w-full p-inputtext-sm"
                                disabled={isSubmitting}
                            />
                        </div>

                        <div className="col-12 mb-3">
                            <label className="text-700 text-sm font-semibold mb-1 block">{t('createIdea.benefitLabel')}</label>
                            <InputTextarea 
                                value={intendedBenefit} 
                                onChange={(e) => setIntendedBenefit(e.target.value)} 
                                rows={2} 
                                className="w-full" 
                                placeholder={t('createIdea.benefitPlaceholder')} 
                                disabled={isSubmitting}
                            />
                        </div>

                        <div className="col-12 mb-3">
                            <label className="text-700 text-sm font-semibold mb-1 block">{t('createIdea.descLabel')}</label>
                            <InputTextarea 
                                value={description} 
                                onChange={(e) => setDescription(e.target.value)} 
                                rows={4} 
                                className="w-full" 
                                placeholder={t('createIdea.descPlaceholder')} 
                                disabled={isSubmitting}
                            />
                        </div>

                        <div className="col-12 mb-4">
                            <label className="text-700 text-sm font-semibold mb-1 block">{t('createIdea.filesLabel')}</label>
                            <FileUpload 
                                mode="advanced" 
                                multiple 
                                accept="image/*,.pdf,.doc,.docx,.xls,.xlsx" 
                                maxFileSize={10000000} 
                                onSelect={onFileSelect}
                                onRemove={onFileRemove}
                                onClear={onFileClear}
                                customUpload
                                auto={false}
                                chooseLabel={t('createIdea.fileSelect')}
                                cancelLabel={t('createIdea.fileCancel')}
                                emptyTemplate={<p className="m-0 text-center text-500 py-2">{t('createIdea.fileEmpty')}</p>}
                                disabled={isSubmitting}
                                className="p-fileupload-sm"
                            />
                            <small className="text-500 block mt-1" style={{ fontSize: '0.8rem' }}>{t('createIdea.fileHelp')}</small>
                        </div>

                        <div className="col-12 flex flex-wrap justify-content-end gap-2 pt-3 border-top-1 surface-border">
                            <Button 
                                type="button" 
                                label={t('createIdea.btnCancel')} 
                                severity="secondary" 
                                outlined
                                onClick={() => router.push('/ideas/my')} 
                                disabled={isSubmitting}
                            />
                            <Button 
                                type="button" 
                                label={t('createIdea.btnDraft')} 
                                icon="pi pi-file" 
                                severity="secondary" 
                                outlined
                                onClick={() => handleSubmit('draft')}
                                loading={isSubmitting}
                            />
                            <Button 
                                type="button" 
                                label={t('createIdea.btnSubmit')} 
                                icon="pi pi-send" 
                                severity="success" 
                                onClick={() => handleSubmit('publish')}
                                loading={isSubmitting}
                            />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default CreateIdeaPage;
