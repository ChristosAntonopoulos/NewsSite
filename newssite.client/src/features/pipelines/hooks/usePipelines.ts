import { useState, useEffect } from 'react';
import { Pipeline } from '../../../types/Pipeline';
import { pipelineService } from '../services/pipelineService';

export const usePipelines = () => {
  const [pipelines, setPipelines] = useState<Pipeline[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadPipelines = async () => {
      try {
        const data = await pipelineService.getPipelines();
        setPipelines(data);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to fetch pipelines');
      } finally {
        setLoading(false);
      }
    };

    loadPipelines();
  }, []);

  return { pipelines, loading, error };
}; 