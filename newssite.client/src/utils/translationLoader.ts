import Papa from 'papaparse';

type TranslationRow = {
  key: string;
  en: string;
  el: string;
  [key: string]: string;
};

type TranslationsMap = {
  [language: string]: {
    [key: string]: any;
  };
};

export async function loadTranslations(): Promise<TranslationsMap> {
  try {
    const response = await fetch('/src/translations/translations.csv');
    const csvText = await response.text();
    
    const { data } = Papa.parse<TranslationRow>(csvText, {
      header: true,
      skipEmptyLines: true,
    });

    const translations: TranslationsMap = {
      en: {},
      el: {},
    };

    data.forEach((row) => {
      const keys = row.key.split('.');
      
      ['en', 'el'].forEach(lang => {
        let current = translations[lang];
        for (let i = 0; i < keys.length; i++) {
          const key = keys[i];
          if (i === keys.length - 1) {
            current[key] = row[lang];
          } else {
            current[key] = current[key] || {};
            current = current[key];
          }
        }
      });
    });

    return translations;
  } catch (error) {
    console.error('Error loading translations:', error);
    return { en: {}, el: {} };
  }
} 