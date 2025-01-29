import { Share } from 'lucide-react'
import { NewsItem } from '../../features/news/types'
import { useLanguage } from '../../contexts/LanguageContext'

interface ShareButtonProps {
  item: NewsItem
}

export function ShareButton({ item }: ShareButtonProps) {
  const { language } = useLanguage()

  return (
    <button 
      className="text-gray-500 hover:text-blue-500 share-button"
      onClick={(e) => {
        e.stopPropagation()
        navigator.share?.({
          title: item.title[language] || item.title.en,
          text: item.summary[language] || item.summary.en,
          url: window.location.href
        })
      }}
    >
      <Share className="h-5 w-5" />
    </button>
  )
} 