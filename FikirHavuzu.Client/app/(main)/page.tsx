'use client';
import React, { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { ProgressSpinner } from 'primereact/progressspinner';
import api from '../../utils/api';
import Link from 'next/link';
import { Button } from 'primereact/button';

const Dashboard = () => {
    const { t } = useTranslation();
    const [data, setData] = useState<any>(null);
    const [loading, setLoading] = useState(true);
    const [userName, setUserName] = useState('');

    useEffect(() => {
        const userStr = localStorage.getItem('user');
        if (userStr) {
            try {
                const userObj = JSON.parse(userStr);
                setUserName(`${userObj.firstName} ${userObj.lastName}`);
            } catch (e) { }
        }

        const fetchDashboard = async () => {
            try {
                const response = await api.get('/dashboard');
                setData(response.data);
            } catch (error) {
                console.error("Dashboard yüklenirken hata:", error);
            } finally {
                setLoading(false);
            }
        };

        fetchDashboard();
    }, []);

    if (loading) {
        return (
            <div className="flex justify-content-center align-items-center min-h-screen">
                <ProgressSpinner />
            </div>
        );
    }

    if (!data) return <div>{t('details.notFound')}</div>;

    const renderWelcomeText = () => {
        if (data.isPermissionManagement) {
            return t('dashboard.welcomeAdmin');
        }
        if (data.isUserManagement && data.isIdeaEvaluation) {
            return t('dashboard.welcomeManagerAndEvaluator');
        }
        if (data.isUserManagement) {
            return t('dashboard.welcomeManager');
        }
        if (data.isIdeaEvaluation) {
            return t('dashboard.welcomeEvaluator');
        }
        return t('dashboard.welcomeDefault');
    };

    return (
        <div className="grid">
            <div className="col-12 mb-2">
                <h3 className="font-bold mb-1" style={{ color: 'var(--text-color)', letterSpacing: '-0.02em' }}>
                    👋 {t("dashboard.welcome")}, {userName}!
                </h3>
                <p className="m-0 text-600" style={{ fontSize: '0.95rem' }}>
                    {renderWelcomeText()}
                </p>
            </div>

            {/* YÖNETİCİ/JÜRİ GÖRÜNÜMÜ */}
            {data.isManagementUser && (
                <>
                    {data.isUserManagement && (
                        <div className="col-12 md:col-4">
                            <div className="card h-full flex flex-column justify-content-between mb-0 border-round-xl shadow-2">
                                <div className="flex justify-content-between align-items-start">
                                    <div>
                                        <span className="text-uppercase font-semibold text-500" style={{ fontSize: '0.75rem', letterSpacing: '0.04em' }}>{t('dashboard.defineEmployee')}</span>
                                        <h2 className="font-bold my-2 text-900" style={{ letterSpacing: '-0.03em' }}>{data.totalUsers}</h2>
                                    </div>
                                    <div className="p-2 border-round-md bg-green-100 text-green-600">
                                        <i className="pi pi-users text-2xl"></i>
                                    </div>
                                </div>
                                <div className="pt-3 mt-3 border-top-1 surface-border">
                                    <Link href="/users">
                                        <Button label={t('dashboard.view')} severity="secondary" outlined className="w-full p-button-sm" />
                                    </Link>
                                </div>
                            </div>
                        </div>
                    )}

                    {data.isIdeaEvaluation && (
                        <>
                            <div className="col-12 md:col-4">
                                <div className="card h-full flex flex-column justify-content-between mb-0 border-round-xl shadow-2">
                                    <div className="flex justify-content-between align-items-start">
                                        <div>
                                            <span className="text-uppercase font-semibold text-500" style={{ fontSize: '0.75rem', letterSpacing: '0.04em' }}>{t('dashboard.totalIdeas')}</span>
                                            <h2 className="font-bold my-2 text-900" style={{ letterSpacing: '-0.03em' }}>{data.totalIdeas}</h2>
                                        </div>
                                        <div className="p-2 border-round-md bg-blue-100 text-blue-600">
                                            <i className="pi pi-star text-2xl"></i>
                                        </div>
                                    </div>
                                    <div className="pt-3 mt-3 border-top-1 surface-border">
                                        <Link href="/ideas/list">
                                            <Button label={t('dashboard.view')} severity="secondary" outlined className="w-full p-button-sm" />
                                        </Link>
                                    </div>
                                </div>
                            </div>

                            <div className="col-12 md:col-4">
                                <div className="card h-full flex flex-column justify-content-between mb-0 border-round-xl shadow-2">
                                    <div className="flex justify-content-between align-items-start">
                                        <div>
                                            <span className="text-uppercase font-semibold text-500" style={{ fontSize: '0.75rem', letterSpacing: '0.04em' }}>{t('dashboard.pendingIdeas')}</span>
                                            <h2 className="font-bold my-2 text-900" style={{ letterSpacing: '-0.03em' }}>{data.pendingIdeasCount}</h2>
                                        </div>
                                        <div className="p-2 border-round-md bg-orange-100 text-orange-600">
                                            <i className="pi pi-hourglass text-2xl"></i>
                                        </div>
                                    </div>
                                    <div className="pt-3 mt-3 border-top-1 surface-border">
                                        <Link href="/ideas/list?filter=pending">
                                            <Button label={t('dashboard.evaluate')} severity="success" className="w-full p-button-sm" />
                                        </Link>
                                    </div>
                                </div>
                            </div>
                        </>
                    )}

                    <div className="col-12 lg:col-8 mt-3">
                        <div className="card shadow-2 border-round-xl">
                            <h5 className="font-bold mb-2 text-900"><i className="pi pi-th-large mr-2 text-500"></i> {t('dashboard.quickManagementPanel')}</h5>
                            <p className="text-600" style={{ fontSize: '0.9rem' }}>{t('dashboard.managementPanelDesc')}</p>
                            <div className="flex flex-wrap gap-2 mt-4">
                                {data.isUserManagement && (
                                    <Link href="/users/create">
                                        <Button label={t('dashboard.defineEmployee')} icon="pi pi-user-plus" severity="success" />
                                    </Link>
                                )}
                                {data.isIdeaEvaluation && (
                                    <Link href="/ideas/list?filter=pending">
                                        <Button label={t('dashboard.ratePendingIdeas')} icon="pi pi-check-circle" severity="secondary" outlined />
                                    </Link>
                                )}
                                {data.isPermissionManagement && (
                                    <Link href="/permissions">
                                        <Button label={t('dashboard.assignPermission')} icon="pi pi-shield" severity="secondary" outlined />
                                    </Link>
                                )}
                            </div>
                        </div>
                    </div>
                </>
            )}

            {/* PERSONEL GÖRÜNÜMÜ */}
            {!data.isManagementUser && (
                <>
                    <div className="col-12 md:col-4">
                        <div className="card h-full flex flex-column justify-content-between mb-0 border-round-xl shadow-2">
                            <div className="flex justify-content-between align-items-start">
                                <div>
                                    <span className="text-uppercase font-semibold text-500" style={{ fontSize: '0.75rem', letterSpacing: '0.04em' }}>{t('dashboard.myIdeas')}</span>
                                    <h2 className="font-bold my-2 text-900" style={{ letterSpacing: '-0.03em' }}>{data.myTotalIdeas}</h2>
                                </div>
                                <div className="p-2 border-round-md bg-blue-100 text-blue-600">
                                    <i className="pi pi-book text-2xl"></i>
                                </div>
                            </div>
                            <div className="pt-3 mt-3 border-top-1 surface-border">
                                <Link href="/ideas/my">
                                    <Button label={t('dashboard.view')} severity="secondary" outlined className="w-full p-button-sm" />
                                </Link>
                            </div>
                        </div>
                    </div>

                    <div className="col-12 md:col-4">
                        <div className="card h-full flex flex-column justify-content-between mb-0 border-round-xl shadow-2">
                            <div className="flex justify-content-between align-items-start">
                                <div>
                                    <span className="text-uppercase font-semibold text-500" style={{ fontSize: '0.75rem', letterSpacing: '0.04em' }}>{t('dashboard.myApprovedIdeas')}</span>
                                    <h2 className="font-bold my-2 text-green-600" style={{ letterSpacing: '-0.03em' }}>{data.myApprovedIdeas}</h2>
                                </div>
                                <div className="p-2 border-round-md bg-green-100 text-green-600">
                                    <i className="pi pi-check-circle text-2xl"></i>
                                </div>
                            </div>
                            <div className="pt-3 mt-3 border-top-1 surface-border">
                                <Link href="/ideas/my">
                                    <Button label={t('dashboard.follow')} severity="secondary" outlined className="w-full p-button-sm" />
                                </Link>
                            </div>
                        </div>
                    </div>

                    <div className="col-12 md:col-4">
                        <div className="card h-full flex flex-column justify-content-between mb-0 border-round-xl shadow-2">
                            <div className="flex justify-content-between align-items-start">
                                <div>
                                    <span className="text-uppercase font-semibold text-500" style={{ fontSize: '0.75rem', letterSpacing: '0.04em' }}>{t('dashboard.inEvaluation')}</span>
                                    <h2 className="font-bold my-2 text-orange-600" style={{ letterSpacing: '-0.03em' }}>{data.myPendingIdeas}</h2>
                                </div>
                                <div className="p-2 border-round-md bg-orange-100 text-orange-600">
                                    <i className="pi pi-clock text-2xl"></i>
                                </div>
                            </div>
                            <div className="pt-3 mt-3 border-top-1 surface-border">
                                <Link href="/ideas/my">
                                    <Button label={t('dashboard.watchProcess')} severity="secondary" outlined className="w-full p-button-sm" />
                                </Link>
                            </div>
                        </div>
                    </div>

                    <div className="col-12 lg:col-8 mt-3">
                        <div className="card shadow-2 border-round-xl">
                            <h5 className="font-bold mb-2 text-900"><i className="pi pi-star mr-2 text-yellow-500"></i> {t('dashboard.haveIdea')}</h5>
                            <p className="text-600" style={{ fontSize: '0.9rem' }}>
                                {t('dashboard.haveIdeaDesc')}
                            </p>
                            <div className="mt-4">
                                <Link href="/ideas/create">
                                    <Button label={t('dashboard.sendNewIdea')} icon="pi pi-plus-circle" severity="success" />
                                </Link>
                            </div>
                        </div>
                    </div>
                </>
            )}
        </div>
    );
};

export default Dashboard;