import React, { useState } from 'react';
import { Article, VerifiedFact } from '../../../types/Article';
import { useLanguage } from '../../../contexts/LanguageContext';
import { format } from 'date-fns';
import { el } from 'date-fns/locale';
import { FaTimes, FaCheckCircle, FaExclamationTriangle, FaTimesCircle, FaExternalLinkAlt, FaBookmark, FaShare, FaNewspaper, FaRegBookmark } from 'react-icons/fa';
import { useTheme } from '@mui/material/styles';
import { getEditorStyles } from '../../../shared/theme/editorTheme';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-hot-toast';
import { ShareButton } from './ShareButton';

interface ArticlePopupProps {
  article: Article;
  onClose: () => void;
  onSave: () => void;
  onShare: () => void;
  isSaved: boolean;
}

const ArticlePopup: React.FC<ArticlePopupProps> = ({
  article,
  onClose,
  onSave,
  onShare,
  isSaved,
}) => {
  const { language, t } = useLanguage();
  const [showSources, setShowSources] = useState(false);
  const theme = useTheme();
  const styles = getEditorStyles(theme);
  const navigate = useNavigate();

  const title = language === 'el' ? article.title.el : article.title.en;
  const summary = language === 'el' ? article.summary.el : article.summary.en;
  const content = language === 'el' ? article.content?.el : article.content?.en;
  

  const formatDate = (date: string) => {   
    return format(new Date(date), 'PPP');
  };

  const getCredibilityColor = (score: number) => {
    if (score >= 0.8) return 'var(--editor-success)';
    if (score >= 0.6) return 'var(--editor-warning)';
    return 'var(--editor-error)';
  };

  const getCredibilityIcon = (score: number) => {
    if (score >= 0.8) return <FaCheckCircle className="icon" />;
    if (score >= 0.6) return <FaExclamationTriangle className="icon" />;
    return <FaTimesCircle className="icon" />;
  };

  const SourcesModal = () => (
    <div 
      className="sources-modal-overlay"
      onClick={() => setShowSources(false)}
      style={{ 
        position: 'fixed',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        backgroundColor: 'rgba(0, 0, 0, 0.75)',
        zIndex: 9999,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center'
      }}
    >
      <div 
        onClick={e => e.stopPropagation()}
        style={{
          backgroundColor: theme.palette.mode === 'dark' ? '#1a202c' : '#ffffff',
          borderRadius: '8px',
          padding: '24px',
          width: '90%',
          maxWidth: '600px',
          maxHeight: '80vh',
          overflow: 'auto',
          position: 'relative',
          boxShadow: '0 4px 20px rgba(0, 0, 0, 0.2)',
          border: `1px solid ${theme.palette.mode === 'dark' ? '#2d3748' : '#e2e8f0'}`
        }}
      >
        <button 
          onClick={() => setShowSources(false)}
          style={{
            position: 'absolute',
            right: '16px',
            top: '16px',
            background: 'none',
            border: 'none',
            cursor: 'pointer',
            color: theme.palette.mode === 'dark' ? '#a0aec0' : '#718096',
            padding: '8px'
          }}
        >
          <FaTimes />
        </button>

        <h3 style={{
          fontSize: '1.5rem',
          fontWeight: 600,
          marginBottom: '16px',
          color: theme.palette.mode === 'dark' ? '#f7fafc' : '#2d3748'
        }}>
          {t('news.relatedSources')}
        </h3>

        <div style={{ marginTop: '20px' }}>
          {article.sources.map((source, index) => (
            <div 
              key={index}
              style={{
                padding: '16px',
                marginBottom: '12px',
                backgroundColor: theme.palette.mode === 'dark' ? '#2d3748' : '#f8f9fa',
                border: `1px solid ${theme.palette.mode === 'dark' ? '#4a5568' : '#e2e8f0'}`,
                borderRadius: '6px',
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center'
              }}
            >
              <div style={{ flex: 1 }}>
                <p style={{ 
                  fontSize: '1rem',
                  fontWeight: 500,
                  color: theme.palette.mode === 'dark' ? '#f7fafc' : '#2d3748'
                }}>
                  {source.name}
                </p>
              </div>
              <a
                href={source.url}
                target="_blank"
                rel="noopener noreferrer"
                style={{
                  padding: '8px 16px',
                  backgroundColor: theme.palette.mode === 'dark' ? '#4a5568' : '#edf2f7',
                  color: theme.palette.mode === 'dark' ? '#f7fafc' : '#2d3748',
                  borderRadius: '4px',
                  textDecoration: 'none',
                  fontSize: '0.875rem',
                  marginLeft: '16px',
                  transition: 'all 0.2s ease',
                  border: 'none',
                  cursor: 'pointer'
                }}
              >
                {t('news.visitSource')}
              </a>
            </div>
          ))}
        </div>
      </div>
    </div>
  );

  const handleShare = async (e?: React.MouseEvent) => {
    e?.stopPropagation();
    
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
      try {
        await navigator.clipboard.writeText(shareUrl);
        toast.success(t('news.linkCopied'));
      } catch (error) {
        console.error('Error copying to clipboard:', error);
      }
    }
  };

  return (
    <div className="article-popup-overlay" onClick={onClose}>
      <div className="article-popup" onClick={e => e.stopPropagation()}>
        <button className="close-button" onClick={onClose}>
          <FaTimes />
        </button>

        <div className="article-popup-content">
          {article.featuredImages?.[0] && (
            <div className="article-popup-image">
              <img src={article.featuredImages[0]} alt={title} />
              {article.category && (
                <div className="article-popup-category">
                  {language === 'el' ? article.category.name.el : article.category.name.en}
                </div>
              )}
            </div>
          )}

          <div className="article-popup-header">
            <h2 className="article-popup-title">{title}</h2>
            <div className="article-popup-meta">
              <span className="article-popup-date">
                {formatDate(article.publishedAt)}
              </span>
              <div className="article-popup-actions">
                <button 
                  className={`popup-action-button ${isSaved ? 'saved' : ''}`}
                  onClick={onSave}
                >
                  {isSaved ? <FaBookmark /> : <FaRegBookmark />} {t('news.saveArticle')}
                </button>
                
                <ShareButton 
                  item={article} 
                  className="popup-action-button"
                />
                
                <button 
                  className="popup-action-button sources-button"
                  onClick={() => setShowSources(true)}
                >
                  <FaNewspaper /> {article.sourceCount} {t('news.sources')}
                </button>
              </div>
            </div>
          </div>

          <div className="article-popup-body">
            <div className="article-popup-summary">
              <p>{summary}</p>
            </div>

            {article.verifiedFacts && article.verifiedFacts.length > 0 && (
              <div className="article-popup-facts">
                <h3>{t('news.keyFacts')}</h3>
                <div className="facts-list">
                  {article.verifiedFacts.map((fact, index) => (
                    <div key={index} className="fact-item">
                      <div className="fact-content">
                        <p>{language === 'el' ? fact.fact.el : fact.fact.en}</p>
                      </div>
                      <div 
                        className="fact-credibility"
                        style={{ color: getCredibilityColor(fact.confidence) }}
                      >
                        {getCredibilityIcon(fact.confidence)}
                        <span>{Math.round(fact.confidence * 10)}%</span>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            )}
           
          </div>
        </div>
        
        {showSources && <SourcesModal />}
      </div>
    </div>
  );
};

export default ArticlePopup; 