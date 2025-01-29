import { useState, useEffect } from 'react'
import { NewsItem, Category, Topic, PagedResult } from '../types'
import { newsApi } from '../api/newsApi'
import { useLanguage } from '../../../contexts/LanguageContext'

export const useNews = () => {
  const [news, setNews] = useState<PagedResult<NewsItem>>()
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<Error | null>(null)
  const [page, setPage] = useState(1)
  const [selectedCategory, setSelectedCategory] = useState<Category | null>(null)
  const [selectedTopic, setSelectedTopic] = useState<Topic | null>(null)

  const loadNews = async () => {
    setIsLoading(true)
    setError(null)
    try {
      const data = await newsApi.getNews(
        page,
        10,
        selectedCategory?.id,
        selectedTopic?.id
      )
      setNews(data)
    } catch (err) {
      setError(err as Error)
    } finally {
      setIsLoading(false)
    }
  }

  const loadMore = async () => {
    if (!isLoading && news && page < news.totalPages) {
      setPage(prev => prev + 1)
    }
  }

  const refreshNews = async () => {
    setPage(1)
    await loadNews()
  }

  useEffect(() => {
    loadNews()
  }, [page, selectedCategory, selectedTopic])

  return {
    news,
    isLoading,
    error,
    page,
    totalPages: news?.totalPages || 0,
    selectedCategory,
    setSelectedCategory,
    selectedTopic,
    setSelectedTopic,
    loadMore,
    refreshNews
  }
} 