import i18n from "i18next";
import { initReactI18next } from "react-i18next";
import LanguageDetector from "i18next-browser-languagedetector";

import translationTR from "../public/locales/tr/common.json";
import translationEN from "../public/locales/en/common.json";

const resources = {
  tr: {
    translation: translationTR
  },
  en: {
    translation: translationEN
  }
};

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources,
    fallbackLng: "tr",
    debug: false,
    interpolation: {
      escapeValue: false
    }
  });

export default i18n;

