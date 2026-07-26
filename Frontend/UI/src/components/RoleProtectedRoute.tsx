import React from "react";
import { Navigate, Outlet, useLocation } from "react-router-dom";
import {
  Box,
  CircularProgress,
  Typography,
} from "@mui/material";
import { useAuth } from "../hooks/useAuth";
import { normalizeRole } from "../utils/roles";

interface RoleProtectedRouteProps {
  allowedRoles: string[];
}

const RoleProtectedRoute: React.FC<RoleProtectedRouteProps> = ({
  allowedRoles,
}) => {
  const { isAuthenticated, isLoading, role } = useAuth();
  const location = useLocation();
  const normalizedRole = normalizeRole(role);
  const normalizedAllowedRoles = allowedRoles
    .map((item) => normalizeRole(item))
    .filter((item): item is NonNullable<typeof item> => !!item);

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
        <Typography color="text.secondary">
          Verifying permissions...
        </Typography>
      </Box>
    );
  }

  // User is not logged in
  if (!isAuthenticated) {
    return (
      <Navigate
        to="/login"
        replace
        state={{
          from: location,
          message: "Please sign in to continue.",
        }}
      />
    );
  }

  // User is logged in but doesn't have permission
  if (!normalizedRole || !normalizedAllowedRoles.includes(normalizedRole)) {
    return (
      <Navigate
        to="/unauthorized"
        replace
        state={{
          message: "You do not have permission to access this page.",
        }}
      />
    );
  }

  return <Outlet />;
};

export default RoleProtectedRoute;