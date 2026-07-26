import React from 'react';
import { Paper, Typography } from '@mui/material';

interface ModulePlaceholderProps {
  description: string;
}

const ModulePlaceholder: React.FC<ModulePlaceholderProps> = ({ description }) => {
  return (
    <Paper
      elevation={0}
      sx={{
        p: { xs: 3, md: 4 },
        borderRadius: 3,
        border: '1px solid',
        borderColor: 'divider',
      }}
    >
      <Typography color="text.secondary" lineHeight={1.8}>
        {description}
      </Typography>
    </Paper>
  );
};

export default ModulePlaceholder;
