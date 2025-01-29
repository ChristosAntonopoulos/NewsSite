import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Pipeline, PipelineMode } from '../../../types/Pipeline';
import { StepEditor } from './StepEditor';
import { Button } from '../../../shared/components/common/Button';
import { LoadingSpinner } from '../../../shared/components/common/LoadingSpinner';
import { PIPELINE_MODES } from '../constants';
import { pipelineService } from '../services/pipelineService';

const defaultPipeline: Pipeline = {
  id: '',
  name: '',
  description: '',
  mode: 'Sequential',
  enabled: true,
  isEnabled: true,
  steps: [],
  status: 'inactive',
  createdAt: new Date().toISOString(),
  updatedAt: new Date().toISOString(),
  retryPolicy: {
    maxRetries: 3,
    initialDelay: '00:00:01',
    exponentialBackoff: true
  }
};

export function PipelineEditor() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [pipeline, setPipeline] = useState<Pipeline>(defaultPipeline);
  const [loading, setLoading] = useState(id ? true : false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleModeChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setPipeline({ ...pipeline, mode: e.target.value as PipelineMode });
  };

  const handleSave = async () => {
    try {
      setSaving(true);
      if (id) {
        await pipelineService.updatePipeline(id, pipeline);
      } else {
        await pipelineService.createPipeline(pipeline);
      }
      navigate('/pipelines');
    } catch (error) {
      console.error('Failed to save pipeline:', error);
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <LoadingSpinner />;

  return (
    <div className="container mx-auto px-4 py-8 max-w-4xl">
      <h1 className="text-3xl font-bold mb-8 text-gray-900 dark:text-white">
        {id ? 'Edit Pipeline' : 'Create Pipeline'}
      </h1>

      <form onSubmit={(e) => { e.preventDefault(); handleSave(); }} className="space-y-6">
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Name</label>
            <input
              type="text"
              value={pipeline.name}
              onChange={(e) => setPipeline({ ...pipeline, name: e.target.value })}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-cyan-500 focus:ring-cyan-500"
              required
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Description</label>
            <textarea
              value={pipeline.description}
              onChange={(e) => setPipeline({ ...pipeline, description: e.target.value })}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-cyan-500 focus:ring-cyan-500"
              rows={3}
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Mode</label>
            <select
              value={pipeline.mode}
              onChange={handleModeChange}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-cyan-500 focus:ring-cyan-500"
            >
              {(Object.keys(PIPELINE_MODES) as PipelineMode[]).map((mode) => (
                <option key={mode} value={mode}>
                  {PIPELINE_MODES[mode]}
                </option>
              ))}
            </select>
          </div>
        </div>

        <StepEditor
          steps={pipeline.steps}
          onChange={(steps) => setPipeline({ ...pipeline, steps })}
        />

        {error && <div className="text-red-500 text-sm">{error}</div>}

        <div className="flex justify-end space-x-4">
          <Button
            type="button"
            variant="secondary"
            onClick={() => navigate('/pipelines')}
            disabled={saving}
          >
            Cancel
          </Button>
          <Button type="submit" loading={saving}>
            {id ? 'Update' : 'Create'} Pipeline
          </Button>
        </div>
      </form>
    </div>
  );
} 