import { createTheme } from '@mui/material/styles';
import { appColors } from './tokens';

const theme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: appColors.brandBlue,
      light: '#4f7af7',
      dark: appColors.brandDeep,
    },
    secondary: {
      main: appColors.brandViolet,
    },
    success: {
      main: '#78a91f',
    },
    warning: {
      main: '#f39a19',
    },
    error: {
      main: '#e14d3a',
    },
    background: {
      default: appColors.pageBg,
      paper: appColors.surface,
    },
    text: {
      primary: appColors.textPrimary,
      secondary: appColors.textSecondary,
    },
    divider: appColors.border,
  },
  typography: {
    fontFamily: 'Inter, Segoe UI, Roboto, Arial, sans-serif',
    h1: {
      fontWeight: 800,
      letterSpacing: 0,
    },
    h4: {
      fontWeight: 800,
      letterSpacing: 0,
    },
    h5: {
      fontWeight: 700,
      letterSpacing: 0,
    },
    button: {
      fontWeight: 700,
      textTransform: 'none',
    },
  },
  shape: {
    borderRadius: 10,
  },
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: 10,
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          borderRadius: 10,
        },
      },
    },
  },
});

export default theme;
