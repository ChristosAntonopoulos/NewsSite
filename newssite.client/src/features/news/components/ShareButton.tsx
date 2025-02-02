import { FaShare } from 'react-icons/fa'
import { useLanguage } from '../../../contexts/LanguageContext'
import { useState } from 'react'
import { SharePopup } from './SharePopup'

interface ShareButtonProps {
  item: {
    id: string;
    title: {
      en: string;
      el: string;
    };
  };
  className?: string;
}

export function ShareButton({ item, className = '' }: ShareButtonProps) {
  const { t, language } = useLanguage()
  const [showSharePopup, setShowSharePopup] = useState(false)

  const handleClick = (e: React.MouseEvent) => {
    e.preventDefault()
    e.stopPropagation()
    setShowSharePopup(true)
  }

  const handleClose = (e?: React.MouseEvent) => {
    if (e) {
      e.preventDefault()
      e.stopPropagation()
    }
    setShowSharePopup(false)
  }

  return (
    <div className="share-button-container">
      <button
        className={`action-button ${className}`}
        onClick={handleClick}
        title={t('news.share')}
      >
        <FaShare />
      </button>

      {showSharePopup && (
        <SharePopup
          title={item.title[language]}
          url={`${window.location.origin}/article/${item.id}`}
          onClose={handleClose}
          isInsideCard={true}
        />
      )}
    </div>
  )
} 