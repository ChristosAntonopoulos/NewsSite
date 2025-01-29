import { Share } from 'lucide-react'
import { NewsItem } from '../types'
import { useLanguage } from '../../../contexts/LanguageContext'

interface ShareButtonProps {
  item: NewsItem
}

export function ShareButton({ item }: ShareButtonProps) {
  const { t, language } = useLanguage()

  const handleShare = async () => {
    if (navigator.share) {
      try {
        await navigator.share({
          title: item.title[language],
          text: item.summary[language],
          url: window.location.href,
        })
      } catch (err) {
        console.error('Error sharing:', err)
      }
    }
  }

  return (
    <button
      onClick={handleShare}
      className="text-gray-500 hover:text-teal-500 transition-colors"
      title={t('news.shareArticle')}
    >
      <Share className="h-5 w-5" />
    </button>
  )
} 