import React, { useEffect, useState } from "react";
import { Navigate, useLocation } from "react-router-dom";
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Paper,
  Stack,
  Typography,
} from "@mui/material";
import { LockRounded } from "@mui/icons-material";
import { useAuth } from "../hooks/useAuth";
import { getRoleRedirectPath } from "../utils/roleRoutes";

const MicrosoftLogo: React.FC = () => (
  <svg
    xmlns="http://www.w3.org/2000/svg"
    viewBox="0 0 21 21"
    width="20"
    height="20"
  >
    <rect x="1" y="1" width="9" height="9" fill="#f25022" />
    <rect x="11" y="1" width="9" height="9" fill="#7fba00" />
    <rect x="1" y="11" width="9" height="9" fill="#00a4ef" />
    <rect x="11" y="11" width="9" height="9" fill="#ffb900" />
  </svg>
);

const LoginPage: React.FC = () => {
  const {
    isAuthenticated,
    isLoading,
    login,
    error,
    clearError,
    role,
  } = useAuth();

  const location = useLocation();

  const [sessionError, setSessionError] = useState<string | null>(null);

  const routeMessage =
    (location.state as { message?: string } | null)?.message ?? null;

  useEffect(() => {
    const handler = () => {
      setSessionError("Your session has expired. Please sign in again.");
    };

    window.addEventListener("auth:unauthorized", handler);

    return () =>
      window.removeEventListener("auth:unauthorized", handler);
  }, []);

  if (isAuthenticated) {
    return <Navigate to={getRoleRedirectPath(role)} replace />;
  }

  if (isLoading) {
    return (
      <Box
        sx={{
          minHeight: "100vh",
          display: "flex",
          justifyContent: "center",
          alignItems: "center",
          flexDirection: "column",
          gap: 2,
          bgcolor: "#f5f7fb",
        }}
      >
        <CircularProgress size={48} />

        <Typography color="text.secondary">
          Authenticating...
        </Typography>
      </Box>
    );
  }

  const alertMessage = routeMessage || error || sessionError;

  return (
    <Box
      sx={{
        minHeight: "100vh",
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        px: 2,
        background:
          "linear-gradient(135deg,#f4f7fb 0%,#eef6f8 60%,#ffffff 100%)",
      }}
    >
      <Paper
        elevation={0}
        sx={{
          width: "100%",
          maxWidth: 480,
          p: 5,
          borderRadius: 3,
          border: "1px solid rgba(8,45,89,.10)",
          boxShadow: "0 18px 60px rgba(8,45,89,.15)",
          textAlign: "center",
        }}
      >
        <Stack spacing={3} alignItems="center">
          <Box
            component="img"
            src="/sarathi-logo.jpeg"
            alt="Sarathi AI"
            sx={{
              width: 150,
              objectFit: "contain",
            }}
          />

          <Box>
            <Typography
              variant="h4"
              fontWeight={700}
              color="primary.dark"
            >
              Sarathi AI
            </Typography>

            <Typography
              mt={1}
              color="text.secondary"
              lineHeight={1.7}
            >
              AI-powered Delivery Governance Platform
            </Typography>
          </Box>

          <Stack
            direction="row"
            spacing={1}
            alignItems="center"
          >
            <LockRounded color="primary" />

            <Typography
              variant="body2"
              fontWeight={600}
            >
              Secured with Microsoft Entra ID
            </Typography>
          </Stack>

          {alertMessage && (
            <Alert
              severity="error"
              sx={{
                width: "100%",
                textAlign: "left",
              }}
              onClose={() => {
                clearError();
                setSessionError(null);
              }}
            >
              {alertMessage}
            </Alert>
          )}

          <Button
            fullWidth
            variant="contained"
            size="large"
            onClick={login}
            sx={{
              py: 1.5,
              fontWeight: 600,
              bgcolor: "#082d59",
              "&:hover": {
                bgcolor: "#041f3f",
              },
            }}
            startIcon={<MicrosoftLogo />}
          >
            Sign in with Microsoft
          </Button>
        </Stack>
      </Paper>
    </Box>
  );
};

export default LoginPage;