import React from "react";
import { Navigate, Outlet, useLocation } from "react-router-dom";
import {
  Box,
  CircularProgress,
  Typography,
} from "@mui/material";
import { useAuth } from "../hooks/useAuth";

const ProtectedRoute: React.FC = () => {
  const { isAuthenticated, isLoading } = useAuth();
  const location = useLocation();

  if (isLoading) {
    return (
      <Box
        sx={{
          minHeight: "100vh",
          display: "flex",
          flexDirection: "column",
          justifyContent: "center",
          alignItems: "center",
          gap: 2,
          bgcolor: "#f5f7fb",
        }}
      >
        <CircularProgress size={48} />

        <Typography
          variant="body1"
          color="text.secondary"
        >
          Authenticating...
        </Typography>
      </Box>
    );
  }

  if (!isAuthenticated) {
    return (
      <Navigate
        to="/login"
        replace
        state={{
          from: location,
          message: "Unauthorized. Please sign in with an authorized Sarathi account.",
        }}
      />
    );
  }

  return <Outlet />;
};

export default ProtectedRoute;