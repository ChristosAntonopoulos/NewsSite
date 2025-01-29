import { ApiClient } from '../../../shared/services/api';
import { Pipeline, PipelineMode } from '../../../types/Pipeline';
import { API_CONFIG } from '../../../config/api';

// Map frontend mode strings to backend enum values
const PIPELINE_MODE_MAP = {
    'Sequential': 0,
    'Parallel': 1,
    'Conditional': 2
} as const;

// Map backend enum values to frontend mode strings
const PIPELINE_MODE_REVERSE_MAP = {
    0: 'Sequential',
    1: 'Parallel',
    2: 'Conditional'
} as const;

class PipelineService extends ApiClient {
    constructor() {
        super('');  // Using empty base URL since we'll use full paths from API_CONFIG
    }

    private convertPipelineForBackend(pipeline: Pipeline): any {
        return {
            ...pipeline,
            mode: PIPELINE_MODE_MAP[pipeline.mode as keyof typeof PIPELINE_MODE_MAP],
            steps: pipeline.steps.map(step => ({
                ...step,
                // Each step gets its own input/output configuration
                configuration: {
                    ...step.configuration,
                    outputPath: `step${step.order}_output`,
                    inputPath: step.order > 0 ? `step${step.order - 1}_output` : 'initial_input'
                }
            })),
            // Remove global schemas as they're now handled per step
            inputSchema: {},
            outputSchema: {}
        };
    }

    private convertPipelineFromBackend(pipeline: any): Pipeline {
        return {
            ...pipeline,
            mode: PIPELINE_MODE_REVERSE_MAP[pipeline.mode as keyof typeof PIPELINE_MODE_REVERSE_MAP] as PipelineMode,
            steps: (pipeline.steps || []).map((step: any, index: number) => ({
                ...step,
                order: index,
                configuration: {
                    ...step.configuration,
                    outputPath: step.configuration?.outputPath || `step${index}_output`,
                    inputPath: step.configuration?.inputPath || (index > 0 ? `step${index - 1}_output` : 'initial_input')
                }
            }))
        };
    }

    async getPipelines(): Promise<Pipeline[]> {
        const response = await this.get<any[]>(API_CONFIG.ENDPOINTS.PIPELINES);
        return response.map(p => this.convertPipelineFromBackend(p));
    }

    async getPipeline(id: string): Promise<Pipeline> {
        const response = await this.get<any>(API_CONFIG.ENDPOINTS.PIPELINE_BY_ID(id));
        return this.convertPipelineFromBackend(response);
    }

    async createPipeline(pipeline: Pipeline): Promise<Pipeline> {
        const convertedPipeline = this.convertPipelineForBackend(pipeline);
        const response = await this.post<any>(API_CONFIG.ENDPOINTS.PIPELINES, convertedPipeline);
        return this.convertPipelineFromBackend(response);
    }

    async updatePipeline(id: string, pipeline: Pipeline): Promise<Pipeline> {
        const convertedPipeline = this.convertPipelineForBackend(pipeline);
        const response = await this.put<any>(API_CONFIG.ENDPOINTS.PIPELINE_BY_ID(id), convertedPipeline);
        return this.convertPipelineFromBackend(response);
    }

    async deletePipeline(id: string): Promise<void> {
        return this.delete<void>(API_CONFIG.ENDPOINTS.PIPELINE_BY_ID(id));
    }

    async executePipeline(id: string): Promise<any> {
        const response = await this.post<any>(API_CONFIG.ENDPOINTS.PIPELINE_EXECUTE(id));
        return response;
    }

    async togglePipeline(id: string, enabled: boolean): Promise<Pipeline> {
        const response = await this.put<any>(API_CONFIG.ENDPOINTS.PIPELINE_TOGGLE(id), { enabled });
        return this.convertPipelineFromBackend(response);
    }

    async cancelPipeline(id: string): Promise<void> {
        return this.post<void>(`${API_CONFIG.ENDPOINTS.PIPELINE_BY_ID(id)}/cancel`);
    }
}

export const pipelineService = new PipelineService(); 