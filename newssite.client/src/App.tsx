import { RouterProvider } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { router } from './routes/routes';
import './App.css';
import { LanguageProvider } from './contexts/LanguageContext';
import { ThemeProvider } from './contexts/ThemeContext';
import { AuthProvider } from './contexts/AuthContext';
import { UserPreferences } from './features/auth/components/UserPreferences';
import { SharedArticleProvider } from './contexts/SharedArticleContext';
import { Toaster } from 'react-hot-toast';

const queryClient = new QueryClient();

function App() {
    return (
        <QueryClientProvider client={queryClient}>
            <SharedArticleProvider>
                <Toaster position="bottom-right" />
                <LanguageProvider>
                    <ThemeProvider>
                        <AuthProvider>
                            <UserPreferences />
                            <div className="app">
                                <RouterProvider router={router} />
                            </div>
                        </AuthProvider>
                    </ThemeProvider>
                </LanguageProvider>
            </SharedArticleProvider>
        </QueryClientProvider>
    );
}

export default App;