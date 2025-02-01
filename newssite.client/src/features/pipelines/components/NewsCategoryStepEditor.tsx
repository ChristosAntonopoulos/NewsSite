import React from 'react';
import { TransformationConfig } from '../../../types/Pipeline';
import { TextField } from '../../../shared/components/common/TextField';
import { Select } from '../../../shared/components/common/Select';
import { TransformationEditor } from './TransformationEditor';

interface NewsCategoryStepEditorProps {
  parameters: Record<string, string>;
  transformations: TransformationConfig[];
  onParametersChange: (parameters: Record<string, string>) => void;
  onTransformationsChange: (transformations: TransformationConfig[]) => void;
}

export function NewsCategoryStepEditor({
  parameters,
  transformations,
  onParametersChange,
  onTransformationsChange
}: NewsCategoryStepEditorProps) {
  const handleChange = (key: string, value: string) => {
    onParametersChange({ ...parameters, [key]: value });
  };

  // Available fields for transformations
  const availableFields = [
    'category',
    'initial_results',
    'keywords_results',
    'related_results'
  ];

  return (
    <div className="space-y-4 bg-gray-50 dark:bg-gray-900 p-4 rounded-lg">
      <h2 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-4">
        News Category Configuration
      </h2>

      <div className="space-y-1">
        <TextField
          label="Category"
          value={parameters.category || ''}
          onChange={(e) => handleChange('category', e.target.value)}
          placeholder="Enter news category"
          required
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
        <TextField
          label="Max Initial Results"
          type="number"
          value={parameters.maxInitialResults || '20'}
          onChange={(e) => handleChange('maxInitialResults', e.target.value)}
          placeholder="Maximum number of initial results"
        />

        <TextField
          label="Max Related Results"
          type="number"
          value={parameters.maxRelatedResults || '5'}
          onChange={(e) => handleChange('maxRelatedResults', e.target.value)}
          placeholder="Maximum number of related results per keyword"
        />
      </div>

      <div className="space-y-1">
        <label className="block text-sm font-medium text-gray-900 dark:text-gray-100">
          Output Path <span className="text-red-500">*</span>
        </label>
        <TextField
          value={parameters.outputPath || ''}
          onChange={(e) => handleChange('outputPath', e.target.value)}
          placeholder="e.g., news.results"
          required
        />
        <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
          Path where the processed results will be stored
        </p>
      </div>

      <div className="border-t border-gray-200 dark:border-gray-800 my-6" />

      <TransformationEditor
        transformations={transformations}
        onChange={onTransformationsChange}
        availableFields={availableFields}
      />
    </div>
  );
} 