import React from 'react';
import { Box } from '@mui/material';
import { useAuth } from '../hooks/useAuth';
import { ModulePlaceholder, PageHeader } from '../components/shared';

const HomePage: React.FC = () => {
  const { role, user } = useAuth();

  return (
    <Box>
      <PageHeader
        title={`Welcome${user?.name ? `, ${user.name}` : ''}`}
        subtitle={`Signed in role: ${role ?? 'Unknown'}`}
      />
      <ModulePlaceholder description="Use the role dashboard from the sidebar to view your authorized workspace." />
    </Box>
  );
};

export default HomePage;
