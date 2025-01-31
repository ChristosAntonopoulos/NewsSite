export interface LocalizedContent {
    en: string;
    el: string;
}

export interface Category {
    id: string;
    name: LocalizedContent;
    slug: string;
    description: LocalizedContent;
    parentId?: string;
}

export interface Topic {
    id: string;
    name: LocalizedContent;
    slug: string;
    description: LocalizedContent;
    categoryId?: string;
    trendScore: number;
}

export interface NewsSource {
    id: string;
    name: string;
    url: string;
    reliability: number;
    bias: number;
}

export interface VerifiedFact {
    claim: LocalizedContent;
    verdict: 'true' | 'false' | 'partially_true' | 'unverified';
    explanation: LocalizedContent;
    sources: string[];
    confidence: number;
}

export interface PagedResult<T> {
    items: T[];
    totalCount: number;
    pageSize: number;
    currentPage: number;
    totalPages: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
} 