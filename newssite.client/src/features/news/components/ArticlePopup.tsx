import React, { useState } from 'react';
import { Article, VerifiedFact, ArticleSource } from '../../../types/Article';
import { useLanguage } from '../../../contexts/LanguageContext';
import { useTheme } from '@mui/material/styles';
import { format } from 'date-fns';
import { el } from 'date-fns/locale';
import { FaTimes, FaCheckCircle, FaExclamationTriangle, FaTimesCircle, FaExternalLinkAlt, FaBookmark, FaShare, FaNewspaper } from 'react-icons/fa';
import { editorColors } from '../../../shared/theme/editorTheme';

// Helper function to generate unique IDs
const generateId = () => {
  return Math.random().toString(36).substring(2) + Date.now().toString(36);
};

const getStyles = (mode: 'light' | 'dark') => {
  const colors = editorColors[mode];
  
  return {
    sourcesPopupOverlay: {
      position: 'fixed',
      top: 0,
      left: 0,
      right: 0,
      bottom: 0,
      backgroundColor: 'rgba(0, 0, 0, 0.75)',
      backdropFilter: 'blur(4px)',
      display: 'flex',
      justifyContent: 'center',
      alignItems: 'center',
      zIndex: 1000,
    } as React.CSSProperties,

    sourcesPopup: {
      backgroundColor: colors.surface,
      borderRadius: '12px',
      padding: '20px',
      maxWidth: '400px',
      width: '90%',
      maxHeight: '60vh',
      position: 'relative',
      boxShadow: '0 8px 32px rgba(0, 0, 0, 0.3)',
      border: `1px solid ${colors.border}`,
    } as React.CSSProperties,

    sourcesHeader: {
      marginBottom: '16px',
      paddingRight: '24px',
      borderBottom: `2px solid ${colors.border}`,
      paddingBottom: '12px',
    } as React.CSSProperties,

    sourcesHeaderTitle: {
      margin: 0,
      fontSize: '18px',
      fontWeight: 600,
      color: colors.text.primary,
    } as React.CSSProperties,

    sourcesList: {
      marginTop: '12px',
      maxHeight: 'calc(60vh - 100px)',
      overflowY: 'auto',
      scrollbarWidth: 'thin',
      scrollbarColor: `${colors.border} transparent`,
      padding: '4px',
    } as React.CSSProperties,

    sourceItem: {
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center',
      padding: '12px 16px',
      borderRadius: '8px',
      marginBottom: '8px',
      backgroundColor: colors.paper,
      transition: 'all 0.2s ease',
      border: `1px solid transparent`,
      '&:hover': {
        backgroundColor: colors.input.background,
        borderColor: colors.input.hoverBorder,
        transform: 'translateY(-1px)',
        boxShadow: '0 4px 8px rgba(0, 0, 0, 0.1)',
      },
    } as React.CSSProperties,

    sourceInfo: {
      display: 'flex',
      alignItems: 'center',
      gap: '12px',
      flex: 1,
      minWidth: 0,
    } as React.CSSProperties,

    sourceIcon: {
      color: colors.primary.main,
      fontSize: '18px',
      flexShrink: 0,
    } as React.CSSProperties,

    sourceDetails: {
      display: 'flex',
      flexDirection: 'column',
      gap: '4px',
      minWidth: 0,
    } as React.CSSProperties,

    sourceName: {
      fontSize: '15px',
      fontWeight: '600',
      color: colors.text.primary,
      whiteSpace: 'nowrap',
      overflow: 'hidden',
      textOverflow: 'ellipsis',
    } as React.CSSProperties,

    sourceType: {
      fontSize: '13px',
      color: colors.text.secondary,
      textTransform: 'capitalize',
    } as React.CSSProperties,

    sourceLink: {
      color: colors.primary.main,
      fontSize: '16px',
      padding: '8px',
      borderRadius: '6px',
      flexShrink: 0,
      transition: 'all 0.2s ease',
      '&:hover': {
        backgroundColor: `${colors.primary.main}1A`,
        transform: 'scale(1.1)',
      },
    } as React.CSSProperties,

    closeButton: {
      position: 'absolute',
      top: '12px',
      right: '12px',
      background: 'none',
      border: 'none',
      color: colors.text.secondary,
      cursor: 'pointer',
      padding: '8px',
      borderRadius: '6px',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      transition: 'all 0.2s ease',
      '&:hover': {
        backgroundColor: `${colors.primary.main}1A`,
        color: colors.text.primary,
      },
    } as React.CSSProperties,
  };
};

interface SourcesPopupProps {
  sources: ArticleSource[];
  onClose: () => void;
}

const SourcesPopup: React.FC<SourcesPopupProps> = ({ sources, onClose }) => {
  const { language, t } = useLanguage();
  const theme = useTheme();
  const styles = getStyles(theme.palette.mode);

  return (
    <div style={styles.sourcesPopupOverlay} onClick={onClose}>
      <div style={styles.sourcesPopup} onClick={e => e.stopPropagation()}>
        <button style={styles.closeButton} onClick={onClose}>
          <FaTimes />
        </button>
        <div style={styles.sourcesHeader}>
          <h3 style={styles.sourcesHeaderTitle}>{t('news.sources')} ({sources.length})</h3>
        </div>
        <div style={styles.sourcesList}>
          {sources.map((source) => (
            <div key={generateId()} style={styles.sourceItem}>
              <div style={styles.sourceInfo}>
                <FaNewspaper style={styles.sourceIcon} />
                <div style={styles.sourceDetails}>
                  <span style={styles.sourceName}>{source.name}</span>
                  <span style={styles.sourceType}>{source.type}</span>
                </div>
              </div>
              <a
                href={source.url}
                target="_blank"
                rel="noopener noreferrer"
                style={styles.sourceLink}
              >
                <FaExternalLinkAlt />
              </a>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};

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
                  className="popup-action-button"
                  onClick={() => setShowSources(true)}
                >
                  <FaNewspaper /> {t('news.sources')} ({article.sources?.length || 0})
                </button>
                <a
                  href={article.url[language] || article.url.en}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="popup-action-button"
                >
                  <FaExternalLinkAlt /> {t('news.readMore')}
                </a>
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
                        <span>{Math.round(fact.confidence )}%</span>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            )}
           
          </div>
        </div>
      </div>

      {showSources && article.sources && (
        <SourcesPopup
          sources={article.sources}
          onClose={() => setShowSources(false)}
        />
      )}
    </div>
  );
};

export default ArticlePopup; 