import { createBrowserRouter } from 'react-router-dom';
import { Layout } from './shared/components/Layout/Layout';
import NewsFeed from './features/news/components/NewsFeed';
import { ErrorBoundary } from './shared/components/ErrorBoundary';

export const router = createBrowserRouter([
  {
    path: '/',
    element: <Layout><NewsFeed /></Layout>,
    errorElement: <ErrorBoundary />,
    children: [
      {
        path: '/',
        element: <NewsFeed />
      },
      {
        path: 'saved',
        element: <div>Saved Articles</div>
      },
      {
        path: 'preferences',
        element: <div>Preferences</div>
      },
      {
        path: 'pipelines',
        element: <div>Pipelines</div>
      }
    ]
  }
]); 