import { ApiClient } from '../../../shared/services/api';
import { Article, Category, Topic } from '../../../types/Article';
import { API_CONFIG } from '../../../config/api';

interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

export class NewsApi extends ApiClient {
  constructor() {
    super('');  // Using empty base URL since we'll use full paths from API_CONFIG
  }

  async getCategories(): Promise<Category[]> {
    return this.get<Category[]>(API_CONFIG.ENDPOINTS.CATEGORIES);
  }

  async getTopics(categoryId: string): Promise<Topic[]> {
    return this.get<Topic[]>(API_CONFIG.ENDPOINTS.TOPICS(categoryId));
  }

  async getNews(
    page: number = 1,
    pageSize: number = 10,
    categoryId?: string,
    topicId?: string
  ): Promise<PagedResult<Article>> {
    return this.get<PagedResult<Article>>(API_CONFIG.ENDPOINTS.ARTICLES, {
      params: {
        page,
        pageSize,
        categoryId,
        topicId,
      },
    });
  }

  async getSavedArticles(
    ids: string[],
    page: number = 1,
    pageSize: number = 10
  ): Promise<PagedResult<Article>> {
    return this.get<PagedResult<Article>>(`${API_CONFIG.ENDPOINTS.ARTICLES}/saved`, {
      params: {
        'ids[]': ids,
        page,
        pageSize,
      },
    });
  }

  async updateArticles(): Promise<void> {
    return this.post<void>(`${API_CONFIG.ENDPOINTS.ARTICLES}/update`);
  }

  async saveArticle(articleId: string): Promise<boolean> {
    return this.post<boolean>(`${API_CONFIG.ENDPOINTS.ARTICLES}/save`, { articleId });
  }

  async likeArticle(articleId: string): Promise<boolean> {
    return this.post<boolean>(`${API_CONFIG.ENDPOINTS.ARTICLES}/like`, { articleId });
  }
  async articleById(articleId: string): Promise<Article> {
    return  this.get<Article>(`${API_CONFIG.ENDPOINTS.ARTICLES}/${articleId}`, { articleId });
    
  } 
}

// Create a singleton instance
export const newsApi = new NewsApi(); 