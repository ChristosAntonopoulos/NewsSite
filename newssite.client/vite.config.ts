import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'path';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [react()],
    define: {
        'process.env.NODE_ENV': JSON.stringify(process.env.NODE_ENV || 'production')
    },
    resolve: {
        alias: {
            '@': path.resolve(__dirname, './src')
        }
    },
    server: {
        proxy: {
            '/api': {
                target: process.env.VITE_API_URL || 'http://localhost:5000',
                changeOrigin: true
            },
            '/api-docs': {
                target: process.env.VITE_API_URL || 'http://localhost:5000',
                changeOrigin: true
            },
            '/images': {
                target: process.env.VITE_API_URL || 'http://localhost:5000',
                changeOrigin: true
            }
        },
        port: 5173
    }
});
