import React, { useState } from 'react';
import { Box } from '@mui/material';
import { Outlet } from 'react-router-dom';
import { AppNavbar, AppSidebar, DRAWER_WIDTH } from '../components/navigation';
import { PageContainer } from '../components/shared';

const MainLayout: React.FC = () => {
  const [mobileOpen, setMobileOpen] = useState(false);

  const handleMenuOpen = () => setMobileOpen(true);
  const handleMenuClose = () => setMobileOpen(false);

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        bgcolor: 'background.default',
      }}
    >
      <AppSidebar
        mobileOpen={mobileOpen}
        onCloseMobile={handleMenuClose}
      />

      <Box
        component="main"
        sx={{
          flex: 1,
          ml: {
            xs: 0,
            lg: `${DRAWER_WIDTH}px`,
          },
          p: {
            xs: 2,
            md: 3,
          },
        }}
      >
        <PageContainer>
          <AppNavbar onMenuClick={handleMenuOpen} />
          <Box sx={{ mt: 2.5 }}>
            <Outlet />
          </Box>
        </PageContainer>
      </Box>
    </Box>
  );
};

export default MainLayout;
