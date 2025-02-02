import React, { useState } from 'react';
import { Article } from '../../../types/Article';
import { useLanguage } from '../../../contexts/LanguageContext';
import { format } from 'date-fns';
import { FaRegBookmark, FaBookmark, FaShare, FaClock, FaNewspaper } from 'react-icons/fa';
import ArticlePopup from './ArticlePopup';
import { useSavedArticles } from '../../../hooks/useSavedArticles';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-hot-toast';

interface NewsCardProps {
  article: Article;
}

export const NewsCard: React.FC<NewsCardProps> = ({ article }) => {
  const { saveArticle, isArticleSaved } = useSavedArticles();
  const [isSaved, setIsSaved] = useState(() => isArticleSaved(article.id));
  const [isPopupOpen, setIsPopupOpen] = useState(false);
  const { language, t } = useLanguage();
  const navigate = useNavigate();

  const title = language === 'el' ? article.title.el : article.title.en;
  const summary = language === 'el' ? article.summary.el : article.summary.en;
  const featuredImage = article.featuredImages?.[0] || article.metadata?.thumbnail;

  const formatDate = (date: string) => {
    return format(new Date(date), 'PPP');
  };

  const handleSave = (e?: React.MouseEvent) => {
    e?.stopPropagation();
    saveArticle(article.id);
    setIsSaved(!isSaved);
  };

  const handleShare = async (e?: React.MouseEvent) => {
    e?.stopPropagation();
    
    // Create the share URL with article ID
    const shareUrl = `${window.location.origin}?article=${article.id}`;
    
    if (navigator.share) {
      try {
        await navigator.share({
          title: title,
          text: summary,
          url: shareUrl
        });
      } catch (error) {
        console.error('Error sharing:', error);
      }
    } else {
      // Fallback to clipboard copy
      try {
        await navigator.clipboard.writeText(shareUrl);
        // Show a toast notification
        toast.success(t('news.linkCopied'));
      } catch (error) {
        console.error('Error copying to clipboard:', error);
      }
    }
  };

  return (
    <>
      <article className="news-card" onClick={() => setIsPopupOpen(true)}>
        {featuredImage && (
          <div className="news-card-image-container">
            <img
              src={featuredImage}
              alt={title}
              className="news-card-image"
            />
            {article.category && (
              <div className="news-card-category">
                {language === 'el' ? article.category.name.el : article.category.name.en}
              </div>
            )}
          </div>
        )}
        
        <div className="news-card-content">
          <h3 className="news-card-title">{title}</h3>
          
          <div className="news-card-meta">
            <span className="news-card-date">
              <FaClock className="icon" />
              {formatDate(article.publishedAt)}             
            </span>
            <span className="news-card-sources">
            <FaNewspaper className="icon" />
              
              
              </span>
              <span className="news-card-sources">
                <span>

                
                {article.sourceCount}


                </span>
            </span>
          </div>

          <p className="news-card-summary">
            {summary.length > 150 ? `${summary.slice(0, 150)}...` : summary}
          </p>

          <div className="news-card-footer">
            <div className="news-card-actions">
              <button
                className={`action-button ${isSaved ? 'saved' : ''}`}
                onClick={handleSave}
                title={t(isSaved ? 'news.saved' : 'news.save')}
              >
                {isSaved ? <FaBookmark /> : <FaRegBookmark />}
              </button>
              <button
                className="action-button"
                onClick={handleShare}
                title={t('news.share')}
              >
                <FaShare />
              </button>
            </div>
          </div>
        </div>
      </article>

      {isPopupOpen && (
        <ArticlePopup
          article={article}
          onClose={() => setIsPopupOpen(false)}
          onSave={handleSave}
          onShare={handleShare}
          isSaved={isSaved}
        />
      )}
    </>
  );
};

export default NewsCard; 