import axios, { AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios';
import { api } from './api';

export class ApiClient {
    protected readonly baseUrl: string;

    constructor(baseUrl: string) {
        this.baseUrl = baseUrl;
    }

    protected async get<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
        const response = await api.get<T>(url, config);
        return response.data;
    }

    protected async post<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
        const response = await api.post<T>(url, data, config);
        return response.data;
    }

    protected async put<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
        const response = await api.put<T>(url, data, config);
        return response.data;
    }

    protected async delete<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
        const response = await api.delete<T>(url, config);
        return response.data;
    }
} 