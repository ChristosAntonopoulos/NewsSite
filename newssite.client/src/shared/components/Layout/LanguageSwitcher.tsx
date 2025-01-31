import { useLanguage } from '../../../contexts/LanguageContext';

export function LanguageSwitcher() {
  const { language, setLanguage } = useLanguage();

  return (
    <select
      value={language}
      onChange={(e) => setLanguage(e.target.value as 'en' | 'el')}
      className="bg-gray-700 text-white rounded-md px-2 py-1 text-sm"
    >
      <option value="en">English</option>
      <option value="el">Ελληνικά</option>
    </select>
  );
} 