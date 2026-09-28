'use client';
import React, { useEffect, useState, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { useRouter, useParams } from 'next/navigation';
import { Button } from 'primereact/button';
import { Toast } from 'primereact/toast';
import { Dialog } from 'primereact/dialog';
import { Dropdown } from 'primereact/dropdown';
import { InputTextarea } from 'primereact/inputtextarea';
import api from '../../../../../utils/api';

const IdeaDetailsPage = () => {
    const { t } = useTranslation();
    const router = useRouter();
    const params = useParams();
    const ideaId = params.id;
    const toast = useRef<Toast>(null);
    
    const [idea, setIdea] = useState<any>(null);
    const [loading, setLoading] = useState(true);

    const [imageModalVisible, setImageModalVisible] = useState(false);
    const [currentImageSrc, setCurrentImageSrc] = useState('');
    const [currentImageName, setCurrentImageName] = useState('');

    // Evaluation State
    const [score, setScore] = useState(50);
    const [decision, setDecision] = useState('Positive');
    const [comment, setComment] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);

    const fetchIdea = async () => {
        setLoading(true);
        try {
            const response = await api.get(`/ideas/${ideaId}`);
            setIdea(response.data);
        } catch (error: any) {
            console.error("Fikir yüklenemedi:", error);
            if (error.response?.status === 404) {
                router.push('/');
            }
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        if (ideaId) fetchIdea();
    }, [ideaId]);

    const handleScoreChange = (val: number) => {
        setScore(val);
        if (val >= 50) setDecision('Positive');
        else setDecision('Negative');
    };

    const submitEvaluation = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsSubmitting(true);
        try {
            const res = await api.post(`/ideas/${ideaId}/evaluate`, { score, decision, comment });
            toast.current?.show({ severity: 'success', summary: t('toast.success'), detail: t('details.evalSuccess') });
            setComment('');
            fetchIdea();
        } catch (error: any) {
            toast.current?.show({ severity: 'error', summary: t('toast.error', 'Hata'), detail: t('details.evalSaveError') });
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleReopen = async () => {
        setIsSubmitting(true);
        try {
            const res = await api.post(`/ideas/${ideaId}/reopen`);
            toast.current?.show({ severity: 'success', summary: t('toast.success'), detail: t('details.reopenSuccess') });
            fetchIdea();
        } catch (error: any) {
            toast.current?.show({ severity: 'error', summary: t('toast.error', 'Hata'), detail: t('details.actionError') });
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleImplement = async () => {
        setIsSubmitting(true);
        try {
            const res = await api.post(`/ideas/${ideaId}/implement`);
            toast.current?.show({ severity: 'success', summary: t('toast.success'), detail: t('details.reopenSuccess') });
            fetchIdea();
        } catch (error: any) {
            toast.current?.show({ severity: 'error', summary: t('toast.error', 'Hata'), detail: t('details.actionError') });
        } finally {
            setIsSubmitting(false);
        }
    };

    const getScoreFeedback = (num: number) => {
        if (num === 100) return { bg: '#FEF3C7', color: '#92400E', border: '#F59E0B', text: t('details.score100') };
        if (num >= 90) return { bg: '#E0F2FE', color: '#0369A1', border: '#7DD3FC', text: t('details.score90') };
        if (num >= 70) return { bg: '#F0FDF4', color: '#15803D', border: '#86EFAC', text: t('details.score70') };
        if (num >= 50) return { bg: '#F0FDF4', color: '#15803D', border: '#86EFAC', text: t('details.score50') };
        if (num >= 35) return { bg: '#FEF9C3', color: '#854D0E', border: '#FDE047', text: t('details.score35') };
        if (num >= 20) return { bg: '#FFEDD5', color: '#9A3412', border: '#FDBA74', text: t('details.score20') };
        return { bg: '#FEE2E2', color: '#991B1B', border: 'rgba(153, 27, 27, 0.15)', text: t('details.score0') };
    };

    if (loading) return <div>{t('details.loading')}</div>;
    if (!idea) return <div>{t('details.notFound')}</div>;

    const feedback = getScoreFeedback(score);
    const decisionOptions = [
        { label: t('details.decisionPositive'), value: 'Positive' },
        { label: t('details.decisionNegative'), value: 'Negative' }
    ];

    return (
        <div className="grid">
            <Toast ref={toast} />
            
            <style dangerouslySetInnerHTML={{__html: `
                input[type=range].score-slider { -webkit-appearance: none; appearance: none; width: 100%; height: 8px; border-radius: 4px; background: #E2E8F0; outline: none; transition: background 0.2s ease; }
                input[type=range].score-slider::-webkit-slider-thumb { -webkit-appearance: none; appearance: none; width: 22px; height: 22px; border-radius: 50%; background: #14b8a6; cursor: pointer; border: 2px solid #FFFFFF; box-shadow: 0 2px 5px rgba(0,0,0,0.18); transition: all 0.2s ease; }
                input[type=range].score-slider.sparkle-thumb::-webkit-slider-thumb { background: #F59E0B; border: 2px solid #FEF3C7; box-shadow: 0 0 12px #F59E0B, 0 0 24px #FBBF24, 0 0 35px #FDE68A; transform: scale(1.25); }
            `}} />

            <div className="col-12">
                <div className="mb-4">
                    <h3 className="font-bold mb-1 text-900" style={{ letterSpacing: '-0.02em' }}>{t('details.pageTitle')}</h3>
                    <p className="m-0 text-600 text-sm">{t('details.pageDesc')}</p>
                </div>
            </div>

            <div className="col-12 lg:col-8">
                <div className="card p-4 shadow-2 border-round-xl mb-4">
                    <div className="flex justify-content-between align-items-start mb-3">
                        <div>
                            <span className="bg-gray-100 text-gray-700 px-2 py-1 border-round text-xs font-semibold d-inline-block mb-2 white-space-nowrap">{t(idea.categoryName === 'Hizmet Geliştirme' ? 'category.service' : idea.categoryName === 'Ürün İyileştirme' ? 'category.product' : idea.categoryName === 'Süreç & Verimlilik' ? 'category.process' : idea.categoryName ? idea.categoryName : 'category.other')}</span>
                            <h4 className="font-bold mb-0 text-900" style={{ letterSpacing: '-0.01em' }}>
                                {idea.title}
                                {idea.editHistory && idea.editHistory.length > 0 && <span className="bg-gray-100 text-gray-600 px-2 py-1 border-round text-xs ml-2 white-space-nowrap"><i className="pi pi-pencil mr-1"></i>{t('details.edited')}</span>}
                            </h4>
                        </div>
                        <div className="ml-3 white-space-nowrap">
                            {idea.status === 'Draft' && <span className="bg-gray-100 text-gray-700 px-2 py-1 border-round text-sm font-semibold white-space-nowrap"><i className="pi pi-file mr-1"></i> {t("details.statusDraft")}</span>}
                            {idea.status === 'Implemented' && <span className="bg-yellow-100 text-yellow-800 px-2 py-1 border-round text-sm font-bold border-1 border-yellow-400 white-space-nowrap"><i className="pi pi-star mr-1"></i> {t('details.statusImplemented')}</span>}
                            {idea.status === 'Approved' && <span className="bg-green-100 text-green-800 px-2 py-1 border-round text-sm font-semibold white-space-nowrap"><i className="pi pi-check-circle mr-1"></i> {t("details.statusApproved")}</span>}
                            {idea.status === 'Rejected' && <span className="bg-red-100 text-red-800 px-2 py-1 border-round text-sm font-semibold white-space-nowrap"><i className="pi pi-times-circle mr-1"></i> {t("details.statusRejected")}</span>}
                            {idea.status === 'Withdrawn' && <span className="bg-gray-100 text-gray-700 px-2 py-1 border-round text-sm font-semibold white-space-nowrap"><i className="pi pi-arrow-left mr-1"></i> {t('details.statusWithdrawn')}</span>}
                            {idea.status === 'Pending' && <span className="bg-orange-100 text-orange-800 px-2 py-1 border-round text-sm font-semibold white-space-nowrap"><i className="pi pi-spin pi-spinner mr-1"></i> {t('details.statusPending')}</span>}
                        </div>
                    </div>

                    <div className="mb-4">
                        <span className="text-500 font-semibold text-xs uppercase block mb-2" style={{ letterSpacing: '0.04em' }}>{t('details.benefit')}</span>
                        <div className="p-3 border-round-md bg-gray-50 border-1 border-gray-200 text-800 text-sm">
                            {idea.intendedBenefit}
                        </div>
                    </div>

                    <div className="mb-4">
                        <span className="text-500 font-semibold text-xs uppercase block mb-2" style={{ letterSpacing: '0.04em' }}>{t('details.description')}</span>
                        <div className="p-3 border-round-md bg-gray-50 border-1 border-gray-200 text-800 text-sm" style={{ whiteSpace: 'pre-wrap' }}>
                            {idea.description}
                        </div>
                    </div>

                    <div className="mb-0">
                        <span className="text-500 font-semibold text-xs uppercase block mb-2" style={{ letterSpacing: '0.04em' }}><i className="pi pi-paperclip mr-1"></i>{t("details.attachments")}</span>
                        {(!idea.documents || idea.documents.length === 0) ? (
                            <p className="text-500 text-sm m-0">{t('details.noDocuments')}</p>
                        ) : (
                            <div className="flex flex-column gap-2">
                                {idea.documents.map((doc: any) => {
                                    const ext = doc.fileName.split('.').pop()?.toLowerCase();
                                    const isImage = ['jpg', 'jpeg', 'png', 'gif', 'webp'].includes(ext);
                                    const baseUrl = api.defaults.baseURL?.replace('/api', '') || '';
                                    const fullPath = baseUrl + doc.filePath;

                                    if (isImage) {
                                        return (
                                            <div key={doc.id} onClick={() => { setCurrentImageSrc(fullPath + '?t=' + Date.now()); setCurrentImageName(doc.fileName); setImageModalVisible(true); }} className="flex justify-content-between align-items-center p-2 border-round-md bg-gray-50 border-1 border-gray-200 cursor-pointer hover:bg-gray-100">
                                                <span className="text-sm text-800"><i className="pi pi-image text-green-500 mr-2"></i> {doc.fileName} <span className="bg-green-100 text-green-800 px-1 py-0 ml-2 border-round text-xs white-space-nowrap">{t('details.preview')}</span></span>
                                            </div>
                                        );
                                    } else {
                                        return (
                                            <a key={doc.id} href={fullPath} target="_blank" rel="noopener noreferrer" className="flex justify-content-between align-items-center p-2 border-round-md bg-gray-50 border-1 border-gray-200 cursor-pointer hover:bg-gray-100 no-underline">
                                                <span className="text-sm text-800"><i className="pi pi-file text-blue-500 mr-2"></i> {doc.fileName}</span>
                                            </a>
                                        );
                                    }
                                })}
                            </div>
                        )}
                    </div>
                </div>

                <div className="card p-4 shadow-2 border-round-xl mb-4">
                    <h6 className="font-bold mb-3 text-900"><i className="pi pi-star text-primary mr-2"></i>{idea.isEvaluator && !idea.isAuthor ? t('details.givenEvals') : t('details.receivedEvals')}</h6>
                    
                    {(!idea.evaluations || idea.evaluations.length === 0) ? (
                        <p className="text-500 text-sm m-0">{t('details.noEvals')}</p>
                    ) : (
                        <div className="flex flex-column gap-3">
                            {idea.evaluations.map((evalObj: any) => (
                                <div key={evalObj.id} className="p-3 border-round-md bg-gray-50 border-1 border-gray-200" style={{ borderLeft: `4px solid ${evalObj.decision === 'Positive' ? '#10b981' : '#ef4444'}` }}>
                                    <div className="flex justify-content-between align-items-center mb-2">
                                        <span className="font-semibold text-sm text-900">
                                            {t('details.scoreLabel')}: <span className={evalObj.decision === 'Positive' ? 'bg-green-100 text-green-800 px-2 py-1 border-round' : 'bg-red-100 text-red-800 px-2 py-1 border-round'}>{evalObj.score} / 100</span>
                                        </span>
                                        <span className="text-500 text-xs">{new Date(evalObj.approvedAt).toLocaleString('tr-TR')}</span>
                                    </div>
                                    <div className="text-700 text-sm">{evalObj.comment}</div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>

                {idea.editHistory && idea.editHistory.length > 0 && (
                    <div className="card p-4 shadow-2 border-round-xl">
                        <h6 className="font-bold mb-3 text-900"><i className="pi pi-history text-500 mr-2"></i>Düzenleme Geçmişi ({idea.editHistory.length} {t("details.revision")})</h6>
                        <div className="flex flex-column gap-3">
                            {idea.editHistory.map((history: any, idx: number) => (
                                <div key={idx} className="p-3 border-round-md bg-gray-50 border-1 border-gray-200">
                                    <div className="flex justify-content-between align-items-center mb-3">
                                        <span className="bg-gray-200 text-gray-700 px-2 py-1 border-round text-xs font-semibold white-space-nowrap">{history.fieldName} {t('details.updated')}</span>
                                        <span className="text-500 text-xs">{new Date(history.editedAt).toLocaleString('tr-TR')}</span>
                                    </div>
                                    <div className="grid">
                                        <div className="col-12 md:col-6">
                                            <span className="text-red-500 font-semibold text-xs block mb-1">{t('details.oldValue')}</span>
                                            <div className="p-2 bg-white border-round border-1 border-gray-200 text-500 text-sm" style={{ whiteSpace: 'pre-wrap' }}>{history.oldValue}</div>
                                        </div>
                                        <div className="col-12 md:col-6">
                                            <span className="text-green-500 font-semibold text-xs block mb-1">{t('details.newValue')}</span>
                                            <div className="p-2 bg-white border-round border-1 border-gray-200 text-800 text-sm" style={{ whiteSpace: 'pre-wrap' }}>{history.newValue}</div>
                                        </div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>
                )}
            </div>

            <div className="col-12 lg:col-4">
                <div className="card p-4 shadow-2 border-round-xl mb-4">
                    <h6 className="font-bold mb-3 text-900"><i className="pi pi-user text-primary mr-2"></i>{t("details.author")}</h6>
                    {!idea.author ? (
                        <div className="text-center py-3">
                            <div className="border-circle inline-flex align-items-center justify-content-center mb-2 bg-gray-100 text-500" style={{ width: '50px', height: '50px' }}>
                                <i className="pi pi-user-minus text-xl"></i>
                            </div>
                            <h6 className="font-bold mb-1 text-600">{t('details.hiddenIdentity')}</h6>
                            <p className="text-500 text-xs m-0">{t('details.hiddenReason')}</p>
                        </div>
                    ) : (
                        <div className="text-center py-3">
                            <div className="relative inline-block mb-2">
                                {idea.author.profilePictureUrl ? (
                                    <img src={`${api.defaults.baseURL?.replace('/api', '') || ''}${idea.author.profilePictureUrl}?t=${Date.now()}`} alt="Avatar" className="border-circle" style={{ width: '56px', height: '56px', objectFit: 'cover' }} />
                                ) : (
                                    <div className="border-circle inline-flex align-items-center justify-content-center font-bold text-xl" style={{ width: '56px', height: '56px', backgroundColor: 'var(--primary-100)', color: 'var(--primary-color)' }}>
                                        {idea.author.firstName?.[0]}{idea.author.lastName?.[0]}
                                    </div>
                                )}
                            </div>
                            <h6 className="font-bold mb-0 text-900">{idea.author.firstName} {idea.author.lastName}</h6>
                            <span className="text-500 text-sm block">{t("details.regNo")}: {idea.author.registrationNumber}</span>
                        </div>
                    )}
                </div>

                {idea.canEvaluate && idea.status === 'Pending' && !idea.isAuthor && (
                    <div className="card p-4 shadow-2 border-round-xl mb-4">
                        <h6 className="font-bold mb-3 text-900"><i className="pi pi-check-square text-primary mr-2"></i>{t("details.evalPanel")}</h6>
                        <form onSubmit={submitEvaluation}>
                            <div className="mb-4">
                                <div className="flex justify-content-between align-items-center mb-2">
                                    <label className="text-600 text-sm font-semibold">{t("details.scoreRange")}</label>
                                    <span className={`font-bold text-sm px-2 py-1 border-round ${score >= 50 ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>{score} / 100</span>
                                </div>
                                <input type="range" className={`score-slider ${score === 100 ? 'sparkle-thumb' : ''}`} min="0" max="100" value={score} onChange={(e) => handleScoreChange(parseInt(e.target.value))} />
                                <div className="text-center mt-3 py-2 px-2 border-round-md text-xs font-semibold" style={{ backgroundColor: feedback.bg, color: feedback.color, border: `1px solid ${feedback.border}`, transition: 'all 0.2s ease' }}>
                                    {feedback.text}
                                </div>
                            </div>

                            <div className="mb-3">
                                <label className="text-600 text-sm font-semibold block mb-2">{t("details.decision")}</label>
                                <Dropdown value={decision} options={decisionOptions} onChange={(e) => setDecision(e.value)} className="w-full p-inputtext-sm" />
                            </div>

                            <div className="mb-4">
                                <label className="text-600 text-sm font-semibold block mb-2">{t('details.evalNote')}</label>
                                <InputTextarea rows={3} value={comment} onChange={(e) => setComment(e.target.value)} className="w-full" required onInvalid={(e: any) => e.target.setCustomValidity(t('validation.required'))} onInput={(e: any) => e.target.setCustomValidity('')} />
                            </div>

                            <Button type="submit" label={t('details.submitEval')} severity="success" className="w-full" loading={isSubmitting} />
                        </form>
                    </div>
                )}

                {idea.isAuthor && idea.status === 'Pending' && (
                    <div className="card p-4 shadow-2 border-round-xl mb-4">
                        <h6 className="font-bold mb-2 text-900"><i className="pi pi-info-circle text-orange-500 mr-2"></i>{t('details.scoringRestriction')}</h6>
                        <div className="p-3 border-round-md text-sm bg-orange-50 text-orange-800 border-1 border-orange-200">
                            {t('details.ownIdeaMsg')}
                        </div>
                    </div>
                )}

                {idea.isSuperAdmin && (idea.status === 'Approved' || idea.status === 'Rejected') && (
                    <div className="card p-4 shadow-1 border-round-xl text-center mb-4 border-dashed border-2 border-primary">
                        <h6 className="font-bold mb-2 text-900"><i className="pi pi-shield mr-2"></i>{t('details.adminAuthority')}</h6>
                        <p className="text-500 text-xs mb-3">{t('details.adminReopenDesc')}</p>
                        <Button label={t('details.reopenBtn')} icon="pi pi-sync" severity="secondary" outlined className="w-full" onClick={handleReopen} loading={isSubmitting} />
                    </div>
                )}

                {idea.isEvaluator && idea.status === 'Approved' && !idea.isAuthor && (
                    <div className="card p-4 shadow-2 border-round-xl text-center border-1 border-yellow-400 bg-yellow-50">
                        <h6 className="font-bold mb-2 text-yellow-900"><i className="pi pi-star text-yellow-600 mr-2"></i>{t('details.makeGlittering')}</h6>
                        <p className="text-yellow-700 text-xs mb-3">{t('details.glitteringDesc')}</p>
                        <Button label={t('details.markGlittering')} icon="pi pi-sparkles" severity="warning" className="w-full text-white bg-yellow-500 border-yellow-500 hover:bg-yellow-600" onClick={handleImplement} loading={isSubmitting} />
                    </div>
                )}
            </div>

            <Dialog header={t('details.docPreview')} visible={imageModalVisible} style={{ width: '50vw' }} breakpoints={{ '960px': '75vw', '640px': '100vw' }} onHide={() => setImageModalVisible(false)}>
                <div className="text-center">
                    <img src={currentImageSrc} alt={currentImageName} style={{ maxWidth: '100%', maxHeight: '70vh', objectFit: 'contain' }} />
                </div>
            </Dialog>
        </div>
    );
};

export default IdeaDetailsPage;
