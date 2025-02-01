export const API_CONFIG = {
    // If VITE_API_URL ends with /api, use it as is, otherwise append /api
    BASE_URL: (import.meta.env.VITE_API_URL || 'http://localhost:32769'),
    ENDPOINTS: {
        ARTICLES: '/articles',
        CATEGORIES: '/articles/categories',
        TOPICS: (categoryId: string) => `/categories/${categoryId}/topics`,
        ARTICLE_BY_ID: (id: string) => `/articles/${id}`,
        SAVE_ARTICLE: '/articles/save',
        LIKE_ARTICLE: '/articles/like',
        PIPELINES: '/pipeline',
        PIPELINE_BY_ID: (id: string) => `/pipeline/${id}`,
        PIPELINE_EXECUTE: (id: string) => `/pipeline/${id}/execute`,
        PIPELINE_TOGGLE: (id: string) => `/pipeline/${id}/toggle`
    }
} as const; 