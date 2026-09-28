'use client';
import React, { useEffect, useState, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { useRouter } from 'next/navigation';
import { Button } from 'primereact/button';
import { InputText } from 'primereact/inputtext';
import { Dropdown } from 'primereact/dropdown';
import { Dialog } from 'primereact/dialog';
import { Toast } from 'primereact/toast';
import { Avatar } from 'primereact/avatar';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import api from '../../../../utils/api';
import Link from 'next/link';

const ShowcasePage = () => { 
    const { t } = useTranslation();
    const router = useRouter();
    const toast = useRef<Toast>(null);
    const [ideas, setIdeas] = useState([]);
    const [categories, setCategories] = useState([]);
    const [allInnovators, setAllInnovators] = useState<any[]>([]);
    const [canManageShowcase, setCanManageShowcase] = useState(false);
    const [loading, setLoading] = useState(true);
    const [currentUserId, setCurrentUserId] = useState(0);

    // Filters
    const [search, setSearch] = useState('');
    const [categoryId, setCategoryId] = useState<number | null>(null);
    const [sort, setSort] = useState('date_desc');

    // Modals
    const [scoringModalVisible, setScoringModalVisible] = useState(false);
    const [leaderboardModalVisible, setLeaderboardModalVisible] = useState(false);

    // Add Glittering
    const [addModalVisible, setAddModalVisible] = useState(false);
    const [approvedIdeas, setApprovedIdeas] = useState<any[]>([]);
    const [selectedApprovedIdea, setSelectedApprovedIdea] = useState<number | null>(null);
    const [isSavingGlittering, setIsSavingGlittering] = useState(false);

    const openAddGlitteringModal = async () => {
        setAddModalVisible(true);
        try {
            const response = await api.get('/ideas/list?filter=approved&sort=date_desc');
            setApprovedIdeas(response.data.ideas);
        } catch (e) {
            console.error("Approved fetch error", e);
        }
    };

    const handleImplementIdea = async () => {
        if (!selectedApprovedIdea) return;
        setIsSavingGlittering(true);
        try {
            const res = await api.post(`/ideas/${selectedApprovedIdea}/implement`);
            toast.current?.show({ severity: 'success', summary: t('toast.success'), detail: t('showcase.implementSuccess') });
            setAddModalVisible(false);
            setSelectedApprovedIdea(null);
            fetchShowcaseData(); // Refresh showcase
        } catch (error: any) {
            toast.current?.show({ severity: 'error', summary: t('toast.error'), detail: t('toast.genericError') });
        } finally {
            setIsSavingGlittering(false);
        }
    };


    const sortOptions = [
        { label: t('showcase.sortNewest'), value: 'date_desc' },
        { label: t('showcase.sortHighest'), value: 'score_desc' },
        { label: t('showcase.sortTitle'), value: 'title_asc' }
    ];

    const fetchShowcaseData = async () => {
        setLoading(true);
        try {
            // Fetch filtered ideas
            let url = `/ideas/showcase?sort=${sort}`;
            if (search) url += `&search=${encodeURIComponent(search)}`;
            if (categoryId) url += `&categoryId=${categoryId}`;
            
            const response = await api.get(url);
            setIdeas(response.data.ideas);
            setCategories(response.data.categories.map((c: any) => ({ label: c.name, value: c.id })));
            setCanManageShowcase(response.data.canManageShowcase);
            
            // To calculate the full leaderboard accurately regardless of active filters,
            // we fetch all implemented ideas without filters once (if not already fetched)
            if (allInnovators.length === 0) {
                const allResp = await api.get('/ideas/showcase?sort=date_desc');
                const allIdeas = allResp.data.ideas;
                if (allIdeas.length > 0) {
                    setCurrentUserId(allResp.data.ideas[0].userId); // Approximation, or check localstorage
                }
                
                // Group by authorName
                const grouped = allIdeas.reduce((acc: any, curr: any) => {
                    const name = curr.authorName;
                    if (!acc[name]) {
                        acc[name] = { name, count: 0, points: 0, jurySum: 0, isMe: curr.isOwner };
                    }
                    acc[name].count += 1;
                    acc[name].points += 150;
                    acc[name].jurySum += (curr.latestScore || 0);
                    return acc;
                }, {});

                const innovatorsArray = Object.values(grouped).sort((a: any, b: any) => {
                    if (b.points !== a.points) return b.points - a.points;
                    return b.jurySum - a.jurySum;
                });
                setAllInnovators(innovatorsArray);
            }
        } catch (error) {
            console.error(t('showcase.fetchError'), error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        // Find current user id from localstorage to highlight "(Siz)"
        const userStr = localStorage.getItem('user');
        if (userStr) {
            try {
                setCurrentUserId(JSON.parse(userStr).id);
            } catch (e) { }
        }
        fetchShowcaseData();
    }, [search, categoryId, sort]);

    const getInitials = (name: string) => {
        if (!name || name === t('showcase.hiddenUser')) return "U";
        const parts = name.split(' ');
        if (parts.length >= 2) return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
        return name[0].toUpperCase();
    };

    const topInnovators = allInnovators.slice(0, 3);

    return (
        <div className="grid">
            <Toast ref={toast} />
            
            <div className="col-12">
                <div className="flex flex-wrap justify-content-between align-items-center mb-4 gap-3">
                    <div>
                        <h3 className="font-bold mb-1" style={{ color: 'var(--text-color)', letterSpacing: '-0.02em' }}>{t('showcase.title')}</h3>
                        <p className="m-0 text-600" style={{ fontSize: '0.95rem' }}>{t('showcase.description')}</p>
                    </div>
                    <div className="flex align-items-center gap-2">
                        {canManageShowcase && (
        <Button label={t('showcase.addGlittering')} icon="pi pi-plus" severity="warning" size="small" onClick={openAddGlitteringModal} />
    )}
    <span className="bg-orange-100 text-orange-700 px-3 py-2 border-round-md font-semibold text-sm border-1 border-orange-200 white-space-nowrap">
                            <i className="pi pi-trophy mr-1"></i> {t('showcase.totalIdeas', { count: allInnovators.reduce((sum, i) => sum + i.count, 0) })}
                        </span>
                    </div>
                </div>

                {topInnovators.length > 0 && (
                    <>
                        <div className="mb-4 p-3 border-round-xl border-1" style={{ backgroundColor: '#F0FDF4', borderColor: '#BBF7D0' }}>
                            <div className="flex gap-3 align-items-center">
                                <div className="text-4xl">🎁</div>
                                <div>
                                    <h6 className="font-bold mb-1" style={{ color: '#166534' }}>{t('showcase.awardTitle')}</h6>
                                    <p className="m-0 text-sm" style={{ color: '#15803D' }}> <span dangerouslySetInnerHTML={{ __html: t('showcase.awardDescription') }} /> </p>
                                </div>
                            </div>
                        </div>

                        <div className="card mb-4 p-4 shadow-2 border-round-xl">
                            <div className="flex align-items-center justify-content-between mb-4 flex-wrap gap-2">
                                <div className="flex align-items-center gap-2">
                                    <span className="text-500 font-semibold text-sm uppercase" style={{ letterSpacing: '0.04em' }}>{t('showcase.podiumTitle')}</span>
                                    <Button icon="pi pi-question-circle" rounded text severity="warning" onClick={() => setScoringModalVisible(true)} tooltip={t('showcase.podiumInfoTooltip')} className="p-0 w-2rem h-2rem" />
                                </div>
                                <span className="text-500 text-xs">{t('showcase.podiumSubTitle')}</span>
                            </div>

                            <div className="grid">
                                {topInnovators.map((innovator, idx) => {
                                    const medal = idx === 0 ? "🥇" : idx === 1 ? "🥈" : "🥉";
                                    const badgeLabel = idx === 0 ? t('showcase.innovationLeader') : idx === 1 ? t('showcase.glitteringInnovator') : t('showcase.innovativeThinker');
                                    const cardBg = idx === 0 ? "#FEF9C3" : "#F8FAFC";
                                    const cardBorder = idx === 0 ? "#FDE047" : "var(--surface-border)";
                                    const shadow = idx === 0 ? "0 4px 12px rgba(234, 179, 8, 0.15)" : "none";

                                    return (
                                        <div className="col-12 md:col-4" key={idx}>
                                            <div className="flex align-items-center p-3 border-round-xl h-full border-1" style={{ backgroundColor: cardBg, borderColor: cardBorder, boxShadow: shadow }}>
                                                <span className="mr-3 text-4xl">{medal}</span>
                                                <Avatar label={getInitials(innovator.name)} size="large" shape="circle" className="mr-3 shadow-1 font-bold bg-blue-100 text-blue-600 border-2 border-white flex-shrink-0" />
                                                <div>
                                                    <span className="bg-orange-100 text-orange-800 px-2 py-1 border-round-md font-semibold text-xs inline-block mb-1 white-space-nowrap">{badgeLabel}</span>
                                                    <div className="font-bold text-900 text-sm">
                                                        {innovator.name} {innovator.isMe && <span className="text-500 text-xs font-semibold ml-1">{t('showcase.you')}</span>}
                                                    </div>
                                                    <div className="text-600 text-xs mt-1">
                                                        {t('showcase.pointsBadge', { points: innovator.points })} <span className="text-500">({t('showcase.juryScoreLabel')} {innovator.jurySum})</span>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    );
                                })}
                            </div>

                            {allInnovators.length > 0 && (
                                <div className="mt-4 text-center">
                                    <Button label={t('showcase.showAllRanking', { count: allInnovators.length })} icon="pi pi-chevron-down" iconPos="right" severity="secondary" outlined rounded size="small" onClick={() => setLeaderboardModalVisible(true)} />
                                </div>
                            )}
                        </div>
                    </>
                )}

                {/* Filters */}
                <div className="card mb-4 p-3 shadow-1 border-round-xl">
                    <div className="grid formgrid p-fluid align-items-center">
                        <div className="col-12 md:col-5">
                            <span className="p-input-icon-left">
                                <i className="pi pi-search" />
                                <InputText placeholder={t('showcase.searchPlaceholder')} value={search} onChange={(e) => setSearch(e.target.value)} className="p-inputtext-sm" />
                            </span>
                        </div>
                        <div className="col-12 md:col-3">
                            <Dropdown value={categoryId} options={categories} onChange={(e) => setCategoryId(e.value)} placeholder={t('showcase.allCategories')} showClear className="p-inputtext-sm" />
                        </div>
                        <div className="col-12 md:col-3">
                            <Dropdown value={sort} options={sortOptions} onChange={(e) => setSort(e.value)} className="p-inputtext-sm" />
                        </div>
                        <div className="col-12 md:col-1 text-right">
                            {(search || categoryId || sort !== 'date_desc') && (
                                <Button icon="pi pi-times" severity="secondary" outlined tooltip={t('showcase.clearFilters')} onClick={() => { setSearch(''); setCategoryId(null); setSort('date_desc'); }} className="p-button-sm w-full" />
                            )}
                        </div>
                    </div>
                </div>

                {/* Idea List */}
                {!loading && ideas.length === 0 ? (
                    <div className="card p-5 text-center shadow-1 border-round-xl">
                        <span className="text-6xl block mb-3">✨</span>
                        <h5 className="font-bold text-900 mb-2">{t('showcase.noMatchTitle')}</h5>
                        <p className="text-500 mx-auto mb-4" style={{ maxWidth: '500px', fontSize: '0.95rem' }}> {t('showcase.noMatchDesc')} </p>
                        <Link href="/ideas/create">
                            <Button label={t('showcase.newIdeaBtn')} icon="pi pi-plus" severity="success" />
                        </Link>
                    </div>
                ) : (
                    <div className="flex flex-column gap-3">
                        {ideas.map((idea: any) => (
                            <div key={idea.id} className="card p-4 shadow-1 border-round-xl mb-0">
                                <div className="flex flex-wrap justify-content-between align-items-start gap-2 mb-3">
                                    <div>
                                        <div className="flex align-items-center gap-2 mb-2 flex-wrap">
                                            <span className="bg-gray-100 text-gray-700 px-2 py-1 border-round text-xs font-semibold white-space-nowrap">{t(idea.categoryName === 'Hizmet Geliştirme' ? 'category.service' : idea.categoryName === 'Ürün İyileştirme' ? 'category.product' : idea.categoryName === 'Süreç & Verimlilik' ? 'category.process' : idea.categoryName ? idea.categoryName : 'category.other')}</span>
                                            <span className="bg-orange-100 text-orange-700 px-2 py-1 border-round text-xs font-semibold white-space-nowrap"><i className="pi pi-star-fill mr-1"></i> {t('showcase.glitteringBadge')}</span>
                                            {idea.latestScore && <span className="bg-green-100 text-green-800 px-2 py-1 border-round text-xs font-semibold white-space-nowrap">{t('showcase.juryScoreBadge', { score: idea.latestScore })}</span>}
                                        </div>
                                        <h5 className="font-bold m-0 text-900">{idea.title}</h5>
                                    </div>
                                    <Button label={t('showcase.detailBtn')} severity="secondary" outlined size="small" onClick={() => router.push(`/ideas/details/${idea.id}`)} />
                                </div>

                                <div className="p-3 border-round-md mb-3 bg-gray-50 border-1 border-gray-200">
                                    <span className="font-semibold block mb-1 text-xs text-600">{t('showcase.benefitLabel')}</span>
                                    <div className="text-sm text-800">{idea.intendedBenefit}</div>
                                </div>

                                <p className="mb-3 text-sm text-700" style={{ lineHeight: '1.6' }}>{idea.description}</p>

                                <div className="pt-3 flex align-items-center justify-content-between border-top-1 surface-border">
                                    <div className="flex align-items-center gap-2">
                                        <Avatar 
                                            image={idea.authorProfilePictureUrl ? `${api.defaults.baseURL?.replace('/api', '') || ''}${idea.authorProfilePictureUrl}?t=${Date.now()}` : undefined} 
                                            label={!idea.authorProfilePictureUrl ? getInitials(idea.authorName) : undefined} 
                                            shape="circle" className="bg-blue-100 text-blue-600 text-xs font-bold flex-shrink-0" 
                                        />
                                        <span className="text-sm font-semibold text-800">{idea.authorName}</span>
                                        {idea.isOwner && <span className="text-500 text-xs font-semibold">{t('showcase.you')}</span>}
                                    </div>
                                    <span className="text-500 text-xs">{new Date(idea.createdAt).toLocaleDateString('tr-TR')}</span>
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </div>

            {/* Scoring Info Dialog */}
            <Dialog visible={scoringModalVisible} onHide={() => setScoringModalVisible(false)} header={t('showcase.guideTitle')} style={{ width: '450px' }} className="border-round-xl">
                <div className="p-1">
                    <h6 className="font-bold mb-2 text-900"><i className="pi pi-star text-yellow-500 mr-2"></i>{t('showcase.guideInnovationTitle')}</h6>
                    <p className="text-sm text-600 mb-4 line-height-3"> <span dangerouslySetInnerHTML={{ __html: t('showcase.guideInnovationDesc1') }} /><br /> <span dangerouslySetInnerHTML={{ __html: t('showcase.guideInnovationDesc2') }} /> </p>

                    <h6 className="font-bold mb-2 text-900"><i className="pi pi-sliders-h text-green-500 mr-2"></i>{t('showcase.guideJuryTitle')}</h6>
                    <p className="text-sm text-600 mb-4 line-height-3"> {t('showcase.guideJuryDesc')} </p>

                    <div className="p-3 border-round-md bg-gray-50 border-1 border-gray-200">
                        <h6 className="font-bold mb-2 text-sm text-900"><i className="pi pi-trophy text-yellow-500 mr-2"></i>{t('showcase.guideRuleTitle')}</h6>
                        <ol className="text-sm text-600 m-0 pl-3 line-height-3">
                            <li className="mb-1" dangerouslySetInnerHTML={{ __html: t('showcase.guideRule1') }}></li>
                            <li className="mb-1" dangerouslySetInnerHTML={{ __html: t('showcase.guideRule2') }}></li>
                            <li dangerouslySetInnerHTML={{ __html: t('showcase.guideRule3') }}></li>
                        </ol>
                    </div>
                </div>
            </Dialog>

            {/* Leaderboard Dialog */}
            <Dialog visible={leaderboardModalVisible} onHide={() => setLeaderboardModalVisible(false)} header={t('showcase.leaderboardTitle')} style={{ width: '700px' }} breakpoints={{ '960px': '90vw' }} className="border-round-xl">
                <DataTable value={allInnovators} className="p-datatable-sm" rowHover stripedRows
                    rowClassName={(rowData) => {
                        const idx = allInnovators.indexOf(rowData);
                        return idx === 0 ? 'bg-yellow-50' : idx === 1 ? 'bg-gray-50' : idx === 2 ? 'bg-orange-50' : '';
                    }}>
                    <Column header={t('showcase.rankCol')} body={(rowData, options) => {
                        const rank = options.rowIndex + 1;
                        if (rank === 1) return <span className="font-bold text-lg">🥇 1.</span>;
                        if (rank === 2) return <span className="font-bold text-lg">🥈 2.</span>;
                        if (rank === 3) return <span className="font-bold text-lg">🥉 3.</span>;
                        return <span className="font-bold text-500">#{rank}</span>;
                    }} style={{ width: '10%' }} align="center" />
                    
                    <Column header={t('showcase.employeeCol')} body={(rowData) => (
                        <div className="flex align-items-center gap-2">
                            <Avatar label={getInitials(rowData.name)} shape="circle" className="bg-blue-100 text-blue-600 text-xs font-bold flex-shrink-0" />
                            <span className={`text-sm ${allInnovators.indexOf(rowData) < 3 ? 'font-bold text-900' : 'font-semibold text-700'}`}>
                                {rowData.name} {rowData.isMe && <span className="text-500 text-xs ml-1">{t('showcase.you')}</span>}
                            </span>
                        </div>
                    )} style={{ width: '40%' }} />

                    <Column header={t('showcase.ideaCountCol')} body={(rowData) => (
                        <span className="bg-orange-100 text-orange-800 px-2 py-1 border-round-md text-xs font-bold white-space-nowrap">{t('showcase.ideaCountBadge', { count: rowData.count })}</span>
                    )} style={{ width: '20%' }} align="center" />

                    <Column header={t('showcase.juryScoreCol')} field="jurySum" style={{ width: '15%' }} align="center" bodyClassName="text-500 text-sm font-semibold" />

                    <Column header={t('showcase.innovationScoreCol')} body={(rowData) => (
                        <span className="bg-yellow-100 text-yellow-800 px-2 py-1 border-round-md text-xs font-bold border-1 border-yellow-400 white-space-nowrap">
                            {t('showcase.pointsBadge', { points: rowData.points })}
                        </span>
                    )} style={{ width: '15%' }} align="right" />
                </DataTable>
            </Dialog>

            {/* Add Glittering Idea Dialog */}
            <Dialog visible={addModalVisible} onHide={() => { setAddModalVisible(false); setSelectedApprovedIdea(null); }} header={t('showcase.addGlittering')} style={{ width: '500px' }} breakpoints={{ '960px': '90vw' }} className="border-round-xl">
                <div className="p-1">
                    <p className="text-600 mb-4">{t('showcase.selectApprovedIdea')}</p>
                    {approvedIdeas.length === 0 ? (
                        <div className="p-3 bg-orange-50 text-orange-800 border-round">{t('showcase.noApprovedIdeas')}</div>
                    ) : (
                        <Dropdown 
                            value={selectedApprovedIdea} 
                            options={approvedIdeas} 
                            onChange={(e) => setSelectedApprovedIdea(e.value)} 
                            optionLabel="title" 
                            optionValue="id" 
                            placeholder={t('showcase.selectApprovedIdea')} 
                            className="w-full mb-4" 
                            filter 
                        />
                    )}
                    
                    <div className="flex justify-content-end gap-2 mt-4">
                        <Button label={t('showcase.cancel')} severity="secondary" outlined onClick={() => setAddModalVisible(false)} />
                        <Button label={t('showcase.saveGlittering')} severity="warning" onClick={handleImplementIdea} disabled={!selectedApprovedIdea} loading={isSavingGlittering} />
                    </div>
                </div>
            </Dialog>

        </div>
    );
};

export default ShowcasePage;
