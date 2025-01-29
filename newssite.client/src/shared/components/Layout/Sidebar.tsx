import { Link } from 'react-router-dom';
import { Box, List, ListItem, ListItemIcon, ListItemText, Divider } from '@mui/material';
import { useTheme } from '@mui/material/styles';
import { getEditorStyles } from '../../../shared/theme/editorTheme';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import StorageIcon from '@mui/icons-material/Storage';
import SettingsIcon from '@mui/icons-material/Settings';

export const Sidebar = () => {
  const theme = useTheme();
  const styles = getEditorStyles(theme);

  return (
    <Box sx={{ width: 240, bgcolor: 'background.paper' }}>
      <List>
        <ListItem button component={Link} to="/pipelines">
          <ListItemIcon>
            <PlayArrowIcon />
          </ListItemIcon>
          <ListItemText primary="Pipelines" />
        </ListItem>
        <ListItem button component={Link} to="/collections">
          <ListItemIcon>
            <StorageIcon />
          </ListItemIcon>
          <ListItemText primary="Collections" />
        </ListItem>
      </List>
      <Divider sx={styles.divider} />
      <List>
        <ListItem button component={Link} to="/settings">
          <ListItemIcon>
            <SettingsIcon />
          </ListItemIcon>
          <ListItemText primary="Settings" />
        </ListItem>
      </List>
    </Box>
  );
}; 