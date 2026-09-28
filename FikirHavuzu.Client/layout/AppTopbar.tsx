/* eslint-disable @next/next/no-img-element */

import Link from 'next/link';
import { classNames } from 'primereact/utils';
import React, { forwardRef, useContext, useImperativeHandle, useRef } from 'react';
import { AppTopbarRef } from '@/types';
import { ConfirmDialog, confirmDialog } from 'primereact/confirmdialog';
import { LayoutContext } from './context/layoutcontext';
import { useTranslation } from 'react-i18next';

const AppTopbar = forwardRef<AppTopbarRef>((props, ref) => {
    const { layoutConfig, layoutState, onMenuToggle, showProfileSidebar } = useContext(LayoutContext);
    const menubuttonRef = useRef(null);
    const topbarmenuRef = useRef(null);
    const topbarmenubuttonRef = useRef(null);
    
    const [user, setUser] = React.useState<any>(null);

    React.useEffect(() => {
        const loadUser = () => {
            const userStr = localStorage.getItem('user');
            if (userStr) {
                setUser(JSON.parse(userStr));
            }
        };
        loadUser();
        window.addEventListener('profile-updated', loadUser);
        return () => window.removeEventListener('profile-updated', loadUser);
    }, []);

    const { t, i18n } = useTranslation();

    useImperativeHandle(ref, () => ({
        menubutton: menubuttonRef.current,
        topbarmenu: topbarmenuRef.current,
        topbarmenubutton: topbarmenubuttonRef.current
    }));

    const changeLanguage = (lng: string) => {
        i18n.changeLanguage(lng);
    };

    return (
        <div className="layout-topbar">
            <Link href="/" className="layout-topbar-logo flex align-items-center">
                <img src="/favicon.svg" alt="Fikir Havuzu Logo" style={{ width: '26px', height: '26px', borderRadius: '50%' }} className="mr-2" />
                <span className="font-bold text-xl text-900" style={{ letterSpacing: '-0.02em' }}>Fikir Havuzu</span>
            </Link>

            <button ref={menubuttonRef} type="button" className="p-link layout-menu-button layout-topbar-button" onClick={onMenuToggle}>
                <i className="pi pi-bars" />
            </button>

            <button ref={topbarmenubuttonRef} type="button" className="p-link layout-topbar-menu-button layout-topbar-button" onClick={showProfileSidebar}>
                <i className="pi pi-ellipsis-v" />
            </button>

            <div ref={topbarmenuRef} className={classNames('layout-topbar-menu', { 'layout-topbar-menu-mobile-active': layoutState.profileSidebarVisible })}>
                {/* Language Switcher */}
                <div className="flex align-items-center mr-3 gap-2">
                    <button type="button" className={`p-link font-bold text-sm ${i18n.language === 'tr' ? 'text-primary' : 'text-500'}`} onClick={() => changeLanguage('tr')}>TR</button>
                    <span className="text-300">|</span>
                    <button type="button" className={`p-link font-bold text-sm ${i18n.language === 'en' ? 'text-primary' : 'text-500'}`} onClick={() => changeLanguage('en')}>EN</button>
                </div>

                {user && (
                    <span className="flex align-items-center font-semibold text-600 mr-3 hidden lg:flex">
                        {user.firstName} {user.lastName}
                    </span>
                )}
                <Link href="/profile">
                    <button type="button" className="p-link layout-topbar-button" style={{ width: '40px', height: '40px', borderRadius: '50%', overflow: 'hidden', padding: 0 }} title={t('topbar.profile')}>
                        {user?.profilePictureUrl ? (
                            <img src={`${user.profilePictureUrl}?t=${Date.now()}`} alt="Profil" style={{ width: '100%', height: '100%', objectFit: 'cover' }} />
                        ) : (
                            <div className="flex align-items-center justify-content-center bg-primary text-white font-bold" style={{ width: '100%', height: '100%' }}>
                                {user ? `${user.firstName?.[0] || ''}${user.lastName?.[0] || ''}` : <i className="pi pi-user"></i>}
                            </div>
                        )}
                    </button>
                </Link>
                <button type="button" className="p-link layout-topbar-button text-red-500" title={t('topbar.logout')} onClick={(e) => {
                    confirmDialog({
                        target: e.currentTarget,
                        message: t('topbar.logoutConfirm'),
                        header: t('topbar.logout'),
                        icon: 'pi pi-exclamation-triangle',
                        acceptLabel: t('topbar.yes'),
                        rejectLabel: t('topbar.no'),
                        acceptClassName: 'p-button-danger',
                        accept: () => {
                            localStorage.removeItem('token');
                            localStorage.removeItem('user');
                            window.location.href = '/auth/login';
                        }
                    });
                }}>
                    <i className="pi pi-sign-out"></i>
                    <span>{t('topbar.logout')}</span>
                </button>
            </div>
            <ConfirmDialog />
        </div>
    );
});

AppTopbar.displayName = 'AppTopbar';

export default AppTopbar;
