import { useState, useEffect, useCallback } from 'react';

export function useSavedArticles() {
  const [savedArticleIds, setSavedArticleIds] = useState<string[]>(() => {
    const saved = localStorage.getItem('savedArticles');
    return saved ? JSON.parse(saved) : [];
  });

  useEffect(() => {
    localStorage.setItem('savedArticles', JSON.stringify(savedArticleIds));
  }, [savedArticleIds]);

  const saveArticle = useCallback((articleId: string) => {
    setSavedArticleIds(prev => {
      if (prev.includes(articleId)) {
        return prev.filter(id => id !== articleId);
      }
      return [...prev, articleId];
    });
  }, []);

  const isArticleSaved = useCallback((articleId: string) => {
    return savedArticleIds.includes(articleId);
  }, [savedArticleIds]);

  return {
    savedArticleIds,
    saveArticle,
    isArticleSaved
  };
} 