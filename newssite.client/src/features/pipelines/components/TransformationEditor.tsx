import React from 'react';
import { TransformationConfig, AVAILABLE_TRANSFORMATIONS } from '../../../types/Pipeline';
import { TextField } from '../../../shared/components/common/TextField';
import { Button } from '../../../shared/components/common/Button';
import { PlusIcon, TrashIcon } from '@heroicons/react/24/outline';
import {
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Switch,
  FormControlLabel,
  Typography,
  Tooltip,
  IconButton,
} from '@mui/material';

interface TransformationEditorProps {
  transformations: TransformationConfig[];
  onChange: (transformations: TransformationConfig[]) => void;
  availableFields: string[];
}

export function TransformationEditor({
  transformations = [],
  onChange,
  availableFields,
}: TransformationEditorProps) {
  const handleAdd = () => {
    const newTransformation: TransformationConfig = {
      type: AVAILABLE_TRANSFORMATIONS[0].value,
      field: availableFields[0] || '',
      transformation: '',
      applyToList: false,
      outputField: '',
      parameters: {}
    };
    onChange([...transformations, newTransformation]);
  };

  const handleRemove = (index: number) => {
    onChange(transformations.filter((_, i) => i !== index));
  };

  const handleUpdate = (index: number, updates: Partial<TransformationConfig>) => {
    onChange(
      transformations.map((t, i) =>
        i === index ? { ...t, ...updates } : t
      )
    );
  };

  return (
    <div className="space-y-4">
      <div className="flex justify-between items-center">
        <Typography variant="h6">Transformations</Typography>
        <Button
          onClick={handleAdd}
          variant="secondary"
          type="button"
          className="text-sm"
        >
          <PlusIcon className="h-4 w-4 mr-1" />
          Add Transformation
        </Button>
      </div>

      {transformations.map((t, index) => (
        <div
          key={index}
          className="bg-white dark:bg-gray-900 rounded-lg shadow-sm p-4 space-y-4 border border-gray-200 dark:border-gray-700"
        >
          <div className="flex justify-between items-start">
            <div className="flex-1 space-y-4">
              <FormControl fullWidth>
                <InputLabel>Field</InputLabel>
                <Select
                  value={t.field}
                  label="Field"
                  onChange={(e) =>
                    handleUpdate(index, { field: e.target.value })
                  }
                >
                  {availableFields.map((field) => (
                    <MenuItem key={field} value={field}>
                      {field}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>

              <FormControl fullWidth>
                <InputLabel>Transformation</InputLabel>
                <Select
                  value={t.transformation}
                  label="Transformation"
                  onChange={(e) =>
                    handleUpdate(index, { transformation: e.target.value })
                  }
                >
                  {AVAILABLE_TRANSFORMATIONS.map((transform) => (
                    <MenuItem key={transform.value} value={transform.value}>
                      <Tooltip title={transform.description} placement="right">
                        <span>{transform.label}</span>
                      </Tooltip>
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>

              <TextField
                label="Output Field"
                value={t.outputField}
                onChange={(e) =>
                  handleUpdate(index, { outputField: e.target.value })
                }
                placeholder="Leave empty to overwrite input field"
                className="w-full"
              />

              <FormControlLabel
                control={
                  <Switch
                    checked={t.applyToList}
                    onChange={(e) =>
                      handleUpdate(index, { applyToList: e.target.checked })
                    }
                  />
                }
                label="Apply to each item if input is a list"
              />
            </div>
            <IconButton
              onClick={() => handleRemove(index)}
              className="text-red-500 hover:text-red-700 dark:text-red-400 dark:hover:text-red-300"
            >
              <TrashIcon className="h-5 w-5" />
            </IconButton>
          </div>
        </div>
      ))}
    </div>
  );
} 