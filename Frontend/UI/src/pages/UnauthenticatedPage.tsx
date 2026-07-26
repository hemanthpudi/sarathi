import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Box, Button, Card, CardContent, Typography } from '@mui/material';
import { useAuth } from '../hooks/useAuth';

const UnauthenticatedPage: React.FC = () => {
  const navigate = useNavigate();
  const { clearError } = useAuth();

  const handleReturnToLogin = () => {
    clearError();
    navigate('/login', { replace: true });
  };

  return (
    <Box
      display="flex"
      justifyContent="center"
      alignItems="center"
      minHeight="100vh"
      bgcolor="grey.100"
      px={2}
    >
      <Card sx={{ maxWidth: 440, width: '100%', p: 2, boxShadow: 4 }}>
        <CardContent>
          <Typography variant="h4" fontWeight="bold" color="error" gutterBottom>
            I am unauthenticated.
          </Typography>
          <Typography variant="body1" color="text.secondary" mb={3}>
            Microsoft sign-in did not complete or your account is not registered in Sarathi AI.
          </Typography>
          <Button variant="contained" fullWidth onClick={handleReturnToLogin}>
            Return to sign in
          </Button>
        </CardContent>
      </Card>
    </Box>
  );
};

export default UnauthenticatedPage;
