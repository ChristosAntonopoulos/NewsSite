import React from 'react';
import { PipelineStep, StepType } from '../../../types/Pipeline';
import { STEP_TYPES, STEP_TYPE_DESCRIPTIONS } from '../constants';
import { PlusIcon, TrashIcon } from '@heroicons/react/24/outline';
import { SerpApiStepEditor } from './SerpApiStepEditor';
import { OpenAIStepEditor } from './OpenAIStepEditor';
import { NewsCategoryStepEditor } from './NewsCategoryStepEditor';

interface StepEditorProps {
    steps: PipelineStep[];
    onChange: (steps: PipelineStep[]) => void;
}

export function StepEditor({ steps, onChange }: StepEditorProps) {
    const handleAddStep = () => {
        const newStep: PipelineStep = {
            id: crypto.randomUUID(),
            name: '',
            type: 'serp.search',
            enabled: true,
            parameters: {},
            transformations: [],
            configuration: {},
            order: steps.length,
            passOutputAsInput: true
        };
        onChange([...steps, newStep]);
    };

    const handleDeleteStep = (index: number) => {
        const newSteps = steps.filter((_, i) => i !== index);
        newSteps.forEach((step, i) => {
            step.order = i;
        });
        onChange(newSteps);
    };

    const handleStepChange = (index: number, field: keyof PipelineStep, value: any) => {
        const newSteps = [...steps];
        newSteps[index] = {
            ...newSteps[index],
            [field]: value
        };
        onChange(newSteps);
    };

    const handleParametersChange = (index: number, parameters: Record<string, string>) => {
        const newSteps = [...steps];
        newSteps[index] = {
            ...newSteps[index],
            parameters
        };
        onChange(newSteps);
    };

    const handleTransformationsChange = (index: number, transformations: any[]) => {
        const newSteps = [...steps];
        newSteps[index] = {
            ...newSteps[index],
            transformations
        };
        onChange(newSteps);
    };

    const renderStepConfig = (step: PipelineStep, index: number) => {
        switch (step.type) {
            case 'serp.search':
                return (
                    <SerpApiStepEditor
                        parameters={step.parameters}
                        transformations={step.transformations}
                        onParametersChange={(params) => handleParametersChange(index, params)}
                        onTransformationsChange={(transforms) => handleTransformationsChange(index, transforms)}
                    />
                );
            case 'openai.completion':
                return (
                    <OpenAIStepEditor
                        parameters={step.parameters}
                        transformations={step.transformations}
                        onParametersChange={(params) => handleParametersChange(index, params)}
                        onTransformationsChange={(transforms) => handleTransformationsChange(index, transforms)}
                    />
                );
            case 'news.category':
                return (
                    <NewsCategoryStepEditor
                        parameters={step.parameters}
                        transformations={step.transformations}
                        onParametersChange={(params) => handleParametersChange(index, params)}
                        onTransformationsChange={(transforms) => handleTransformationsChange(index, transforms)}
                    />
                );
            case 'database.operation':
                return (
                    <div className="space-y-4">
                        <div>
                            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Operation</label>
                            <select
                                value={step.parameters.operation || 'query'}
                                onChange={(e) => handleParametersChange(index, { ...step.parameters, operation: e.target.value })}
                                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-cyan-500 focus:ring-cyan-500 dark:bg-gray-700 dark:border-gray-600"
                            >
                                <option value="query">Query</option>
                                <option value="insert">Insert</option>
                                <option value="update">Update</option>
                                <option value="delete">Delete</option>
                            </select>
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Collection</label>
                            <input
                                type="text"
                                value={step.parameters.collection || ''}
                                onChange={(e) => handleParametersChange(index, { ...step.parameters, collection: e.target.value })}
                                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-cyan-500 focus:ring-cyan-500 dark:bg-gray-700 dark:border-gray-600"
                                placeholder="Enter collection name"
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Query/Filter</label>
                            <textarea
                                value={step.parameters.query || ''}
                                onChange={(e) => handleParametersChange(index, { ...step.parameters, query: e.target.value })}
                                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-cyan-500 focus:ring-cyan-500 dark:bg-gray-700 dark:border-gray-600"
                                rows={3}
                                placeholder="Enter query or filter criteria"
                            />
                        </div>
                    </div>
                );
            default:
                return null;
        }
    };

    return (
        <div className="space-y-4">
            {steps.map((step, index) => (
                <div key={step.id} className="bg-white dark:bg-gray-800 p-6 rounded-lg shadow border border-gray-200 dark:border-gray-700">
                    <div className="flex items-center justify-between mb-6">
                        <h3 className="text-lg font-medium text-gray-900 dark:text-white">Step {index + 1}</h3>
                        <button
                            type="button"
                            onClick={() => handleDeleteStep(index)}
                            className="text-red-600 hover:text-red-800 dark:text-red-400 dark:hover:text-red-300"
                        >
                            <TrashIcon className="h-5 w-5" />
                        </button>
                    </div>
                    <div className="space-y-6">
                        <div>
                            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Name</label>
                            <input
                                type="text"
                                value={step.name}
                                onChange={(e) => handleStepChange(index, 'name', e.target.value)}
                                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-cyan-500 focus:ring-cyan-500 dark:bg-gray-700 dark:border-gray-600"
                                placeholder="Enter step name"
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Type</label>
                            <select
                                value={step.type}
                                onChange={(e) => handleStepChange(index, 'type', e.target.value as StepType)}
                                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-cyan-500 focus:ring-cyan-500 dark:bg-gray-700 dark:border-gray-600"
                            >
                                {Object.entries(STEP_TYPES).map(([type, label]) => (
                                    <option key={type} value={type}>
                                        {label}
                                    </option>
                                ))}
                            </select>
                            {STEP_TYPE_DESCRIPTIONS[step.type] && (
                                <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
                                    {STEP_TYPE_DESCRIPTIONS[step.type]}
                                </p>
                            )}
                        </div>
                        <div className="bg-gray-50 dark:bg-gray-700/50 p-4 rounded-md">
                            <h4 className="text-sm font-medium text-gray-900 dark:text-white mb-4">Configuration</h4>
                            {renderStepConfig(step, index)}
                        </div>
                        <div>
                            <label className="flex items-center">
                                <input
                                    type="checkbox"
                                    checked={step.enabled}
                                    onChange={(e) => handleStepChange(index, 'enabled', e.target.checked)}
                                    className="rounded border-gray-300 text-cyan-600 shadow-sm focus:border-cyan-500 focus:ring-cyan-500"
                                />
                                <span className="ml-2 text-sm text-gray-700 dark:text-gray-300">Enabled</span>
                            </label>
                        </div>
                        <div>
                            <label className="flex items-center">
                                <input
                                    type="checkbox"
                                    checked={step.passOutputAsInput}
                                    onChange={(e) => handleStepChange(index, 'passOutputAsInput', e.target.checked)}
                                    className="rounded border-gray-300 text-cyan-600 shadow-sm focus:border-cyan-500 focus:ring-cyan-500"
                                />
                                <span className="ml-2 text-sm text-gray-700 dark:text-gray-300">Pass Output as Input to Next Step</span>
                            </label>
                        </div>
                        <div>
                            <label className="flex items-center">
                                <input
                                    type="checkbox"
                                    checked={step.ignoreErrors}
                                    onChange={(e) => handleStepChange(index, 'ignoreErrors', e.target.checked)}
                                    className="rounded border-gray-300 text-cyan-600 shadow-sm focus:border-cyan-500 focus:ring-cyan-500"
                                />
                                <span className="ml-2 text-sm text-gray-700 dark:text-gray-300">Ignore Errors</span>
                            </label>
                        </div>
                    </div>
                </div>
            ))}
            <button
                type="button"
                onClick={handleAddStep}
                className="flex items-center justify-center w-full px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-cyan-600 hover:bg-cyan-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-cyan-500 dark:focus:ring-offset-gray-800"
            >
                <PlusIcon className="h-5 w-5 mr-2" />
                Add Step
            </button>
        </div>
    );
} 