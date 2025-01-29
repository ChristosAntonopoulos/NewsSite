export interface PipelineStep {
    id: string;
    name: string;
    type: string;
    config: Record<string, any>;
    order: number;
}

export interface Pipeline {
    id: string;
    name: string;
    description: string;
    steps: PipelineStep[];
    isActive: boolean;
    createdAt: string;
    updatedAt: string;
}

export interface PipelineState {
    pipelines: Pipeline[];
    loading: boolean;
    error: string | null;
} 