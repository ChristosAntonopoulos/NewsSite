import React, { useEffect, useState } from 'react';
import { 
  Box, 
  FormControl, 
  InputLabel, 
  MenuItem, 
  Select, 
  TextField, 
  Typography, 
  Paper, 
  IconButton, 
  SelectChangeEvent,
  Tooltip,
  Stack,
  Button,
  Divider
} from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import AddIcon from '@mui/icons-material/Add';
import ArrowRightAltIcon from '@mui/icons-material/ArrowRightAlt';
import { useTheme } from '@mui/material/styles';
import { getEditorStyles } from '../../../shared/theme/editorTheme';
import { TransformationConfig } from '../../../types/Pipeline';
import { TransformationEditor } from './TransformationEditor';

interface DatabaseStepEditorProps {
  parameters: Record<string, string>;
  transformations: TransformationConfig[];
  onParametersChange: (parameters: Record<string, string>) => void;
  onTransformationsChange: (transformations: TransformationConfig[]) => void;
}

interface SchemaField {
  name: string;
  type: string;
}

interface Mapping {
  input: string;
  output: string;
}

export function DatabaseStepEditor({
  parameters,
  transformations,
  onParametersChange,
  onTransformationsChange
}: DatabaseStepEditorProps) {
  const theme = useTheme();
  const styles = getEditorStyles(theme);
  const [collections, setCollections] = useState<string[]>([]);
  const [collectionSchema, setCollectionSchema] = useState<SchemaField[]>([]);
  const [mappings, setMappings] = useState<Mapping[]>([]);

  useEffect(() => {
    fetchCollections();
  }, []);

  useEffect(() => {
    if (parameters.collection) {
      fetchCollectionSchema(parameters.collection);
    }
  }, [parameters.collection]);

  useEffect(() => {
    if (parameters.schemaMapping) {
      try {
        const parsed = JSON.parse(parameters.schemaMapping);
        const mappingArray = Object.entries(parsed).map(([input, output]) => ({
          input,
          output: output as string,
        }));
        setMappings(mappingArray);
      } catch {
        setMappings([]);
      }
    }
  }, [parameters.schemaMapping]);

  const fetchCollections = async () => {
    try {
      const response = await fetch('/api/database/collections');
      const data = await response.json();
      setCollections(data);
    } catch (error) {
      console.error('Failed to fetch collections:', error);
    }
  };

  const fetchCollectionSchema = async (collection: string) => {
    try {
      const response = await fetch(`/api/database/collections/${collection}/schema`);
      const schema = await response.json();
      const fields = Object.entries(schema).map(([name, value]) => ({
        name,
        type: typeof value,
      }));
      setCollectionSchema(fields);
    } catch (error) {
      console.error('Failed to fetch schema:', error);
    }
  };

  const handleChange = (field: string, value: string) => {
    onParametersChange({
      ...parameters,
      [field]: value
    });
  };

  const handleOperationChange = (event: SelectChangeEvent<string>) => {
    onParametersChange({
      ...parameters,
      operation: event.target.value,
    });
  };

  const handleCollectionChange = (event: SelectChangeEvent<string>) => {
    onParametersChange({
      ...parameters,
      collection: event.target.value,
    });
  };

  const handleMappingChange = (index: number, field: keyof Mapping, value: string) => {
    const newMappings = [...mappings];
    newMappings[index][field] = value;
    
    // Convert mappings to schema mapping format
    const schemaMapping = newMappings.reduce((acc, { input, output }) => {
      if (input && output) {
        acc[input] = output;
      }
      return acc;
    }, {} as Record<string, string>);

    onParametersChange({
      ...parameters,
      schemaMapping: JSON.stringify(schemaMapping),
    });
  };

  const addMapping = () => {
    setMappings([...mappings, { input: '', output: '' }]);
  };

  const removeMapping = (index: number) => {
    const newMappings = mappings.filter((_, i) => i !== index);
    setMappings(newMappings);
  };

  const getOperationDescription = (operation: string) => {
    switch (operation) {
      case 'query': return 'Search for documents in the collection';
      case 'insert': return 'Add new documents to the collection';
      case 'update': return 'Modify existing documents in the collection';
      case 'delete': return 'Remove documents from the collection';
      default: return '';
    }
  };

  // Available fields for transformations
  const availableFields = [
    'id',
    'title',
    'content',
    'url',
    'source',
    'publishedAt',
    'author',
    'category'
  ];

  return (
    <Paper className="p-4 space-y-4">
      <Typography variant="h6" className="mb-4">
        Database Operation
      </Typography>

      <FormControl fullWidth>
        <InputLabel>Operation</InputLabel>
        <Select
          value={parameters.operation || 'query'}
          label="Operation"
          onChange={(e) => handleChange('operation', e.target.value)}
        >
          <MenuItem value="query">Query</MenuItem>
          <MenuItem value="insert">Insert</MenuItem>
          <MenuItem value="update">Update</MenuItem>
          <MenuItem value="delete">Delete</MenuItem>
        </Select>
      </FormControl>

      <TextField
        fullWidth
        label="Collection"
        value={parameters.collection || ''}
        onChange={(e) => handleChange('collection', e.target.value)}
        placeholder="e.g., articles"
      />

      <TextField
        fullWidth
        multiline
        rows={4}
        label="Query"
        value={parameters.query || ''}
        onChange={(e) => handleChange('query', e.target.value)}
        placeholder="Enter MongoDB query with ${context.var} or ${input.var} variables"
      />

      <TextField
        fullWidth
        label="Output Path"
        value={parameters.outputPath || ''}
        onChange={(e) => handleChange('outputPath', e.target.value)}
        placeholder="e.g., result.articles"
        helperText="Path to store the operation results in the pipeline context"
      />

      <Divider className="my-6" />

      <TransformationEditor
        transformations={transformations}
        onChange={onTransformationsChange}
        availableFields={availableFields}
      />
    </Paper>
  );
} 