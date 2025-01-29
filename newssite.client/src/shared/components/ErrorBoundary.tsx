import React from 'react';
import { useRouteError } from 'react-router-dom';

export const ErrorBoundary: React.FC = () => {
    const error = useRouteError();
    
    return (
        <div className="error-container">
            <h1>Oops! Something went wrong</h1>
            <p>{error instanceof Error ? error.message : 'An unexpected error occurred'}</p>
            <button onClick={() => window.location.href = '/'}>
                Return to Home
            </button>
        </div>
    );
}; 