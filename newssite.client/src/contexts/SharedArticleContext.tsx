import React, { createContext, useContext, useState } from 'react';

interface SharedArticleContextType {
  sharedArticleId: string | null;
  setSharedArticleId: (id: string | null) => void;
}

const SharedArticleContext = createContext<SharedArticleContextType | undefined>(undefined);

export function SharedArticleProvider({ children }: { children: React.ReactNode }) {
  const [sharedArticleId, setSharedArticleId] = useState<string | null>(null);

  return (
    <SharedArticleContext.Provider value={{ sharedArticleId, setSharedArticleId }}>
      {children}
    </SharedArticleContext.Provider>
  );
}

export function useSharedArticle() {
  const context = useContext(SharedArticleContext);
  if (context === undefined) {
    throw new Error('useSharedArticle must be used within a SharedArticleProvider');
  }
  return context;
} 