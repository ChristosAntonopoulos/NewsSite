import React from 'react';

interface TextFieldProps extends React.InputHTMLAttributes<HTMLInputElement | HTMLTextAreaElement> {
  label?: string;
  multiline?: boolean;
  rows?: number;
}

export function TextField({ label, multiline, rows = 3, className = '', ...props }: TextFieldProps) {
  const inputClasses = `block w-full px-3 py-2 bg-gray-50 dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:focus:ring-indigo-400 ${className}`;
  
  return (
    <div className="space-y-1">
      {label && (
        <label className="block text-sm font-medium text-gray-900 dark:text-gray-100">
          {label}
        </label>
      )}
      {multiline ? (
        <textarea
          rows={rows}
          className={inputClasses}
          {...(props as React.TextareaHTMLAttributes<HTMLTextAreaElement>)}
        />
      ) : (
        <input
          className={inputClasses}
          {...(props as React.InputHTMLAttributes<HTMLInputElement>)}
        />
      )}
    </div>
  );
} 