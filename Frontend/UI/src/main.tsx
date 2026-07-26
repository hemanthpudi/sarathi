import ReactDOM from 'react-dom/client';
import React, { useEffect } from 'react';
import { MsalProvider } from '@azure/msal-react';
import { msalInstance } from './config/msalConfig';
import { BrowserRouter, useLocation } from 'react-router-dom';
import { ThemeProvider, CssBaseline } from '@mui/material';
import theme from './theme';
import { AuthProvider } from './contexts/AuthContext';
import { NotificationProvider } from './contexts/NotificationContext';
import App from './App';

const RuntimePatches: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const location = useLocation();
  useEffect(() => {
    try {
      if (location.pathname === '/portfolio') {
        // Find the heading then the KPI grid container and force 2 columns (2x4)
        const headings = Array.from(document.querySelectorAll('h2')) as HTMLHeadingElement[];
        const heading = headings.find(h => h.textContent && h.textContent.trim() === 'Portfolio Dashboard');
        if (heading) {
          const container = heading.closest('main')?.querySelector('div.grid');
          if (container && container instanceof HTMLElement) {
            container.style.setProperty('display', 'grid');
            container.style.setProperty('grid-template-columns', 'repeat(2, minmax(0, 1fr))');
            container.style.setProperty('gap', '12px');
          }
        }
      }
    } catch (e) {
      // ignore
    }
  }, [location.pathname]);

  return <>{children}</>;
};

ReactDOM.createRoot(document.getElementById('root')!).render(
  <MsalProvider instance={msalInstance}>
    <BrowserRouter>
      <RuntimePatches>
        <ThemeProvider theme={theme}>
          <CssBaseline />
          <AuthProvider>
            <NotificationProvider>
              <App />
            </NotificationProvider>
          </AuthProvider>
        </ThemeProvider>
      </RuntimePatches>
    </BrowserRouter>
  </MsalProvider>
);
