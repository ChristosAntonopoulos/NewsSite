import { NewsItem } from './types';
import { API_CONFIG } from '../../config/api';

interface Category {
  id: string;
  name: {
    en: string;
    el: string;
  };
  description: {
    en: string;
    el: string;
  };
  slug: {
    en: string;
    el: string;
  };
}

interface Topic {
  id: string;
  name: {
    en: string;
    el: string;
  };
  description: {
    en: string;
    el: string;
  };
  slug: {
    en: string;
    el: string;
  };
  categoryId: string;
}

interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

const createUrl = (endpoint: string) => `${API_CONFIG.BASE_URL}${endpoint}`;

export const fetchCategories = async (language: string = 'en'): Promise<Category[]> => {
  const response = await fetch(createUrl(`${API_CONFIG.ENDPOINTS.CATEGORIES}?language=${language}`));
  if (!response.ok) {
    throw new Error('Failed to fetch categories');
  }
  return response.json();
};

export const fetchTopics = async (categoryId: string, language: string = 'en'): Promise<Topic[]> => {
  const response = await fetch(createUrl(`${API_CONFIG.ENDPOINTS.TOPICS(categoryId)}?language=${language}`));
  if (!response.ok) {
    throw new Error('Failed to fetch topics');
  }
  return response.json();
};

export const fetchNews = async (
  page: number = 1,
  pageSize: number = 10,
  categoryId?: string,
  topicId?: string,
  language: string = 'en'
): Promise<PagedResult<NewsItem>> => {
  const params = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString(),
    language,
    ...(categoryId && { categoryId }),
    ...(topicId && { topicId })
  });

  const response = await fetch(createUrl(`${API_CONFIG.ENDPOINTS.ARTICLES}?${params}`));
  if (!response.ok) {
    throw new Error('Failed to fetch news');
  }
  return response.json();
};

export const saveArticle = async (articleId: string): Promise<boolean> => {
  const response = await fetch(createUrl(API_CONFIG.ENDPOINTS.SAVE_ARTICLE), {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ articleId })
  });
  if (!response.ok) {
    throw new Error('Failed to save article');
  }
  return response.json();
};

export const likeArticle = async (articleId: string): Promise<boolean> => {
  const response = await fetch(createUrl(API_CONFIG.ENDPOINTS.LIKE_ARTICLE), {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ articleId })
  });
  if (!response.ok) {
    throw new Error('Failed to like article');
  }
  return response.json();
}; 