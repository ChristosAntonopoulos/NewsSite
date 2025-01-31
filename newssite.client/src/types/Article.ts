export interface LocalizedContent {
  en: string;
  el: string;
}

export interface Category {
  id: string;
  name: LocalizedContent;
}

export interface Topic {
  id: string;
  name: LocalizedContent;
  categoryId: string;
}

export interface VerifiedFact {
  fact: LocalizedContent;
  confidence: number;
  sources: ArticleSource[];
}

export interface ArticleSource {
  url: string;
  name: string;
  type: string;
  title: LocalizedContent;
}

export interface Article {
  id: string;
  title: LocalizedContent;
  summary: LocalizedContent;
  content?: LocalizedContent;
  url: { [key: string]: string };
  publishedAt: string;
  category: Category;
  topic?: Topic;
  verifiedFacts: VerifiedFact[];
  featuredImages: string[];
  sources: ArticleSource[];
  sourceCount: number;
  metadata: {
    keywords: string;
    sourceName: string;
    sourceIcon: string;
    authors: string;
    thumbnail: string;
    generatedImages?: string;
  };
  analysis?: string;
  processedAt: string;
} 