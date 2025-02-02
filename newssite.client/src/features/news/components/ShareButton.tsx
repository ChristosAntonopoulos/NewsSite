import { Share } from 'lucide-react'
import { NewsItem } from '../types'
import { useLanguage } from '../../../contexts/LanguageContext'
import { toast } from 'react-hot-toast'

interface ShareButtonProps {
  item: NewsItem
}

export function ShareButton({ item }: ShareButtonProps) {
  const { t, language } = useLanguage()

  const handleShare = async () => {
    // Create the share URL with article ID
    const shareUrl = `${window.location.origin}?article=${item.id}`

    if (navigator.share) {
      try {
        await navigator.share({
          title: item.title[language],
          text: item.summary[language],
          url: shareUrl,
        })
      } catch (err) {
        console.error('Error sharing:', err)
      }
    } else {
      // Fallback to clipboard copy
      try {
        await navigator.clipboard.writeText(shareUrl)
        toast.success(t('news.linkCopied'))
      } catch (err) {
        console.error('Error copying to clipboard:', err)
        toast.error(t('common.error'))
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