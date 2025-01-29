import React from 'react';
import { useLanguage } from '../../../contexts/LanguageContext';

export function LanguageSwitcher() {
    const { language, setLanguage } = useLanguage();

    const handleChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        const newLanguage = e.target.value;
        if (newLanguage === 'en' || newLanguage === 'el') {
            setLanguage(newLanguage);
        }
    };

    return (
        <select
            value={language}
            onChange={handleChange}
            className="bg-gray-800 text-gray-300 rounded-md text-sm font-medium focus:outline-none focus:ring-2 focus:ring-cyan-500"
        >
            <option value="en">English</option>
            <option value="el">Ελληνικά</option>
        </select>
    );
} 