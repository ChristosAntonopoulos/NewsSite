import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Pipeline, PipelineMode, RetryPolicy } from '../../../types/Pipeline';
import { StepEditor } from '../components/StepEditor';
import { Button } from '../../../shared/components/common/Button';
import { LoadingSpinner } from '../../../shared/components/common/LoadingSpinner';
import { PIPELINE_MODES } from '../constants';
import { pipelineService } from '../services/pipelineService';

const defaultRetryPolicy: RetryPolicy = {
  maxRetries: 3,
  initialDelay: '00:00:01',
  exponentialBackoff: true,
  retryableExceptions: []
};

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
  retryPolicy: defaultRetryPolicy
};

export default function PipelineEditor() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [pipeline, setPipeline] = useState<Pipeline>(defaultPipeline);
  const [loading, setLoading] = useState(id ? true : false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<'basic' | 'steps' | 'advanced'>('basic');
  const [validationErrors, setValidationErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    const loadPipeline = async () => {
      if (!id) return;
      try {
        const data = await pipelineService.getPipeline(id);
        setPipeline(data);
      } catch (error) {
        console.error('Failed to load pipeline:', error);
        setError('Failed to load pipeline');
      } finally {
        setLoading(false);
      }
    };

    if (id) {
      loadPipeline();
    }
  }, [id]);

  const validatePipeline = () => {
    const errors: Record<string, string> = {};

    if (!pipeline.name.trim()) {
      errors.name = 'Name is required';
    }

    if (!pipeline.mode) {
      errors.mode = 'Mode is required';
    }

    if (pipeline.steps.length === 0) {
      errors.steps = 'At least one step is required';
    }

    setValidationErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleModeChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const mode = e.target.value as PipelineMode;
    setPipeline({ ...pipeline, mode });
    if (validationErrors.mode) {
      setValidationErrors({ ...validationErrors, mode: '' });
    }
  };

  const handleSave = async () => {
    if (!validatePipeline()) {
      setActiveTab('basic');
      return;
    }

    try {
      setSaving(true);
      setError(null);

      if (id) {
        await pipelineService.updatePipeline(id, pipeline);
      } else {
        await pipelineService.createPipeline(pipeline);
      }
      navigate('/pipelines');
    } catch (err) {
      console.error('Failed to save pipeline:', err);
      setError(err instanceof Error ? err.message : 'Failed to save pipeline');
    } finally {
      setSaving(false);
    }
  };

  if (loading) return (
    <div className="flex justify-center items-center min-h-[200px]">
      <LoadingSpinner className="w-8 h-8 text-cyan-600" />
    </div>
  );

  return (
    <div className="container mx-auto px-4 py-8 max-w-4xl">
      <div className="bg-white dark:bg-gray-800 shadow-lg rounded-lg overflow-hidden">
        <div className="border-b border-gray-200 dark:border-gray-700">
          <nav className="flex -mb-px">
            {(['basic', 'steps', 'advanced'] as const).map((tab) => (
              <button
                key={tab}
                onClick={() => setActiveTab(tab)}
                className={`py-4 px-6 text-sm font-medium border-b-2 ${
                  activeTab === tab
                    ? 'border-cyan-500 text-cyan-600 dark:text-cyan-400'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300 dark:text-gray-400 dark:hover:text-gray-300'
                }`}
              >
                {tab.charAt(0).toUpperCase() + tab.slice(1)}
              </button>
            ))}
          </nav>
        </div>

        <form onSubmit={(e) => { e.preventDefault(); handleSave(); }} className="p-6 space-y-6">
          {activeTab === 'basic' && (
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  Name
                  <span className="text-red-500 ml-1">*</span>
                </label>
                <input
                  type="text"
                  value={pipeline.name}
                  onChange={(e) => {
                    setPipeline({ ...pipeline, name: e.target.value });
                    if (validationErrors.name) {
                      setValidationErrors({ ...validationErrors, name: '' });
                    }
                  }}
                  className={`mt-1 block w-full rounded-md shadow-sm focus:border-cyan-500 focus:ring-cyan-500 dark:bg-gray-700 dark:border-gray-600 ${
                    validationErrors.name ? 'border-red-300' : 'border-gray-300'
                  }`}
                  required
                />
                {validationErrors.name && (
                  <p className="mt-1 text-sm text-red-600">{validationErrors.name}</p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Description</label>
                <textarea
                  value={pipeline.description}
                  onChange={(e) => setPipeline({ ...pipeline, description: e.target.value })}
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-cyan-500 focus:ring-cyan-500 dark:bg-gray-700 dark:border-gray-600"
                  rows={3}
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  Mode
                  <span className="text-red-500 ml-1">*</span>
                </label>
                <select
                  value={pipeline.mode}
                  onChange={handleModeChange}
                  className={`mt-1 block w-full rounded-md shadow-sm focus:border-cyan-500 focus:ring-cyan-500 dark:bg-gray-700 dark:border-gray-600 ${
                    validationErrors.mode ? 'border-red-300' : 'border-gray-300'
                  }`}
                >
                  {Object.entries(PIPELINE_MODES).map(([mode, label]) => (
                    <option key={mode} value={mode}>
                      {label}
                    </option>
                  ))}
                </select>
                {validationErrors.mode && (
                  <p className="mt-1 text-sm text-red-600">{validationErrors.mode}</p>
                )}
              </div>

              <div className="flex items-center space-x-4">
                <label className="flex items-center">
                  <input
                    type="checkbox"
                    checked={pipeline.enabled}
                    onChange={(e) => setPipeline({ ...pipeline, enabled: e.target.checked })}
                    className="rounded border-gray-300 text-cyan-600 shadow-sm focus:border-cyan-500 focus:ring-cyan-500"
                  />
                  <span className="ml-2 text-sm text-gray-700 dark:text-gray-300">Enabled</span>
                </label>
              </div>
            </div>
          )}

          {activeTab === 'steps' && (
            <div className="space-y-4">
              <StepEditor
                steps={pipeline.steps}
                onChange={(steps) => {
                  setPipeline({ ...pipeline, steps });
                  if (validationErrors.steps) {
                    setValidationErrors({ ...validationErrors, steps: '' });
                  }
                }}
              />
              {validationErrors.steps && (
                <p className="mt-1 text-sm text-red-600">{validationErrors.steps}</p>
              )}
            </div>
          )}

          {activeTab === 'advanced' && (
            <div className="space-y-6">
              <div className="bg-gray-50 dark:bg-gray-900 p-4 rounded-lg">
                <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-4">Retry Policy</h3>
                <div className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Max Retries</label>
                    <input
                      type="number"
                      min="0"
                      value={pipeline.retryPolicy.maxRetries}
                      onChange={(e) => setPipeline({
                        ...pipeline,
                        retryPolicy: {
                          ...pipeline.retryPolicy,
                          maxRetries: parseInt(e.target.value)
                        }
                      })}
                      className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-cyan-500 focus:ring-cyan-500 dark:bg-gray-700 dark:border-gray-600"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                      Initial Delay (format: hh:mm:ss)
                    </label>
                    <input
                      type="text"
                      value={pipeline.retryPolicy.initialDelay}
                      onChange={(e) => setPipeline({
                        ...pipeline,
                        retryPolicy: {
                          ...pipeline.retryPolicy,
                          initialDelay: e.target.value
                        }
                      })}
                      placeholder="00:00:01"
                      className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-cyan-500 focus:ring-cyan-500 dark:bg-gray-700 dark:border-gray-600"
                    />
                  </div>

                  <div>
                    <label className="flex items-center">
                      <input
                        type="checkbox"
                        checked={pipeline.retryPolicy.exponentialBackoff}
                        onChange={(e) => setPipeline({
                          ...pipeline,
                          retryPolicy: {
                            ...pipeline.retryPolicy,
                            exponentialBackoff: e.target.checked
                          }
                        })}
                        className="rounded border-gray-300 text-cyan-600 shadow-sm focus:border-cyan-500 focus:ring-cyan-500"
                      />
                      <span className="ml-2 text-sm text-gray-700 dark:text-gray-300">Use Exponential Backoff</span>
                    </label>
                  </div>
                </div>
              </div>
            </div>
          )}

          {error && (
            <div className="rounded-md bg-red-50 dark:bg-red-900/50 p-4">
              <div className="flex">
                <div className="flex-shrink-0">
                  <svg className="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
                    <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
                  </svg>
                </div>
                <div className="ml-3">
                  <p className="text-sm font-medium text-red-800 dark:text-red-200">{error}</p>
                </div>
              </div>
            </div>
          )}

          <div className="flex justify-end space-x-4 pt-4 border-t border-gray-200 dark:border-gray-700">
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
    </div>
  );
} 