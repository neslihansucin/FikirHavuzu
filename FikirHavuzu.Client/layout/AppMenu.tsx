/* eslint-disable @next/next/no-img-element */

import React, { useContext, useState, useEffect } from 'react';
import AppMenuitem from './AppMenuitem';
import { LayoutContext } from './context/layoutcontext';
import { MenuProvider } from './context/menucontext';
import Link from 'next/link';
import { AppMenuItem } from '@/types';
import { useTranslation } from 'react-i18next';

const AppMenu = () => {
    const { layoutConfig } = useContext(LayoutContext);

    const [userPermissions, setUserPermissions] = useState<string[]>([]);

    useEffect(() => {
        const userStr = localStorage.getItem('user');
        if (userStr) {
            try {
                const userObj = JSON.parse(userStr);
                if (userObj.permissions) {
                    setUserPermissions(userObj.permissions);
                }
            } catch (e) {
                console.error("User parse error", e);
            }
        }
    }, []);

    const { t } = useTranslation();

    const isUserManagement = userPermissions.includes('UserManagement');
    const isPermissionManagement = userPermissions.includes('PermissionManagement');
    const isIdeaEvaluation = userPermissions.includes('IdeaEvaluation');

    const canManageIdeas = isIdeaEvaluation || isPermissionManagement;

    const model: AppMenuItem[] = [
        {
            label: t('menu.general', 'Genel'),
            items: [
                { label: t('menu.dashboard'), icon: 'pi pi-fw pi-home', to: '/' },
                { label: t('menu.showcase'), icon: 'pi pi-fw pi-star', to: '/ideas/showcase' }
            ]
        },
        {
            label: t('menu.ideaPool', 'Fikir Havuzu'),
            items: [
                { label: t('menu.newIdea'), icon: 'pi pi-fw pi-plus', to: '/ideas/create' },
                { label: t('menu.myIdeas'), icon: 'pi pi-fw pi-user', to: '/ideas/my' }
            ]
        }
    ];

    if (canManageIdeas) {
        model.push({
            label: t('menu.ideaManagement'),
            items: [
                { label: t('menu.evaluateIdeas'), icon: 'pi pi-fw pi-list', to: '/ideas/list' },
                { label: t('menu.withdrawnIdeas'), icon: 'pi pi-fw pi-history', to: '/ideas/withdrawn' }
            ]
        });
    }

    if (isUserManagement || isPermissionManagement) {
        const adminItems = [];
        if (isUserManagement) adminItems.push({ label: t('menu.users'), icon: 'pi pi-fw pi-users', to: '/users' });
        if (isPermissionManagement) adminItems.push({ label: t('menu.permissions', 'Yetki Yönetimi'), icon: 'pi pi-fw pi-shield', to: '/permissions' });
        
        model.push({
            label: t('menu.systemManagement', 'Sistem Yönetimi'),
            items: adminItems
        });
    }

    model.push({
        label: t('menu.account', 'Hesap'),
        items: [
            { label: t('topbar.profile'), icon: 'pi pi-fw pi-user-edit', to: '/profile' }
        ]
    });

    return (
        <MenuProvider>
            <ul className="layout-menu">
                {model.map((item, i) => {
                    return !item?.seperator ? <AppMenuitem item={item} root={true} index={i} key={item.label} /> : <li className="menu-separator"></li>;
                })}
            </ul>
        </MenuProvider>
    );
};

export default AppMenu;
