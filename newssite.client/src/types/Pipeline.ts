export type PipelineMode = 'Sequential' | 'Parallel' | 'Conditional';

export type StepType = 
    | 'serp.search'
    | 'openai.completion'
    | 'database.operation'
    | 'news.category';

export interface RetryPolicy {
    maxRetries: number;
    initialDelay: string;
    exponentialBackoff: boolean;
    retryableExceptions?: string[];
}

export interface PipelineStep {
    id: string;
    name: string;
    type: StepType;
    enabled: boolean;
    parameters: Record<string, any>;
    transformations: TransformationConfig[];
    configuration: StepConfiguration;
    metadata?: Record<string, any>;
    retryPolicy?: RetryPolicy;
    order: number;
    passOutputAsInput?: boolean;
    condition?: string;
    ignoreErrors?: boolean;
    timeout?: { seconds: number };
    retryConditions?: Record<string, string>;
    dependsOn?: string[];
}

export interface StepConfiguration {
    [key: string]: any;
}

export interface SerpApiConfig extends StepConfiguration {
    searchQuery: string;
    engine: string;
    resultCount: number;
    apiKey: string;
    includeImages?: boolean;
    additionalParameters?: Record<string, string>;
}

export interface GptConfig extends StepConfiguration {
    model: string;
    systemPrompt: string;
    temperature: number;
    maxTokens: number;
}

export interface Pipeline {
    id: string;
    name: string;
    description: string;
    mode: PipelineMode;
    enabled: boolean;
    isEnabled: boolean;
    steps: PipelineStep[];
    schedule?: string;
    lastRun?: string;
    nextRun?: string;
    status: 'active' | 'inactive' | 'error';
    createdAt: string;
    updatedAt: string;
    metadata?: Record<string, any>;
    retryPolicy: RetryPolicy;
    inputSchema?: Record<string, any>;
    outputSchema?: Record<string, any>;
}

export interface PipelineExecutionContext {
    pipelineId: string;
    executionId: string;
    startTime: string;
    endTime?: string;
    status: 'running' | 'completed' | 'failed';
    currentStep?: string;
    error?: string;
    logs: PipelineExecutionLog[];
    results: Record<string, any>;
    stepOutputs: Record<string, any>;
    globalContext: Record<string, any>;
    stepStates: Record<string, StepExecutionState>;
    totalSteps: number;
    completedSteps: number;
    executionPath: string[];
    retryCount: Record<string, number>;
}

export interface StepExecutionState {
    stepId: string;
    status: 'pending' | 'running' | 'completed' | 'failed' | 'skipped' | 'cancelled';
    startTime?: string;
    endTime?: string;
    error?: string;
    output?: any;
}

export interface PipelineExecutionLog {
    timestamp: string;
    level: 'info' | 'warning' | 'error';
    message: string;
    stepId?: string;
}

export interface TransformationConfig {
    type: string;
    field: string;
    transformation: string;
    applyToList: boolean;
    outputField: string;
    parameters: Record<string, any>;
}

export const AVAILABLE_TRANSFORMATIONS = [
    { value: 'extract_keywords', label: 'Extract Keywords', description: 'Extract meaningful keywords from text' },
    { value: 'to_list', label: 'Convert to List', description: 'Convert input to a list (splits by comma if string)' },
    { value: 'to_lowercase', label: 'To Lowercase', description: 'Convert text to lowercase' },
    { value: 'to_uppercase', label: 'To Uppercase', description: 'Convert text to uppercase' },
    { value: 'trim', label: 'Trim', description: 'Remove leading and trailing whitespace' },
    { value: 'remove_punctuation', label: 'Remove Punctuation', description: 'Remove all punctuation marks' },
    { value: 'extract_numbers', label: 'Extract Numbers', description: 'Extract all numbers from text' },
    { value: 'extract_urls', label: 'Extract URLs', description: 'Extract all URLs from text' },
    { value: 'extract_emails', label: 'Extract Emails', description: 'Extract all email addresses from text' },
    { value: 'extract_hashtags', label: 'Extract Hashtags', description: 'Extract all hashtags from text' }
] as const; 