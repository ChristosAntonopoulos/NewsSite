import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { PlusIcon } from '@heroicons/react/24/outline';
import { Pipeline } from '../../../types/Pipeline';
import { PipelineCard } from '../components/PipelineCard';
import { LoadingSpinner } from '../../../shared/components/common/LoadingSpinner';
import { pipelineService } from '../services/pipelineService';

export default function PipelineList() {
  const [pipelines, setPipelines] = useState<Pipeline[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetchPipelines();
  }, []);

  const fetchPipelines = async () => {
    try {
      const data = await pipelineService.getPipelines();
      setPipelines(data);
    } catch (err) {
      setError('Failed to load pipelines');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (loading) return (
    <div className="flex justify-center items-center min-h-[200px]">
      <LoadingSpinner className="w-8 h-8 text-cyan-600" />
    </div>
  );
  if (error) return <div className="text-red-500 text-center">{error}</div>;

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="flex justify-between items-center mb-8">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Pipelines</h1>
        <Link
          to="/pipelines/create"
          className="inline-flex items-center px-4 py-2 bg-cyan-600 hover:bg-cyan-700 text-white rounded-md"
        >
          <PlusIcon className="h-5 w-5 mr-2" />
          Create Pipeline
        </Link>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {pipelines.map((pipeline) => (
          <PipelineCard key={pipeline.id} pipeline={pipeline} onUpdate={fetchPipelines} />
        ))}
      </div>

      {pipelines.length === 0 && (
        <div className="text-center py-12">
          <p className="text-gray-500 dark:text-gray-400">No pipelines found. Create your first pipeline!</p>
        </div>
      )}
    </div>
  );
} 