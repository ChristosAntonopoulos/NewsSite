import { LocalizedContent, Category, Topic, NewsSource, VerifiedFact } from '../../../types';

export interface Article {
    id: string;
    title: LocalizedContent;
    content: LocalizedContent;
    summary: LocalizedContent;
    source: string;
    url: LocalizedContent;
    imageUrl?: string;
    publishedAt: string;
    category?: Category;
    topic?: Topic;
    sourceCount: number;
    wordCount: { [key: string]: number };
    verifiedFacts: VerifiedFact[];
    sources: NewsSource[];
    featuredImage?: string;
    version: number;
    metadata: Record<string, any>;
    createdAt: string;
    updatedAt: string;
}

export interface NewsState {
    items: Article[];
    loading: boolean;
    error: string | null;
}

export interface PagedResult<T> {
  items: T[];
  hasMore: boolean;
  total: number;
  page: number;
  pageSize: number;
} 