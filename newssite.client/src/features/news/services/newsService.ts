import { ApiClient } from '../../../shared/services/api';
import { Article } from '../types/news.types';

class NewsService extends ApiClient {
    constructor() {
        super('/api/articles');
    }

    async getArticles(): Promise<Article[]> {
        return this.get<Article[]>('');
    }

    async getArticle(id: string): Promise<Article> {
        return this.get<Article>(`/${id}`);
    }

    async createArticle(article: Article): Promise<Article> {
        return this.post<Article>('', article);
    }

    async updateArticle(id: string, article: Article): Promise<Article> {
        return this.put<Article>(`/${id}`, article);
    }

    async deleteArticle(id: string): Promise<void> {
        return this.delete<void>(`/${id}`);
    }
}

export const newsService = new NewsService(); 