import React from 'react';
import { Box } from '@mui/material';

interface PageContainerProps {
  children: React.ReactNode;
}

const PageContainer: React.FC<PageContainerProps> = ({ children }) => {
  return (
    <Box
      sx={{
        width: '100%',
        maxWidth: 1240,
        mx: 'auto',
      }}
    >
      {children}
    </Box>
  );
};

export default PageContainer;
