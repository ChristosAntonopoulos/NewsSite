import { TransformationConfig } from '../../../types/Pipeline';
import { TextField } from '../../../shared/components/common/TextField';
import { TransformationEditor } from './TransformationEditor';

interface OpenAIStepEditorProps {
  parameters: Record<string, string>;
  transformations: TransformationConfig[];
  onParametersChange: (parameters: Record<string, string>) => void;
  onTransformationsChange: (transformations: TransformationConfig[]) => void;
}

export function OpenAIStepEditor({
  parameters,
  transformations,
  onParametersChange,
  onTransformationsChange,
}: OpenAIStepEditorProps) {
  const handleChange = (key: string, value: string) => {
    onParametersChange({ ...parameters, [key]: value });
  };

  return (
    <div className="space-y-4">
      <TextField
        label="Prompt"
        value={parameters.prompt || ''}
        onChange={(e) => handleChange('prompt', e.target.value)}
        multiline
        rows={4}
        placeholder="Enter your prompt here. Use ${variableName} to reference values from previous steps"
        required
      />

      <TextField
        label="Model"
        value={parameters.model || 'gpt-3.5-turbo'}
        onChange={(e) => handleChange('model', e.target.value)}
        placeholder="Enter model name (e.g., gpt-3.5-turbo)"
      />

      <div className="grid grid-cols-2 gap-4">
        <TextField
          label="Max Tokens"
          type="number"
          value={parameters.maxTokens || '150'}
          onChange={(e) => handleChange('maxTokens', e.target.value)}
          placeholder="Maximum number of tokens"
        />

        <TextField
          label="Temperature"
          type="number"
          step="0.1"
          min="0"
          max="1"
          value={parameters.temperature || '0.7'}
          onChange={(e) => handleChange('temperature', e.target.value)}
          placeholder="Temperature (0-1)"
        />
      </div>

      <TextField
        label="Output Path"
        value={parameters.outputPath || ''}
        onChange={(e) => handleChange('outputPath', e.target.value)}
        placeholder="Path to store the output (e.g., result.text)"
        required
      />

      <TransformationEditor
        transformations={transformations}
        onChange={onTransformationsChange}
        availableFields={['text', 'completion', 'tokens', 'finish_reason']}
      />
    </div>
  );
} 