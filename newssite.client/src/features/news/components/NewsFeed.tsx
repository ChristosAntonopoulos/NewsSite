import React, { useEffect, useState } from 'react';
import { Article, Category } from '../../../types/Article';
import NewsCard from './NewsCard';
import CategoryBar from './CategoryBar';
import { newsApi } from '../api/newsApi';
import { useTheme as useAppTheme } from '../../../contexts/ThemeContext';
import { editorColors } from '../../../shared/theme/editorTheme';
import { useLanguage } from '../../../contexts/LanguageContext';
import { useSavedArticles } from '../../../hooks/useSavedArticles';
import { useLocation, useNavigate } from 'react-router-dom';
import { useSharedArticle } from '../../../contexts/SharedArticleContext';
import ArticlePopup from './ArticlePopup';
import './NewsFeed.css';
import { toast } from 'react-hot-toast';

interface NewsFeedProps {
  mode?: 'all' | 'saved';
}

interface NewsData {
  items: Article[];
  totalItems: number;
  page: number;
  pageSize: number;
}

const NewsFeed: React.FC<NewsFeedProps> = ({ mode = 'all' }) => {
  const [articles, setArticles] = useState<Article[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [selectedCategory, setSelectedCategory] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [hasMore, setHasMore] = useState(true);
  const { theme } = useAppTheme();
  const { t } = useLanguage();
  const { savedArticleIds } = useSavedArticles();
  const colors = editorColors[theme];
  const { sharedArticleId, setSharedArticleId } = useSharedArticle();
  const [selectedArticle, setSelectedArticle] = useState<Article | null>(null);
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    if (mode === 'all') {
      loadCategories();
    }
  }, [mode]);

  useEffect(() => {
    setPage(1);
    setArticles([]);
    loadArticles(1);
  }, [selectedCategory, mode, savedArticleIds]);

  useEffect(() => {
    const handleSharedArticle = async () => {
      const params = new URLSearchParams(location.search);
      const articleId = params.get('article');
      
      if (articleId) {
        try {
          const article = await newsApi.articleById(articleId);
          if (article) {
            setSelectedArticle(article);
          } else {
            toast.error(t('news.articleNotFound'));
          }
        } catch (error) {
          console.error('Error fetching shared article:', error);
          toast.error(t('news.articleNotFound'));
        }
      }
    };

    handleSharedArticle();
  }, [location.search]);

  const loadCategories = async () => {
    try {
      const data = await newsApi.getCategories();
      setCategories(data);
    } catch (err) {
      setError(t('news.errorLoadingCategories'));
      console.error('Error fetching categories:', err);
    }
  };

  const loadArticles = async (pageNum: number) => {
    if (pageNum === 1) {
      setLoading(true);
    }
    try {
      let data: NewsData;
      if (mode === 'saved') {
        if (savedArticleIds.length === 0) {
          data = { items: [], totalItems: 0, page: 1, pageSize: 12 };
        } else {
          data = await newsApi.getSavedArticles(savedArticleIds, pageNum, 12);
        }
      } else {
        data = await newsApi.getNews(pageNum, 12, selectedCategory || undefined);
      }

      if (pageNum === 1) {
        setArticles(data.items);
      } else {
        setArticles(prev => [...prev, ...data.items]);
      }
      setHasMore(data.items.length === 12);
      setError(null);
    } catch (err) {
      setError(t('news.errorLoadingArticles'));
      console.error('Error fetching articles:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleLoadMore = () => {
    const nextPage = page + 1;
    setPage(nextPage);
    loadArticles(nextPage);
  };

  const handleCategorySelect = (categoryId: string) => {
    setSelectedCategory(categoryId || null);
  };

  if (error) {
    return (
      <div className="error-message">
        {error}
      </div>
    );
  }

  return (
    <div className="news-feed">
      {mode === 'all' && (
        <CategoryBar
          categories={categories}
          selectedCategory={selectedCategory}
          onCategorySelect={handleCategorySelect}
        />
      )}

      {loading && articles.length === 0 ? (
        <div className="loading">
          {t('common.loading')}
        </div>
      ) : (
        <>
          {mode === 'saved' && (
            <h1 className="text-2xl font-bold mb-6">{t('savedArticles.title')}</h1>
          )}
          
          <div className="news-grid">
            {articles.map(article => (
              <NewsCard
                key={article.id}
                article={article}
              />
            ))}
          </div>
          
          {articles.length === 0 ? (
            <div className="no-articles">
              {mode === 'saved' ? t('savedArticles.noArticles') : t('news.noArticlesFound')}
            </div>
          ) : hasMore && (
            <div className="load-more-container">
              <button
                onClick={handleLoadMore}
                disabled={loading}
                className="load-more-button"
              >
                {loading ? t('common.loading') : t('news.loadMore')}
              </button>
            </div>
          )}
        </>
      )}

      {selectedArticle && (
        <ArticlePopup
          article={selectedArticle}
          onClose={() => {
            setSelectedArticle(null);
            // Clear the URL parameter without navigation
            window.history.replaceState({}, '', window.location.pathname);
          }}
          onSave={() => {/* your save handler */}}
          onShare={() => {/* your share handler */}}
          isSaved={false} // Use your actual saved state here
        />
      )}
    </div>
  );
};

export default NewsFeed;

// Move ShareButton and PollSection to separate components for better organization