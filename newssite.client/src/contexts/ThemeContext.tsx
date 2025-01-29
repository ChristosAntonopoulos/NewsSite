import React, { createContext, useContext, useState, useEffect, useRef } from 'react';

type Theme = 'light' | 'dark';

interface ThemeContextType {
  theme: Theme;
  toggleTheme: () => void;
}

const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

// Debug function
const debug = (message: string, ...args: any[]) => {
  console.log(`[Theme] ${message}`, ...args);
};

export function ThemeProvider({ children }: { children: React.ReactNode }) {
  const [theme, setTheme] = useState<Theme>(() => {
    const savedTheme = localStorage.getItem('theme');
    debug('Initial theme from localStorage:', savedTheme);
    return (savedTheme === 'dark' || savedTheme === 'light') ? savedTheme : 'light';
  });

  const isUpdating = useRef(false);

  // Apply theme changes
  useEffect(() => {
    if (isUpdating.current) {
      return;
    }

    const applyTheme = () => {
      debug('Applying theme:', theme);
      
      // Remove both classes first
      document.documentElement.classList.remove('light', 'dark');
      // Add new theme class
      document.documentElement.classList.add(theme);
      // Save to localStorage
      localStorage.setItem('theme', theme);
      
      debug('Current classList:', document.documentElement.classList.toString());
    };

    isUpdating.current = true;
    applyTheme();
    
    // Use RAF to ensure DOM updates are complete
    requestAnimationFrame(() => {
      isUpdating.current = false;
    });
  }, [theme]);

  const toggleTheme = React.useCallback(() => {
    if (isUpdating.current) {
      debug('Theme update in progress, skipping toggle');
      return;
    }

    debug('Toggle theme called, current theme:', theme);
    setTheme(currentTheme => {
      const newTheme = currentTheme === 'light' ? 'dark' : 'light';
      debug('Setting new theme to:', newTheme);
      return newTheme;
    });
  }, [theme]);

  const value = React.useMemo(() => ({
    theme,
    toggleTheme
  }), [theme, toggleTheme]);

  return (
    <ThemeContext.Provider value={value}>
      {children}
    </ThemeContext.Provider>
  );
}

export const useTheme = () => {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error('useTheme must be used within a ThemeProvider');
  }
  return context;
}; 