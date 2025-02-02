import React from 'react';
import { useLanguage } from '../../../contexts/LanguageContext';
import { FaFacebook, FaTwitter, FaPinterest, FaLinkedin } from 'react-icons/fa';
import { toast } from 'react-hot-toast';

interface SharePopupProps {
  title: string;
  url: string;
  onClose: () => void;
}

export function SharePopup({ title, url, onClose }: SharePopupProps) {
  const { t } = useLanguage();

  const shareOptions = [
    {
      name: 'Facebook',
      icon: <FaFacebook size={20} />,
      color: '#4267B2',
      onClick: () => window.open(`https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(url)}`, '_blank')
    },
    {
      name: 'Twitter',
      icon: <FaTwitter size={20} />,
      color: '#1DA1F2',
      onClick: () => window.open(`https://twitter.com/intent/tweet?text=${encodeURIComponent(title)}&url=${encodeURIComponent(url)}`, '_blank')
    },
    {
      name: 'LinkedIn',
      icon: <FaLinkedin size={20} />,
      color: '#0077B5',
      onClick: () => window.open(`https://www.linkedin.com/sharing/share-offsite/?url=${encodeURIComponent(url)}`, '_blank')
    },
    {
      name: 'Pinterest',
      icon: <FaPinterest size={20} />,
      color: '#E60023',
      onClick: () => window.open(`https://pinterest.com/pin/create/button/?url=${encodeURIComponent(url)}&description=${encodeURIComponent(title)}`, '_blank')
    }
  ];

  const handleCopyLink = async () => {
    try {
      await navigator.clipboard.writeText(url);
      toast.success(t('news.linkCopied'));
    } catch (err) {
      console.error('Error copying to clipboard:', err);
      toast.error(t('news.shareError'));
    }
  };

  return (
    <div 
      className="share-popup-overlay"
      onClick={onClose}
      style={{ backgroundColor: 'rgba(0, 0, 0, 0.5)' }}
    >
      <div 
        className="share-popup"
        onClick={e => e.stopPropagation()}
      >
        <h3 className="share-title">Share it</h3>
        
        <div className="share-buttons">
          {shareOptions.map(option => (
            <button
              key={option.name}
              onClick={option.onClick}
              className="share-button"
              style={{ backgroundColor: option.color }}
            >
              <span className="share-icon">{option.icon}</span>
              <span className="share-name">{option.name}</span>
            </button>
          ))}
          <button
            onClick={handleCopyLink}
            className="share-button copy-link"
            style={{ backgroundColor: '#6B7280' }}
          >
            <span className="share-name">Copy Link</span>
          </button>
        </div>
      </div>
    </div>
  );
} 