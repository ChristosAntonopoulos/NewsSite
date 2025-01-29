import { useState } from 'react';
import { Dialog } from '@headlessui/react';
import { PipelineStep, StepConfiguration } from '../../types/Pipeline';
import { Button } from '../../shared/components/common/Button';

interface StepModalProps {
  step: PipelineStep;
  onSave: (step: PipelineStep) => void;
  onCancel: () => void;
}

export function StepModal({ step, onSave, onCancel }: StepModalProps) {
  const [editedStep, setEditedStep] = useState<PipelineStep>({ ...step });

  const handleConfigChange = (key: string, value: any) => {
    setEditedStep({
      ...editedStep,
      configuration: {
        ...editedStep.configuration,
        [key]: value,
      },
    });
  };

  const handleMetadataChange = (key: string, value: any) => {
    setEditedStep({
      ...editedStep,
      metadata: {
        ...editedStep.metadata,
        [key]: value,
      },
    });
  };

  const renderConfigFields = () => {
    if (editedStep.type === 'SerpApi') {
      return (
        <>
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
              Search Query
            </label>
            <input
              type="text"
              value={editedStep.configuration.searchQuery || ''}
              onChange={(e) => handleConfigChange('searchQuery', e.target.value)}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-cyan-500 focus:ring-cyan-500"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
              Engine
            </label>
            <input
              type="text"
              value={editedStep.configuration.engine || ''}
              onChange={(e) => handleConfigChange('engine', e.target.value)}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-cyan-500 focus:ring-cyan-500"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
              Result Count
            </label>
            <input
              type="number"
              value={editedStep.configuration.resultCount || 10}
              onChange={(e) => handleConfigChange('resultCount', parseInt(e.target.value))}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-cyan-500 focus:ring-cyan-500"
            />
          </div>
        </>
      );
    }

    if (editedStep.type === 'GPT') {
      return (
        <>
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
              Model
            </label>
            <input
              type="text"
              value={editedStep.configuration.model || ''}
              onChange={(e) => handleConfigChange('model', e.target.value)}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-cyan-500 focus:ring-cyan-500"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
              System Prompt
            </label>
            <textarea
              value={editedStep.configuration.systemPrompt || ''}
              onChange={(e) => handleConfigChange('systemPrompt', e.target.value)}
              rows={4}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-cyan-500 focus:ring-cyan-500"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
              Temperature
            </label>
            <input
              type="number"
              step="0.1"
              min="0"
              max="1"
              value={editedStep.configuration.temperature || 0.7}
              onChange={(e) => handleConfigChange('temperature', parseFloat(e.target.value))}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-cyan-500 focus:ring-cyan-500"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
              Max Tokens
            </label>
            <input
              type="number"
              value={editedStep.configuration.maxTokens || 1000}
              onChange={(e) => handleConfigChange('maxTokens', parseInt(e.target.value))}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-cyan-500 focus:ring-cyan-500"
            />
          </div>
        </>
      );
    }

    return null;
  };

  return (
    <Dialog open={true} onClose={onCancel} className="relative z-50">
      <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <Dialog.Panel className="mx-auto max-w-xl rounded-lg bg-white dark:bg-gray-800 p-6">
          <Dialog.Title className="text-lg font-medium text-gray-900 dark:text-white mb-4">
            {step.id ? 'Edit Step' : 'Add Step'}
          </Dialog.Title>

          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                Name
              </label>
              <input
                type="text"
                value={editedStep.name}
                onChange={(e) => setEditedStep({ ...editedStep, name: e.target.value })}
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-cyan-500 focus:ring-cyan-500"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                Type
              </label>
              <select
                value={editedStep.type}
                onChange={(e) => setEditedStep({ ...editedStep, type: e.target.value as 'SerpApi' | 'GPT' })}
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-cyan-500 focus:ring-cyan-500"
              >
                <option value="SerpApi">SerpAPI</option>
                <option value="GPT">GPT</option>
              </select>
            </div>

            {renderConfigFields()}

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                API Key
              </label>
              <input
                type="password"
                value={editedStep.configuration.apiKey || ''}
                onChange={(e) => handleConfigChange('apiKey', e.target.value)}
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-cyan-500 focus:ring-cyan-500"
              />
            </div>

            <div className="flex justify-end space-x-4 mt-6">
              <Button variant="secondary" onClick={onCancel}>
                Cancel
              </Button>
              <Button onClick={() => onSave(editedStep)}>Save</Button>
            </div>
          </div>
        </Dialog.Panel>
      </div>
    </Dialog>
  );
} 