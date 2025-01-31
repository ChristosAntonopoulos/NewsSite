import { LocalizedContent, Category, Topic, VerifiedFact } from '../../types/Article';

export interface HashtagData {
  twitter: string[]
  reddit: string[]
}

export interface NewsSource {
  title: string
  url: string
}

export interface NewsItem {
  id: string;
  title: LocalizedContent;
  summary: LocalizedContent;
  content?: LocalizedContent;
  url: { [key: string]: string };
  publishedAt: string;
  category: Category;
  topic?: Topic;
  verifiedFacts: VerifiedFact[];
  metadata: {
    keywords: string;
    sourceName: string;
    sourceIcon: string;
    authors: string;
    thumbnail: string;
  };
  analysis: string;
  processedAt: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

export interface KeyFact {
  fact: string
  confidence: number
  sources: string[]
} 