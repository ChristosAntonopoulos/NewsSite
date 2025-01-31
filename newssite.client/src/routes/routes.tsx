import React, { Suspense } from 'react';
import { createBrowserRouter, Outlet } from 'react-router-dom';
import { Layout } from '../shared/components/Layout/Layout';
import { ErrorBoundary } from '../shared/components/ErrorBoundary';
import { LoginPage } from '../pages/LoginPage';
import { RegisterPage } from '../pages/RegisterPage';

// Lazy load pages
const NewsFeed = React.lazy(() => import('../features/news/components/NewsFeed'));
const PipelineList = React.lazy(() => import('../features/pipelines/pages/PipelineList'));
const PipelineEditor = React.lazy(() => import('../features/pipelines/pages/PipelineEditor'));
const CryptoPrices = React.lazy(() => import('../features/crypto/pages/CryptoPrices'));

const LoadingFallback = () => (
    <div className="flex items-center justify-center min-h-screen">
        <div className="loader">Loading...</div>
    </div>
);

export const router = createBrowserRouter([
    {
        path: '/login',
        element: <LoginPage />,
        errorElement: <ErrorBoundary />
    },
    {
        path: '/register',
        element: <RegisterPage />,
        errorElement: <ErrorBoundary />
    },
    {
        path: '/',
        element: <Layout>
            <Outlet />
        </Layout>,
        errorElement: <ErrorBoundary />,
        children: [
            {
                index: true,
                element: (
                    <Suspense fallback={<LoadingFallback />}>
                        <NewsFeed />
                    </Suspense>
                ),
            },
            {
                path: 'pipelines',
                children: [
                    {
                        index: true,
                        element: (
                            <Suspense fallback={<LoadingFallback />}>
                                <PipelineList />
                            </Suspense>
                        ),
                    },
                    {
                        path: 'create',
                        element: (
                            <Suspense fallback={<LoadingFallback />}>
                                <PipelineEditor />
                            </Suspense>
                        ),
                    },
                    {
                        path: 'edit/:id',
                        element: (
                            <Suspense fallback={<LoadingFallback />}>
                                <PipelineEditor />
                            </Suspense>
                        ),
                    }
                ]
            },
            {
                path: 'crypto',
                element: (
                    <Suspense fallback={<LoadingFallback />}>
                        <CryptoPrices />
                    </Suspense>
                ),
            },
            {
                path: 'saved',
                element: (
                    <Suspense fallback={<LoadingFallback />}>
                        <NewsFeed mode="saved" />
                    </Suspense>
                ),
            },
        ]
    }
]); 