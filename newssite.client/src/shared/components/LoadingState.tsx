import React from 'react';

interface LoadingStateProps {
    message?: string;
}

export const LoadingState: React.FC<LoadingStateProps> = ({ message = 'Loading...' }) => {
    return (
        <div className="loading-container">
            <div className="loading-spinner"></div>
            <p>{message}</p>
        </div>
    );
}; 