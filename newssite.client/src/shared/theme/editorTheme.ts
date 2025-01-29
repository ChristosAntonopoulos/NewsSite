import { Theme } from '@mui/material/styles';

export const editorColors = {
  light: {
    background: '#ffffff',
    paper: '#f8f9fa',
    surface: '#ffffff',
    border: '#e0e0e0',
    divider: '#f0f0f0',
    text: {
      primary: '#2d3748',
      secondary: '#718096',
      hint: '#a0aec0',
    },
    input: {
      background: '#ffffff',
      border: '#e2e8f0',
      hoverBorder: '#cbd5e0',
      focusBorder: '#3182ce',
    },
    primary: {
      main: '#3182ce',
      light: '#63b3ed',
      dark: '#2c5282',
      contrastText: '#ffffff',
    },
    error: {
      main: '#e53e3e',
      light: '#fc8181',
      dark: '#c53030',
      contrastText: '#ffffff',
    },
  },
  dark: {
    background: '#1a202c',
    paper: '#2d3748',
    surface: '#2d3748',
    border: '#4a5568',
    divider: '#4a5568',
    text: {
      primary: '#f7fafc',
      secondary: '#e2e8f0',
      hint: '#a0aec0',
    },
    input: {
      background: '#2d3748',
      border: '#4a5568',
      hoverBorder: '#718096',
      focusBorder: '#63b3ed',
    },
    primary: {
      main: '#63b3ed',
      light: '#90cdf4',
      dark: '#3182ce',
      contrastText: '#1a202c',
    },
    error: {
      main: '#fc8181',
      light: '#fed7d7',
      dark: '#e53e3e',
      contrastText: '#1a202c',
    },
  },
};

export const getEditorStyles = (theme: Theme) => {
  const colors = editorColors[theme.palette.mode];

  return {
    container: {
      backgroundColor: colors.background,
      borderRadius: '8px',
      padding: '20px',
    },
    section: {
      backgroundColor: colors.surface,
      borderRadius: '6px',
      padding: '16px',
      border: `1px solid ${colors.border}`,
      '&:hover': {
        borderColor: colors.input.hoverBorder,
      },
    },
    input: {
      backgroundColor: colors.input.background,
      color: colors.text.primary,
      '& .MuiInputBase-input': {
        color: colors.text.primary,
      },
      '& .MuiInputLabel-root': {
        color: colors.text.secondary,
      },
      '& .MuiOutlinedInput-notchedOutline': {
        borderColor: colors.input.border,
      },
      '&:hover .MuiOutlinedInput-notchedOutline': {
        borderColor: colors.input.hoverBorder,
      },
      '&.Mui-focused .MuiOutlinedInput-notchedOutline': {
        borderColor: colors.input.focusBorder,
      },
      '& .MuiSelect-icon': {
        color: colors.text.secondary,
      },
    },
    menuItem: {
      color: colors.text.primary,
      backgroundColor: colors.surface,
      '&:hover': {
        backgroundColor: colors.paper,
      },
      '&.Mui-selected': {
        backgroundColor: `${colors.primary.main}1A`,
        '&:hover': {
          backgroundColor: `${colors.primary.main}33`,
        },
      },
    },
    paper: {
      backgroundColor: colors.surface,
      border: `1px solid ${colors.border}`,
      color: colors.text.primary,
      borderRadius: '6px',
      padding: '16px',
      '&:hover': {
        borderColor: colors.input.hoverBorder,
      },
    },
    addButton: {
      borderStyle: 'dashed',
      color: colors.primary.main,
      borderColor: 'currentColor',
      borderRadius: '6px',
      padding: '8px 16px',
      '&:hover': {
        borderStyle: 'dashed',
        backgroundColor: `${colors.primary.main}1A`,
      },
    },
    deleteButton: {
      color: colors.error.main,
      padding: '8px',
      '&:hover': {
        backgroundColor: `${colors.error.main}1A`,
      },
    },
    title: {
      color: colors.text.primary,
      fontWeight: 600,
      fontSize: '1.125rem',
    },
    subtitle: {
      color: colors.text.secondary,
      fontSize: '0.875rem',
    },
    hint: {
      color: colors.text.hint,
      fontSize: '0.75rem',
      marginTop: '4px',
    },
    icon: {
      color: colors.text.secondary,
    },
    divider: {
      backgroundColor: colors.divider,
      margin: '16px 0',
    },
    label: {
      color: colors.text.secondary,
      fontSize: '0.875rem',
      fontWeight: 500,
      marginBottom: '8px',
    },
  };
}; 