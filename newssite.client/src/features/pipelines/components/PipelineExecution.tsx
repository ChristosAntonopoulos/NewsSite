import { useEffect, useState } from 'react';
import { PipelineExecutionContext, StepExecutionState } from '../../../types/Pipeline';
import { CheckCircleIcon, XCircleIcon, ClockIcon, XMarkIcon, ClipboardIcon, ClipboardDocumentCheckIcon } from '@heroicons/react/24/outline';
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter';
import { oneDark } from 'react-syntax-highlighter/dist/esm/styles/prism';

interface PipelineExecutionProps {
  executionContext: any;
  onCancel?: () => void;
}

interface StepDetailsModalProps {
  stepId: string;
  stepResult: {
    logs: Log[];
    output: any;
    status: string;
  };
  onClose: () => void;
}

interface Log {
  Level: string;
  Message: string;
  Timestamp: string;
  stepId: string;
}

const getLogLevelClass = (level: string) => {
  switch (level) {
    case 'Error':
      return 'text-red-600 dark:text-red-400';
    case 'Warning':
      return 'text-yellow-600 dark:text-yellow-400';
    default:
      return 'text-gray-700 dark:text-gray-300';
  }
};

function StepDetailsModal({ stepId, stepResult, onClose }: StepDetailsModalProps) {
  const stepLogs: Log[] = stepResult.logs || [];
  const output = stepResult.output;
  const [isCopied, setIsCopied] = useState(false);

  const handleCopy = async () => {
    if (output) {
      try {
        await navigator.clipboard.writeText(JSON.stringify(output, null, 2));
        setIsCopied(true);
        setTimeout(() => setIsCopied(false), 2000); // Reset after 2 seconds
      } catch (err) {
        console.error('Failed to copy:', err);
      }
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white dark:bg-gray-900 rounded-lg w-11/12 max-w-4xl max-h-[90vh] overflow-hidden flex flex-col">
        <div className="flex justify-between items-center p-4 border-b border-gray-200 dark:border-gray-700">
          <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Step {stepId} Details</h3>
          <button
            onClick={onClose}
            className="text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
          >
            <XMarkIcon className="h-6 w-6" />
          </button>
        </div>
        
        <div className="flex-1 overflow-y-auto p-4 space-y-4">
          {stepLogs.length > 0 && (
            <div className="space-y-2">
              <h4 className="text-base font-semibold text-gray-900 dark:text-white">Logs</h4>
              <div className="bg-gray-50 dark:bg-gray-800 rounded-md p-3 space-y-2">
                {stepLogs.map((log: Log, index: number) => (
                  <div
                    key={index}
                    className={`text-base font-mono ${getLogLevelClass(log.Level)}`}
                  >
                    <span className="text-gray-500 dark:text-gray-400">
                      {new Date(log.Timestamp).toLocaleTimeString()}
                    </span>
                    {' - '}
                    {log.Message}
                  </div>
                ))}
              </div>
            </div>
          )}

          {output && (
            <div className="space-y-2">
              <div className="flex justify-between items-center">
                <h4 className="text-base font-semibold text-gray-900 dark:text-white">Output</h4>
                <button
                  onClick={handleCopy}
                  className="flex items-center space-x-2 px-3 py-1 text-sm font-medium text-gray-700 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white transition-colors rounded-md hover:bg-gray-100 dark:hover:bg-gray-800"
                  title={isCopied ? "Copied!" : "Copy to clipboard"}
                >
                  {isCopied ? (
                    <ClipboardDocumentCheckIcon className="h-5 w-5 text-green-500" />
                  ) : (
                    <ClipboardIcon className="h-5 w-5" />
                  )}
                  <span>{isCopied ? "Copied!" : "Copy"}</span>
                </button>
              </div>
              <div className="rounded-md overflow-hidden">
                <SyntaxHighlighter
                  language="json"
                  style={oneDark}
                  customStyle={{
                    margin: 0,
                    borderRadius: '0.375rem',
                    fontSize: '0.875rem',
                  }}
                >
                  {JSON.stringify(output, null, 2)}
                </SyntaxHighlighter>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export function PipelineExecution({ executionContext, onCancel }: PipelineExecutionProps) {
  const [selectedStep, setSelectedStep] = useState<string | null>(null);

  if (!executionContext) {
    return <div className="text-lg text-gray-700 dark:text-gray-300">No execution context available</div>;
  }

  const getDuration = () => {
    if (!executionContext.startTime) return null;
    const start = new Date(executionContext.startTime);
    const end = executionContext.endTime ? new Date(executionContext.endTime) : new Date();
    const duration = (end.getTime() - start.getTime()) / 1000;
    return duration.toFixed(2);
  };

  // Map step outputs to a format the component expects
  const stepResults = Object.entries(executionContext.stepOutputs || {}).reduce((acc, [stepId, data]: [string, any]) => {
    acc[stepId] = {
      output: data.output,
      status: data.state,
      logs: executionContext.logs?.filter((log: Log) => log.stepId === stepId) || []
    };
    return acc;
  }, {} as Record<string, any>);

  const stepIds = Object.keys(stepResults);

  const getStepIcon = (state: string) => {
    switch (state) {
      case 'Completed':
        return <CheckCircleIcon className="h-6 w-6 text-green-600" />;
      case 'Failed':
        return <XCircleIcon className="h-6 w-6 text-red-600" />;
      case 'Running':
        return (
          <div className="animate-spin">
            <ClockIcon className="h-6 w-6 text-blue-600" />
          </div>
        );
      default:
        return <ClockIcon className="h-6 w-6 text-gray-500" />;
    }
  };

  const getStepClass = (state: string) => {
    switch (state) {
      case 'Completed':
        return 'border-2 border-green-200 bg-green-50 dark:bg-green-900/20 dark:border-green-800';
      case 'Failed':
        return 'border-2 border-red-200 bg-red-50 dark:bg-red-900/20 dark:border-red-800';
      case 'Running':
        return 'border-2 border-blue-200 bg-blue-50 dark:bg-blue-900/20 dark:border-blue-800';
      default:
        return 'border-2 border-gray-200 bg-gray-50 dark:bg-gray-800 dark:border-gray-700';
    }
  };

  return (
    <div className="space-y-6 p-4 bg-white dark:bg-gray-900 rounded-lg shadow-sm">
      <div className="flex justify-between items-center border-b border-gray-200 dark:border-gray-700 pb-4">
        <div className="flex items-center space-x-4">
          <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Pipeline Execution</h3>
          <div className="text-base text-gray-600 dark:text-gray-400">
            Pipeline ID: <span className="font-mono">{executionContext.pipelineId}</span>
          </div>
        </div>
        {executionContext.status === 'Running' && onCancel && (
          <button
            onClick={onCancel}
            className="px-4 py-2 text-base font-medium text-red-600 dark:text-red-400 border-2 border-red-300 dark:border-red-700 rounded-md hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors"
          >
            Cancel Execution
          </button>
        )}
      </div>

      <div className="space-y-4">
        {stepIds.map(stepId => {
          const stepResult = stepResults[stepId];
          const stepLogs = stepResult.logs || [];
          const status = stepResult.status;

          return (
            <div
              key={stepId}
              className={`${getStepClass(status)} rounded-lg shadow-sm transition-all duration-200 hover:shadow-md cursor-pointer`}
              onClick={() => setSelectedStep(stepId)}
            >
              <div className="flex items-center justify-between p-4">
                <div className="flex items-center space-x-3">
                  {getStepIcon(status)}
                  <span className="text-lg font-medium text-gray-900 dark:text-white">Step {stepId}</span>
                </div>
                <div className="text-base text-gray-600 dark:text-gray-400">
                  {stepLogs.length} log {stepLogs.length === 1 ? 'entry' : 'entries'}
                </div>
              </div>
            </div>
          );
        })}
      </div>

      <div className={`text-base font-medium ${
        executionContext.status === 'Completed' ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'
      }`}>
        Execution {executionContext.status.toLowerCase()}
        {(() => {
          const duration = getDuration();
          if (duration === null) return null;
          return <> in <span className="font-mono">{duration}s</span></>;
        })()}
      </div>

      {selectedStep && (
        <StepDetailsModal
          stepId={selectedStep}
          stepResult={stepResults[selectedStep]}
          onClose={() => setSelectedStep(null)}
        />
      )}
    </div>
  );
} 