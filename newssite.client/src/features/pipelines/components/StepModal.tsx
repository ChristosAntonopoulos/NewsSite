import { useState } from 'react';
import { Dialog } from '@headlessui/react';
import { PipelineStep, StepType } from '../../../types/Pipeline';
import { Button } from '../../../shared/components/common/Button';
import { InformationCircleIcon } from '@heroicons/react/24/outline';

interface StepModalProps {
  step: PipelineStep;
  onSave: (step: PipelineStep) => void;
  onCancel: () => void;
}

const defaultConfigs: Record<StepType, any> = {
  'serp.search': {
    searchQuery: '',
    engine: 'google_news',
    resultCount: '10',
    type: 'serp.search' as const
  },
  'openai.completion': {
    model: 'gpt-4',
    systemPrompt: '',
    temperature: '0.7',
    maxTokens: '1000',
    type: 'openai.completion' as const
  },
  'database.operation': {
    operation: 'query',
    collection: '',
    type: 'database.operation' as const
  },
  'news.category': {
    category: '',
    language: 'en',
    type: 'news.category' as const
  }
} as const;

const stepTypeDescriptions: Record<StepType, string> = {
  'serp.search': 'Search Engine Results Page API - Fetch news and search results from various search engines.',
  'openai.completion': 'GPT Language Model - Process and generate text using advanced AI models.',
  'database.operation': 'Database operations for storing and retrieving data.',
  'news.category': 'News category processing and organization.'
};

interface TooltipProps {
  text: string;
}

function Tooltip({ text }: TooltipProps) {
  return (
    <div className="group relative inline-block ml-2">
      <InformationCircleIcon className="h-4 w-4 text-gray-400 hover:text-gray-500" />
      <div className="invisible group-hover:visible absolute z-50 w-64 p-2 mt-1 text-sm text-white bg-gray-900 rounded-md shadow-lg -left-32 top-full">
        {text}
        <div className="absolute w-2 h-2 bg-gray-900 transform rotate-45 -top-1 left-1/2 -translate-x-1/2"></div>
      </div>
    </div>
  );
}

export function StepModal({ step, onSave, onCancel }: StepModalProps) {
  const [editedStep, setEditedStep] = useState<PipelineStep>(() => ({
    ...step,
    parameters: {
      ...defaultConfigs[step.type as StepType || 'serp.search'],
      ...step.parameters
    }
  }));
  const [errors, setErrors] = useState<Record<string, string>>({});

  const validateStep = () => {
    const newErrors: Record<string, string> = {};

    if (!editedStep.name.trim()) {
      newErrors.name = 'Name is required';
    }

    switch (editedStep.type) {
      case 'serp.search':
        if (!editedStep.parameters.searchQuery?.trim()) {
          newErrors.searchQuery = 'Search query is required';
        }
        if (!editedStep.parameters.engine?.trim()) {
          newErrors.engine = 'Engine is required';
        }
        if (!editedStep.parameters.resultCount || Number(editedStep.parameters.resultCount) < 1) {
          newErrors.resultCount = 'Result count must be at least 1';
        }
        break;

      case 'openai.completion':
        if (!editedStep.parameters.model?.trim()) {
          newErrors.model = 'Model is required';
        }
        if (!editedStep.parameters.systemPrompt?.trim()) {
          newErrors.systemPrompt = 'System prompt is required';
        }
        if (!editedStep.parameters.temperature || Number(editedStep.parameters.temperature) < 0 || Number(editedStep.parameters.temperature) > 1) {
          newErrors.temperature = 'Temperature must be between 0 and 1';
        }
        if (!editedStep.parameters.maxTokens || Number(editedStep.parameters.maxTokens) < 1) {
          newErrors.maxTokens = 'Max tokens must be at least 1';
        }
        break;

      case 'database.operation':
        if (!editedStep.parameters.operation?.trim()) {
          newErrors.operation = 'Operation is required';
        }
        if (!editedStep.parameters.collection?.trim()) {
          newErrors.collection = 'Collection is required';
        }
        break;

      case 'news.category':
        if (!editedStep.parameters.category?.trim()) {
          newErrors.category = 'Category is required';
        }
        break;
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSave = () => {
    if (validateStep()) {
      onSave(editedStep);
    }
  };

  const handleConfigChange = (key: string, value: any) => {
    setEditedStep({
      ...editedStep,
      parameters: {
        ...editedStep.parameters,
        [key]: value,
      },
    });
    if (errors[key]) {
      setErrors({ ...errors, [key]: '' });
    }
  };

  const handleTypeChange = (type: StepType) => {
    setEditedStep({
      ...editedStep,
      type,
      parameters: defaultConfigs[type]
    });
    setErrors({});
  };

  const renderConfigFields = () => {
    switch (editedStep.type) {
      case 'serp.search':
        return (
          <div className="space-y-6 bg-gray-50 dark:bg-gray-900 p-4 rounded-lg">
            <div className="flex items-center">
              <h3 className="text-base font-medium text-gray-900 dark:text-white">SerpAPI Configuration</h3>
              <Tooltip text="Configure how the step fetches and processes search results from various search engines." />
            </div>
            
            <div>
              <div className="flex items-center">
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  Search Query
                  <span className="text-red-500 ml-1">*</span>
                </label>
                <Tooltip text="The search terms to use when fetching results. Be specific for better results." />
              </div>
              <input
                type="text"
                value={editedStep.parameters.searchQuery || ''}
                onChange={(e) => handleConfigChange('searchQuery', e.target.value)}
                className={`mt-1 block w-full rounded-md shadow-sm focus:border-cyan-500 focus:ring-cyan-500 dark:bg-gray-700 dark:border-gray-600 ${
                  errors.searchQuery ? 'border-red-300' : 'border-gray-300'
                }`}
                placeholder="e.g., latest technology news in artificial intelligence"
              />
              {errors.searchQuery && (
                <p className="mt-1 text-sm text-red-600">{errors.searchQuery}</p>
              )}
            </div>

            <div>
              <div className="flex items-center">
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  Engine
                  <span className="text-red-500 ml-1">*</span>
                </label>
                <Tooltip text="Select the search engine to use. Each engine may provide different types of results." />
              </div>
              <select
                value={editedStep.parameters.engine || 'google_news'}
                onChange={(e) => handleConfigChange('engine', e.target.value)}
                className={`mt-1 block w-full rounded-md shadow-sm focus:border-cyan-500 focus:ring-cyan-500 dark:bg-gray-700 dark:border-gray-600 ${
                  errors.engine ? 'border-red-300' : 'border-gray-300'
                }`}
              >
                <option value="google_news">Google News</option>
                <option value="bing_news">Bing News</option>
                <option value="google">Google Search</option>
              </select>
              {errors.engine && (
                <p className="mt-1 text-sm text-red-600">{errors.engine}</p>
              )}
            </div>

            <div>
              <div className="flex items-center">
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  Result Count
                  <span className="text-red-500 ml-1">*</span>
                </label>
                <Tooltip text="Number of results to fetch. Higher numbers may increase processing time." />
              </div>
              <input
                type="number"
                min="1"
                max="100"
                value={editedStep.parameters.resultCount || 10}
                onChange={(e) => handleConfigChange('resultCount', parseInt(e.target.value))}
                className={`mt-1 block w-full rounded-md shadow-sm focus:border-cyan-500 focus:ring-cyan-500 dark:bg-gray-700 dark:border-gray-600 ${
                  errors.resultCount ? 'border-red-300' : 'border-gray-300'
                }`}
              />
              {errors.resultCount && (
                <p className="mt-1 text-sm text-red-600">{errors.resultCount}</p>
              )}
              <p className="mt-1 text-sm text-gray-500">Choose between 1 and 100 results</p>
            </div>
          </div>
        );

      case 'openai.completion':
        return (
          <div className="space-y-6 bg-gray-50 dark:bg-gray-900 p-4 rounded-lg">
            <div className="flex items-center">
              <h3 className="text-base font-medium text-gray-900 dark:text-white">GPT Configuration</h3>
              <Tooltip text="Configure how the AI model processes and generates text based on your inputs." />
            </div>

            <div>
              <div className="flex items-center">
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  Model
                  <span className="text-red-500 ml-1">*</span>
                </label>
                <Tooltip text="Select the GPT model version. Higher versions are more capable but may be slower." />
              </div>
              <select
                value={editedStep.parameters.model || 'gpt-4'}
                onChange={(e) => handleConfigChange('model', e.target.value)}
                className={`mt-1 block w-full rounded-md shadow-sm focus:border-cyan-500 focus:ring-cyan-500 dark:bg-gray-700 dark:border-gray-600 ${
                  errors.model ? 'border-red-300' : 'border-gray-300'
                }`}
              >
                <option value="gpt-4">GPT-4 (Most Capable)</option>
                <option value="gpt-4-turbo">GPT-4 Turbo (Faster)</option>
                <option value="gpt-3.5-turbo">GPT-3.5 Turbo (Balanced)</option>
              </select>
              {errors.model && (
                <p className="mt-1 text-sm text-red-600">{errors.model}</p>
              )}
            </div>

            <div>
              <div className="flex items-center">
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  System Prompt
                  <span className="text-red-500 ml-1">*</span>
                </label>
                <Tooltip text="Instructions that guide the AI's behavior. Be clear and specific about what you want." />
              </div>
              <textarea
                value={editedStep.parameters.systemPrompt || ''}
                onChange={(e) => handleConfigChange('systemPrompt', e.target.value)}
                rows={4}
                className={`mt-1 block w-full rounded-md shadow-sm focus:border-cyan-500 focus:ring-cyan-500 dark:bg-gray-700 dark:border-gray-600 ${
                  errors.systemPrompt ? 'border-red-300' : 'border-gray-300'
                }`}
                placeholder="You are a helpful assistant that processes news articles. Your task is to..."
              />
              {errors.systemPrompt && (
                <p className="mt-1 text-sm text-red-600">{errors.systemPrompt}</p>
              )}
              <p className="mt-1 text-sm text-gray-500">Define the AI's role and specific instructions</p>
            </div>

            <div>
              <div className="flex items-center">
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  Temperature
                  <span className="text-red-500 ml-1">*</span>
                </label>
                <Tooltip text="Controls the randomness of the output. Lower values are more focused, higher values are more creative." />
              </div>
              <div className="flex items-center space-x-4">
                <input
                  type="range"
                  min="0"
                  max="1"
                  step="0.1"
                  value={editedStep.parameters.temperature || 0.7}
                  onChange={(e) => handleConfigChange('temperature', parseFloat(e.target.value))}
                  className="w-full accent-cyan-500"
                />
                <input
                  type="number"
                  step="0.1"
                  min="0"
                  max="1"
                  value={editedStep.parameters.temperature || 0.7}
                  onChange={(e) => handleConfigChange('temperature', parseFloat(e.target.value))}
                  className={`w-20 rounded-md shadow-sm focus:border-cyan-500 focus:ring-cyan-500 dark:bg-gray-700 dark:border-gray-600 ${
                    errors.temperature ? 'border-red-300' : 'border-gray-300'
                  }`}
                />
              </div>
              {errors.temperature && (
                <p className="mt-1 text-sm text-red-600">{errors.temperature}</p>
              )}
              <div className="flex justify-between text-sm text-gray-500 mt-1">
                <span>Focused (0.0)</span>
                <span>Balanced (0.5)</span>
                <span>Creative (1.0)</span>
              </div>
            </div>

            <div>
              <div className="flex items-center">
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  Max Tokens
                  <span className="text-red-500 ml-1">*</span>
                </label>
                <Tooltip text="Maximum length of the generated text. One token is roughly 4 characters or 3/4 of a word." />
              </div>
              <input
                type="number"
                min="1"
                max="4000"
                value={editedStep.parameters.maxTokens || 1000}
                onChange={(e) => handleConfigChange('maxTokens', parseInt(e.target.value))}
                className={`mt-1 block w-full rounded-md shadow-sm focus:border-cyan-500 focus:ring-cyan-500 dark:bg-gray-700 dark:border-gray-600 ${
                  errors.maxTokens ? 'border-red-300' : 'border-gray-300'
                }`}
              />
              {errors.maxTokens && (
                <p className="mt-1 text-sm text-red-600">{errors.maxTokens}</p>
              )}
              <p className="mt-1 text-sm text-gray-500">Recommended: 1000 tokens ≈ 750 words</p>
            </div>
          </div>
        );

      case 'database.operation':
        return (
          <div className="space-y-6 bg-gray-50 dark:bg-gray-900 p-4 rounded-lg">
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                Operation
                <span className="text-red-500 ml-1">*</span>
              </label>
              <select
                value={editedStep.parameters.operation || 'query'}
                onChange={(e) => handleConfigChange('operation', e.target.value)}
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-cyan-500 focus:ring-cyan-500 dark:bg-gray-700 dark:border-gray-600"
              >
                <option value="query">Query</option>
                <option value="insert">Insert</option>
                <option value="update">Update</option>
                <option value="delete">Delete</option>
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                Collection
                <span className="text-red-500 ml-1">*</span>
              </label>
              <input
                type="text"
                value={editedStep.parameters.collection || ''}
                onChange={(e) => handleConfigChange('collection', e.target.value)}
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-cyan-500 focus:ring-cyan-500 dark:bg-gray-700 dark:border-gray-600"
                placeholder="Enter collection name"
              />
            </div>
          </div>
        );

      case 'news.category':
        return (
          <div className="space-y-6 bg-gray-50 dark:bg-gray-900 p-4 rounded-lg">
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                Category
                <span className="text-red-500 ml-1">*</span>
              </label>
              <input
                type="text"
                value={editedStep.parameters.category || ''}
                onChange={(e) => handleConfigChange('category', e.target.value)}
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-cyan-500 focus:ring-cyan-500 dark:bg-gray-700 dark:border-gray-600"
                placeholder="Enter category name"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                Language
              </label>
              <select
                value={editedStep.parameters.language || 'en'}
                onChange={(e) => handleConfigChange('language', e.target.value)}
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-cyan-500 focus:ring-cyan-500 dark:bg-gray-700 dark:border-gray-600"
              >
                <option value="en">English</option>
                <option value="el">Greek</option>
              </select>
            </div>
          </div>
        );

      default:
        return null;
    }
  };

  return (
    <Dialog open={true} onClose={onCancel} className="relative z-50">
      <div className="fixed inset-0 bg-black/30 backdrop-blur-sm" aria-hidden="true" />
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <Dialog.Panel className="mx-auto max-w-2xl w-full rounded-xl bg-white dark:bg-gray-800 p-6 shadow-xl">
          <div className="border-b border-gray-200 dark:border-gray-700 pb-4 mb-6">
            <Dialog.Title className="text-xl font-semibold text-gray-900 dark:text-white">
              {step.id ? 'Edit Step' : 'Add Step'}
            </Dialog.Title>
            <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
              Configure this pipeline step to process your data exactly how you need it.
            </p>
          </div>

          <div className="space-y-6">
            <div className="bg-gray-50 dark:bg-gray-900 p-4 rounded-lg">
              <div className="space-y-4">
                <div>
                  <div className="flex items-center">
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                      Name
                      <span className="text-red-500 ml-1">*</span>
                    </label>
                    <Tooltip text="A descriptive name that identifies this step's purpose in the pipeline." />
                  </div>
                  <input
                    type="text"
                    value={editedStep.name}
                    onChange={(e) => {
                      setEditedStep({ ...editedStep, name: e.target.value });
                      if (errors.name) {
                        setErrors({ ...errors, name: '' });
                      }
                    }}
                    className={`mt-1 block w-full rounded-md shadow-sm focus:border-cyan-500 focus:ring-cyan-500 dark:bg-gray-700 dark:border-gray-600 ${
                      errors.name ? 'border-red-300' : 'border-gray-300'
                    }`}
                    placeholder="e.g., Fetch Technology News"
                  />
                  {errors.name && (
                    <p className="mt-1 text-sm text-red-600">{errors.name}</p>
                  )}
                </div>

                <div>
                  <div className="flex items-center">
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                      Type
                      <span className="text-red-500 ml-1">*</span>
                    </label>
                    <Tooltip text="The type of processing this step will perform." />
                  </div>
                  <select
                    value={editedStep.type}
                    onChange={(e) => handleTypeChange(e.target.value as StepType)}
                    className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-cyan-500 focus:ring-cyan-500 dark:bg-gray-700 dark:border-gray-600"
                  >
                    <option value="serp.search">Search Engine Results</option>
                    <option value="openai.completion">AI Text Processing</option>
                    <option value="database.operation">Database Operation</option>
                    <option value="news.category">News Category</option>
                  </select>
                  <p className="mt-1 text-sm text-gray-500">{stepTypeDescriptions[editedStep.type as StepType]}</p>
                </div>
              </div>
            </div>

            {renderConfigFields()}
          </div>

          <div className="flex justify-end space-x-4 mt-6 pt-4 border-t border-gray-200 dark:border-gray-700">
            <Button variant="secondary" onClick={onCancel}>
              Cancel
            </Button>
            <Button onClick={handleSave}>
              Save Changes
            </Button>
          </div>
        </Dialog.Panel>
      </div>
    </Dialog>
  );
} 