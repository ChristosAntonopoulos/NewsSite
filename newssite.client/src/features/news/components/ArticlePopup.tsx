import React, { useState } from 'react';
import { Article, VerifiedFact } from '../../../types/Article';
import { useLanguage } from '../../../contexts/LanguageContext';
import { format } from 'date-fns';
import { el } from 'date-fns/locale';
import { FaTimes, FaCheckCircle, FaExclamationTriangle, FaTimesCircle, FaExternalLinkAlt, FaBookmark, FaShare, FaNewspaper } from 'react-icons/fa';
import { useTheme } from '@mui/material/styles';
import { getEditorStyles } from '../../../shared/theme/editorTheme';

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
        backgroundColor: 'rgba(0, 0, 0, 0.5)',
        position: 'fixed',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        zIndex: 1000,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center'
      }}
    >
      <div 
        className="sources-modal"
        onClick={e => e.stopPropagation()}
        style={{
          ...styles.paper,
          maxWidth: '600px',
          width: '90%',
          maxHeight: '80vh',
          overflow: 'auto',
          position: 'relative'
        }}
      >
        <button 
          className="close-button"
          onClick={() => setShowSources(false)}
          style={{
            position: 'absolute',
            right: '16px',
            top: '16px',
            background: 'none',
            border: 'none',
            cursor: 'pointer',
            color: 'inherit'
          }}
        >
          <FaTimes />
        </button>
        
        <h3 style={styles.title}>{t('news.relatedSources')}</h3>
        <p style={styles.subtitle}>{t('news.sourcesDescription')}</p>
        
        <div style={{ marginTop: '20px' }}>
          {article.sources.map((source, index) => (
            <div 
              key={index}
              style={{
                ...styles.section,
                marginBottom: '12px',
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center'
              }}
            >
              <div style={{ flex: 1 }}>
                <p style={{ ...styles.title, fontSize: '1rem', marginBottom: '4px' }}>
                  {source.title.en}
                </p>
              </div>
              <a
                href={source.url}
                target="_blank"
                rel="noopener noreferrer"
                style={{
                  ...styles.addButton,
                  textDecoration: 'none',
                  marginLeft: '16px',
                  display: 'flex',
                  alignItems: 'center',
                  gap: '8px'
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
                  <FaBookmark /> {t(isSaved ? 'news.saved' : 'news.save')}
                </button>
                <button className="popup-action-button" onClick={onShare}>
                  <FaShare /> {t('news.share')}
                </button>
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
                        <span>{Math.round(fact.confidence * 100)}%</span>
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