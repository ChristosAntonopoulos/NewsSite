import { useState } from 'react';
import { Pipeline, PipelineExecutionContext } from '../../../types/Pipeline';
import { Switch } from '@headlessui/react';
import { PlayIcon, PencilIcon, TrashIcon, XMarkIcon, EyeIcon } from '@heroicons/react/24/outline';
import { Link } from 'react-router-dom';
import { PipelineExecution } from './PipelineExecution';
import { pipelineService } from '../services/pipelineService';
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter';
import { oneDark } from 'react-syntax-highlighter/dist/esm/styles/prism';
import { Dialog, Transition } from '@headlessui/react';
import { Fragment } from 'react';

interface PipelineCardProps {
  pipeline: Pipeline;
  onUpdate: () => void;
}

interface ResultsModalProps {
  isOpen: boolean;
  onClose: () => void;
  results: any;
}

function ResultsModal({ isOpen, onClose, results }: ResultsModalProps) {
  return (
    <Transition appear show={isOpen} as={Fragment}>
      <Dialog as="div" className="relative z-50" onClose={onClose}>
        <Transition.Child
          as={Fragment}
          enter="ease-out duration-300"
          enterFrom="opacity-0"
          enterTo="opacity-100"
          leave="ease-in duration-200"
          leaveFrom="opacity-100"
          leaveTo="opacity-0"
        >
          <div className="fixed inset-0 bg-black bg-opacity-25" />
        </Transition.Child>

        <div className="fixed inset-0 overflow-y-auto">
          <div className="flex min-h-full items-center justify-center p-4">
            <Transition.Child
              as={Fragment}
              enter="ease-out duration-300"
              enterFrom="opacity-0 scale-95"
              enterTo="opacity-100 scale-100"
              leave="ease-in duration-200"
              leaveFrom="opacity-100 scale-100"
              leaveTo="opacity-0 scale-95"
            >
              <Dialog.Panel className="w-full max-w-3xl transform overflow-hidden rounded-2xl bg-white dark:bg-gray-800 p-6 shadow-xl transition-all">
                <div className="flex justify-between items-center mb-4">
                  <Dialog.Title className="text-lg font-medium text-gray-900 dark:text-white">
                    Pipeline Execution Results
                  </Dialog.Title>
                  <button
                    onClick={onClose}
                    className="text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300"
                  >
                    <XMarkIcon className="h-6 w-6" />
                  </button>
                </div>
                <div className="mt-4">
                  <div className="rounded-lg bg-gray-50 dark:bg-gray-900">
                    <SyntaxHighlighter
                      language="json"
                      style={oneDark}
                      customStyle={{
                        margin: 0,
                        padding: '1rem',
                        borderRadius: '0.5rem',
                        maxHeight: '70vh',
                        overflow: 'auto'
                      }}
                    >
                      {JSON.stringify(results, null, 2)}
                    </SyntaxHighlighter>
                  </div>
                </div>
              </Dialog.Panel>
            </Transition.Child>
          </div>
        </div>
      </Dialog>
    </Transition>
  );
}

export function PipelineCard({ pipeline, onUpdate }: PipelineCardProps) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [executionContext, setExecutionContext] = useState<PipelineExecutionContext | null>(null);
  const [executionResults, setExecutionResults] = useState<any>(null);
  const [isResultsModalOpen, setIsResultsModalOpen] = useState(false);

  const togglePipeline = async () => {
    try {
      await pipelineService.togglePipeline(pipeline.id, !pipeline.enabled);
      onUpdate();
    } catch (err) {
      console.error('Failed to toggle pipeline:', err);
    }
  };

  const handleDelete = async () => {
    if (!window.confirm('Are you sure you want to delete this pipeline?')) return;

    try {
      await pipelineService.deletePipeline(pipeline.id);
      onUpdate();
    } catch (err) {
      console.error(err);
      setError('Failed to delete pipeline');
    }
  };

  const handleExecute = async () => {
    setLoading(true);
    setError(null);
    setExecutionContext(null);
    setExecutionResults(null);

    try {
      const results = await pipelineService.executePipeline(pipeline.id);
      setExecutionResults(results);
      setIsResultsModalOpen(true);
    } catch (err) {
      console.error(err);
      setError('Failed to execute pipeline');
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = async () => {
    try {
      await pipelineService.cancelPipeline(pipeline.id);
    } catch (err) {
      console.error(err);
      setError('Failed to cancel pipeline execution');
    }
  };

  return (
    <div className="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
      <div className="flex justify-between items-start mb-4">
        <div>
          <h3 className="text-xl font-semibold text-gray-900 dark:text-white">
            {pipeline.name}
          </h3>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
            {pipeline.description || 'No description'}
          </p>
        </div>
        <div className="flex space-x-2">
          <button
            onClick={handleExecute}
            disabled={loading}
            className="p-2 text-green-600 hover:text-green-700 dark:text-green-400 dark:hover:text-green-300 transition-colors"
            title="Execute pipeline"
          >
            <PlayIcon className="h-5 w-5" />
          </button>
          {executionResults && (
            <button
              onClick={() => setIsResultsModalOpen(true)}
              className="p-2 text-blue-600 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300 transition-colors"
              title="View results"
            >
              <EyeIcon className="h-5 w-5" />
            </button>
          )}
          <Link
            to={`/pipelines/edit/${pipeline.id}`}
            className="p-2 text-blue-600 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300 transition-colors"
            title="Edit pipeline"
          >
            <PencilIcon className="h-5 w-5" />
          </Link>
          <button
            onClick={handleDelete}
            className="p-2 text-red-600 hover:text-red-700 dark:text-red-400 dark:hover:text-red-300 transition-colors"
            title="Delete pipeline"
          >
            <TrashIcon className="h-5 w-5" />
          </button>
        </div>
      </div>

      <div className="mt-4">
        <div className="text-sm text-gray-600 dark:text-gray-300">
          <span className="font-medium">Steps:</span> {pipeline.steps?.length || 0}
        </div>
        <div className="text-sm text-gray-600 dark:text-gray-300">
          <span className="font-medium">Mode:</span> {pipeline.mode}
        </div>
      </div>

      {error && (
        <div className="mt-4 text-sm text-red-600 dark:text-red-400">
          {error}
        </div>
      )}

      {loading && !executionContext && (
        <div className="mt-4 text-sm text-gray-600 dark:text-gray-400">
          <div className="flex items-center">
            <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-blue-500" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
              <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
              <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
            </svg>
            Executing pipeline...
          </div>
        </div>
      )}

      {executionContext && (
        <div className="mt-6 border-t pt-4">
          <PipelineExecution
            executionContext={executionContext}
            onCancel={handleCancel}
          />
        </div>
      )}

      <ResultsModal
        isOpen={isResultsModalOpen}
        onClose={() => setIsResultsModalOpen(false)}
        results={executionResults}
      />
    </div>
  );
} 