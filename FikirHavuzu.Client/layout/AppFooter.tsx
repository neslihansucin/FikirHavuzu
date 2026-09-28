/* eslint-disable @next/next/no-img-element */

import React, { useContext } from 'react';
import { LayoutContext } from './context/layoutcontext';
import { useTranslation } from 'react-i18next';

const AppFooter = () => {
    const { layoutConfig } = useContext(LayoutContext);
    const { t } = useTranslation();

    return (
        <div className="layout-footer flex align-items-center justify-content-center gap-2">
            <img src="/favicon.svg" alt="Logo" style={{ width: '20px', height: '20px', borderRadius: '50%', opacity: 0.9 }} />
            <span className="text-sm">
                &copy; {new Date().getFullYear()} - {t('footer.brand', 'Fikir Havuzu - Staj Projesi')}
            </span>
        </div>
    );
};

export default AppFooter;
