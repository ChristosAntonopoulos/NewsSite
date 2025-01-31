import { useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { useLanguage } from '../contexts/LanguageContext';
import { useTheme } from '../contexts/ThemeContext';
import { Language } from '../services/translationService';

export function useUserPreferences() {
  const { user } = useAuth();
  const { setLanguage } = useLanguage();
  const { theme, toggleTheme } = useTheme();

  useEffect(() => {
    if (user) {
      // Sync language preference
      setLanguage(user.language as Language);
      
      // Only sync theme when user object changes and themes don't match
      const userTheme = user.theme;
      if (userTheme && userTheme !== theme) {
        localStorage.setItem('theme', userTheme);
        // Force a single theme toggle if needed to match user preference
        if ((userTheme === 'dark' && theme === 'light') || 
            (userTheme === 'light' && theme === 'dark')) {
          toggleTheme();
        }
      }
    }
  }, [user]); // Only run when user object changes
} 