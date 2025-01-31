import React from 'react';
import { TransformationConfig } from '../../../types/Pipeline';
import { TextField } from '../../../shared/components/common/TextField';
import { Select } from '../../../shared/components/common/Select';
import { TransformationEditor } from './TransformationEditor';

interface SerpApiStepEditorProps {
  parameters: Record<string, string>;
  transformations: TransformationConfig[];
  onParametersChange: (parameters: Record<string, string>) => void;
  onTransformationsChange: (transformations: TransformationConfig[]) => void;
}

const searchEngines = [
  // Search Types that match Google Custom Search API
  { value: 'google', label: 'Google Web Search (Default)' },
  { value: 'google_images', label: 'Google Image Search' },
  { value: 'google_news', label: 'Google News Search' },
  { value: 'google_blogs', label: 'Google Blog Search' },
  { value: 'google_videos', label: 'Google Video Search' },
  { value: 'google_books', label: 'Google Books Search' },
  { value: 'bing', label: 'Bing Web Search' },
  { value: 'bing_images', label: 'Bing Image Search' },
  { value: 'yahoo', label: 'Yahoo Search' },
  { value: 'duckduckgo', label: 'DuckDuckGo Search' },
  { value: 'baidu', label: 'Baidu Search' },
  { value: 'yandex', label: 'Yandex Search' },
  { value: 'ebay', label: 'eBay Search' },
  { value: 'amazon', label: 'Amazon Search' },
  { value: 'youtube', label: 'YouTube Video Search' }
];

export function SerpApiStepEditor({
  parameters,
  transformations,
  onParametersChange,
  onTransformationsChange
}: SerpApiStepEditorProps) {
  const handleChange = (key: string, value: string) => {
    onParametersChange({ ...parameters, [key]: value });
  };

  // Available fields for transformations
  const availableFields = [
    'title',
    'snippet',
    'link',
    'source',
    'date',
    'imageUrl',
    'keywords'
  ];

  return (
    <div className="space-y-4 bg-gray-50 dark:bg-gray-900 p-4 rounded-lg">
      <h2 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-4">
        SERP API Configuration
      </h2>

      <TextField
        label="Search Query"
        value={parameters.query || ''}
        onChange={(e) => handleChange('query', e.target.value)}
        placeholder="Enter your search query"
        required
      />

      <div className="space-y-1">
        <label className="block text-sm font-medium text-gray-900 dark:text-gray-100">
          Search Engine
        </label>
        <select
          value={parameters.engine || ''}
          onChange={(e) => handleChange('engine', e.target.value)}
          className="block w-full px-3 py-2 bg-gray-50 dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:focus:ring-indigo-400"
        >
          {searchEngines.map((engine) => (
            <option key={engine.value} value={engine.value}>
              {engine.label}
            </option>
          ))}
        </select>
      </div>

      <div className="grid grid-cols-2 gap-4">
        <TextField
          label="Max Results"
          type="number"
          value={parameters.maxResults || '10'}
          onChange={(e) => handleChange('maxResults', e.target.value)}
          placeholder="Maximum number of results"
        />

        <div className="flex items-center space-x-2">
          <input
            type="checkbox"
            id="includeImages"
            checked={parameters.includeImages === 'true'}
            onChange={(e) => handleChange('includeImages', e.target.checked.toString())}
            className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
          />
          <label htmlFor="includeImages" className="text-sm font-medium text-gray-900 dark:text-gray-100">
            Include Images
          </label>
        </div>
      </div>

      <TextField
        label="Additional Parameters (JSON)"
        value={parameters.additionalParams || ''}
        onChange={(e) => handleChange('additionalParams', e.target.value)}
        multiline
        rows={4}
        placeholder='{"param1": "value1", "param2": "value2"}'
      />

      <div className="border-t border-gray-200 dark:border-gray-800 my-6" />

      <TransformationEditor
        transformations={transformations}
        onChange={onTransformationsChange}
        availableFields={availableFields}
      />
    </div>
  );
} 