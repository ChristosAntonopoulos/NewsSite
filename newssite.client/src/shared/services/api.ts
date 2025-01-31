import axios from 'axios';

export class ApiClient {
    protected readonly baseUrl: string;
    private static readonly API_PREFIX = '/api';

    constructor(baseUrl: string) {
        this.baseUrl = baseUrl;
    }

    private getFullUrl(url: string): string {
        // Check if the base URL or the current URL already contains /api
        const hasApiPrefix = baseURL.includes('/api') || url.startsWith('/api');
        const finalUrl = hasApiPrefix ? url.replace('/api', '') : `${ApiClient.API_PREFIX}${url}`;
        console.log('API Call:', {
            baseURL,
            originalUrl: url,
            finalUrl,
            hasApiPrefix
        });
        return finalUrl;
    }

    protected async get<T>(url: string, config?: any): Promise<T> {
        const response = await api.get<T>(this.getFullUrl(url), config);
        return response.data;
    }

    protected async post<T>(url: string, data?: any, config?: any): Promise<T> {
        const response = await api.post<T>(this.getFullUrl(url), data, config);
        return response.data;
    }

    protected async put<T>(url: string, data?: any, config?: any): Promise<T> {
        const response = await api.put<T>(this.getFullUrl(url), data, config);
        return response.data;
    }

    protected async delete<T>(url: string, config?: any): Promise<T> {
        const response = await api.delete<T>(this.getFullUrl(url), config);
        return response.data;
    }
}

// Log environment variables to help debug
console.log('Environment:', {
    VITE_API_URL: import.meta.env.VITE_API_URL,
    NODE_ENV: import.meta.env.MODE
});

// Clean up any double /api occurrences in the base URL
const rawBaseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000';
const baseURL = rawBaseUrl.replace(/\/api\/api/g, '/api');

// Log the final base URL
console.log('Final Base URL:', baseURL);

export const api = axios.create({
    baseURL,
    headers: {
        'Content-Type': 'application/json',
    },
});

api.interceptors.request.use((config) => {
    const token = localStorage.getItem('token');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    // Clean up any double /api occurrences in the URL
    config.url = config.url?.replace(/\/api\/api/g, '/api');
    // Log the full URL being requested
    console.log('Full Request URL:', `${config.baseURL}${config.url}`);
    return config;
});

api.interceptors.response.use(
    (response) => response,
    async (error) => {
        if (error.response?.status === 401) {
            localStorage.removeItem('token');
            window.location.href = '/login';
        }
        return Promise.reject(error);
    }
); 