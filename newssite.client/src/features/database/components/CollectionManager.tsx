import React, { useState } from 'react';
import {
  Box,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Typography,
  Stack,
  IconButton,
  Paper,
  Divider,
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import DeleteIcon from '@mui/icons-material/Delete';
import EditIcon from '@mui/icons-material/Edit';
import { useTheme } from '@mui/material/styles';
import { getEditorStyles } from '../../../shared/theme/editorTheme';

interface CollectionField {
  name: string;
  type: string;
  required?: boolean;
}

interface Collection {
  name: string;
  fields: CollectionField[];
}

export const CollectionManager: React.FC = () => {
  const theme = useTheme();
  const styles = getEditorStyles(theme);
  const [collections, setCollections] = useState<Collection[]>([]);
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [currentCollection, setCurrentCollection] = useState<Collection | null>(null);
  const [isEditing, setIsEditing] = useState(false);

  const handleAddCollection = () => {
    setCurrentCollection({ name: '', fields: [] });
    setIsEditing(false);
    setIsDialogOpen(true);
  };

  const handleEditCollection = (collection: Collection) => {
    setCurrentCollection({ ...collection });
    setIsEditing(true);
    setIsDialogOpen(true);
  };

  const handleDeleteCollection = (name: string) => {
    setCollections(collections.filter(c => c.name !== name));
  };

  const handleAddField = () => {
    if (currentCollection) {
      setCurrentCollection({
        ...currentCollection,
        fields: [...currentCollection.fields, { name: '', type: 'string', required: false }]
      });
    }
  };

  const handleFieldChange = (index: number, field: Partial<CollectionField>) => {
    if (currentCollection) {
      const newFields = [...currentCollection.fields];
      newFields[index] = { ...newFields[index], ...field };
      setCurrentCollection({ ...currentCollection, fields: newFields });
    }
  };

  const handleDeleteField = (index: number) => {
    if (currentCollection) {
      const newFields = currentCollection.fields.filter((_, i) => i !== index);
      setCurrentCollection({ ...currentCollection, fields: newFields });
    }
  };

  const handleSave = () => {
    if (currentCollection) {
      if (isEditing) {
        setCollections(collections.map(c => 
          c.name === currentCollection.name ? currentCollection : c
        ));
      } else {
        setCollections([...collections, currentCollection]);
      }
      setIsDialogOpen(false);
    }
  };

  return (
    <Box sx={styles.container}>
      <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 3 }}>
        <Typography variant="h5" sx={styles.title}>
          Collections
        </Typography>
        <Button
          startIcon={<AddIcon />}
          variant="contained"
          onClick={handleAddCollection}
          sx={{ backgroundColor: theme.palette.primary.main }}
        >
          Add Collection
        </Button>
      </Stack>

      <List>
        {collections.map((collection) => (
          <React.Fragment key={collection.name}>
            <ListItem component={Paper} sx={styles.section}>
              <ListItemText
                primary={
                  <Typography variant="h6" sx={styles.title}>
                    {collection.name}
                  </Typography>
                }
                secondary={
                  <Typography variant="body2" sx={styles.subtitle}>
                    {collection.fields.length} fields
                  </Typography>
                }
              />
              <ListItemSecondaryAction>
                <IconButton
                  edge="end"
                  onClick={() => handleEditCollection(collection)}
                  sx={{ mr: 1 }}
                >
                  <EditIcon />
                </IconButton>
                <IconButton
                  edge="end"
                  onClick={() => handleDeleteCollection(collection.name)}
                  sx={styles.deleteButton}
                >
                  <DeleteIcon />
                </IconButton>
              </ListItemSecondaryAction>
            </ListItem>
            <Divider sx={styles.divider} />
          </React.Fragment>
        ))}
      </List>

      <Dialog 
        open={isDialogOpen} 
        onClose={() => setIsDialogOpen(false)}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>
          {isEditing ? 'Edit Collection' : 'New Collection'}
        </DialogTitle>
        <DialogContent>
          <Stack spacing={3} sx={{ mt: 2 }}>
            <TextField
              label="Collection Name"
              value={currentCollection?.name || ''}
              onChange={(e) => setCurrentCollection(curr => curr ? { ...curr, name: e.target.value } : null)}
              fullWidth
              sx={styles.input}
            />

            <Box>
              <Typography variant="h6" sx={styles.title}>Fields</Typography>
              <Stack spacing={2} sx={{ mt: 2 }}>
                {currentCollection?.fields.map((field, index) => (
                  <Paper key={index} sx={styles.paper}>
                    <Stack direction="row" spacing={2} alignItems="center">
                      <TextField
                        label="Field Name"
                        value={field.name}
                        onChange={(e) => handleFieldChange(index, { name: e.target.value })}
                        size="small"
                        sx={styles.input}
                      />
                      <TextField
                        select
                        label="Type"
                        value={field.type}
                        onChange={(e) => handleFieldChange(index, { type: e.target.value })}
                        size="small"
                        sx={styles.input}
                        SelectProps={{
                          native: true,
                        }}
                      >
                        {['string', 'number', 'boolean', 'date', 'object', 'array'].map((type) => (
                          <option key={type} value={type}>
                            {type}
                          </option>
                        ))}
                      </TextField>
                      <IconButton
                        onClick={() => handleDeleteField(index)}
                        sx={styles.deleteButton}
                      >
                        <DeleteIcon />
                      </IconButton>
                    </Stack>
                  </Paper>
                ))}
                <Button
                  startIcon={<AddIcon />}
                  onClick={handleAddField}
                  sx={styles.addButton}
                >
                  Add Field
                </Button>
              </Stack>
            </Box>
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setIsDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleSave} variant="contained">Save</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}; 