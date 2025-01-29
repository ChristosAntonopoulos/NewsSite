import React, { createContext, useContext, useState, useEffect } from 'react';
import { translationService, Language } from '../services/translationService';

interface LanguageContextType {
  language: Language;
  setLanguage: (lang: Language) => void;
  t: (key: string) => string;
  isLoading: boolean;
}

const LanguageContext = createContext<LanguageContextType | undefined>(undefined);

export const LanguageProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [language, setLanguageState] = useState<Language>(() => {
    const savedLanguage = localStorage.getItem('preferredLanguage');
    return (savedLanguage as Language) || 'en';
  });
  const [isLoading, setIsLoading] = useState(true);

  const setLanguage = (lang: Language) => {
    setLanguageState(lang);
    localStorage.setItem('preferredLanguage', lang);
  };

  useEffect(() => {
    const initializeTranslations = async () => {
      try {
        await translationService.initialize();
      } catch (error) {
        console.error('Failed to initialize translations:', error);
      } finally {
        setIsLoading(false);
      }
    };

    initializeTranslations();
  }, []);

  const t = (key: string): string => {
    return translationService.translate(key, language);
  };

  const value = {
    language,
    setLanguage,
    t,
    isLoading
  };

  return (
    <LanguageContext.Provider value={value}>
      {children}
    </LanguageContext.Provider>
  );
};

export const useLanguage = () => {
  const context = useContext(LanguageContext);
  if (context === undefined) {
    throw new Error('useLanguage must be used within a LanguageProvider');
  }
  return context;
}; 