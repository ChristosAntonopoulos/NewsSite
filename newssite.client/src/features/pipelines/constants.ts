import { PipelineMode } from '../../types/Pipeline';

export const PIPELINE_MODES: Record<PipelineMode, string> = {
    Sequential: 'Sequential Execution',
    Parallel: 'Parallel Execution',
    Conditional: 'Conditional Execution'
} as const;

export const STEP_TYPES = {
    'serp.search': 'Search Engine Results',
    'openai.completion': 'AI Text Processing',
    'database.operation': 'Database Operations',
    'news.category': 'News Category Processing'
} as const;

export const STEP_TYPE_DESCRIPTIONS = {
    'serp.search': 'Fetch data from various search engines using SerpAPI',
    'openai.completion': 'Process text using OpenAI GPT models',
    'database.operation': 'Perform database operations like querying and storing data',
    'news.category': 'Process and categorize news articles with AI-powered analysis'
} as const;

export const COLOR_SCHEME = {
    primary: {
        light: 'text-cyan-600 dark:text-cyan-400',
        bg: 'bg-cyan-600 dark:bg-cyan-500',
        hover: 'hover:bg-cyan-700 dark:hover:bg-cyan-600',
        border: 'border-cyan-500 dark:border-cyan-400',
        ring: 'ring-cyan-500 dark:ring-cyan-400'
    },
    secondary: {
        light: 'text-gray-600 dark:text-gray-400',
        bg: 'bg-gray-100 dark:bg-gray-700',
        hover: 'hover:bg-gray-200 dark:hover:bg-gray-600',
        border: 'border-gray-300 dark:border-gray-600',
        ring: 'ring-gray-300 dark:ring-gray-600'
    },
    success: {
        light: 'text-green-600 dark:text-green-400',
        bg: 'bg-green-100 dark:bg-green-800/20',
        hover: 'hover:bg-green-200 dark:hover:bg-green-800/30',
        border: 'border-green-500 dark:border-green-400',
        ring: 'ring-green-500 dark:ring-green-400'
    },
    error: {
        light: 'text-red-600 dark:text-red-400',
        bg: 'bg-red-100 dark:bg-red-800/20',
        hover: 'hover:bg-red-200 dark:hover:bg-red-800/30',
        border: 'border-red-500 dark:border-red-400',
        ring: 'ring-red-500 dark:ring-red-400'
    }
} as const; 