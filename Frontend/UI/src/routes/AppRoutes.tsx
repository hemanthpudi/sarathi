import React, { Suspense, lazy } from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { Box, CircularProgress } from '@mui/material';
import { useAuth } from '../hooks/useAuth';
import ProtectedRoute from '../components/ProtectedRoute';
import RoleProtectedRoute from '../components/RoleProtectedRoute';
import MainLayout from '../layouts/MainLayout';
import AdministratorLayout from '../layouts/AdministratorLayout';
import ProjectManagerLayout from '../layouts/ProjectManagerLayout';
import ITAdminLayout from '../layouts/ITAdminLayout';
import { getRoleRedirectPath } from '../utils/roleRoutes';
import { ROUTES } from './paths';

const LoginPage = lazy(() => import('../pages/LoginPage'));
const UnauthorizedPage = lazy(() => import('../pages/UnauthorizedPage'));
const HomePage = lazy(() => import('../pages/HomePage'));
const RoleDashboardPage = lazy(() => import('../pages/RoleDashboardPage'));
const NotificationsPage = lazy(() => import('../pages/NotificationsPage'));
const ReportsPage = lazy(() => import('../pages/ReportsPage'));

const RouteLoadingFallback: React.FC = () => (
  <Box
    display="flex"
    justifyContent="center"
    alignItems="center"
    minHeight="100vh"
  >
    <CircularProgress />
  </Box>
);

const withSuspense = (node: React.ReactNode) => (
  <Suspense fallback={<RouteLoadingFallback />}>
    {node}
  </Suspense>
);

const RoleDefaultRedirect: React.FC = () => {
  const { error, isAuthenticated, isLoading, role } = useAuth();

  if (isLoading) {
    return (
      <Box
        display="flex"
        justifyContent="center"
        alignItems="center"
        minHeight="100vh"
      >
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return <Navigate to={`${ROUTES.login}?message=unauthorized`} replace />;
  }

  if (!isAuthenticated) {
    return <Navigate to={ROUTES.login} replace />;
  }

  return <Navigate to={getRoleRedirectPath(role)} replace />;
};

const AppRoutes: React.FC = () => {
  return (
    <Routes>
      {/* Public routes */}
      <Route path={ROUTES.login} element={withSuspense(<LoginPage />)} />
      <Route path={ROUTES.unauthorized} element={withSuspense(<UnauthorizedPage />)} />

      {/* Root redirect */}
      <Route path="/" element={<RoleDefaultRedirect />} />

      {/* Authenticated home */}
      <Route element={<ProtectedRoute />}>
        <Route element={<MainLayout />}>
          <Route path={ROUTES.home} element={withSuspense(<HomePage />)} />
          <Route path={ROUTES.notifications} element={withSuspense(<NotificationsPage />)} />
          <Route path={ROUTES.reports} element={withSuspense(<ReportsPage />)} />
        </Route>
      </Route>

      {/* Administrator routes */}
      <Route element={<RoleProtectedRoute allowedRoles={['Administrator']} />}>
        <Route element={<AdministratorLayout />}>
          <Route path={ROUTES.adminDashboard} element={withSuspense(<RoleDashboardPage />)} />
        </Route>
      </Route>

      {/* ProjectManager routes */}
      <Route element={<RoleProtectedRoute allowedRoles={['ProjectManager']} />}>
        <Route element={<ProjectManagerLayout />}>
          <Route path={ROUTES.pmRoleDashboard} element={withSuspense(<RoleDashboardPage />)} />
        </Route>
      </Route>

      {/* ITAdmin routes */}
      <Route element={<RoleProtectedRoute allowedRoles={['ITAdmin']} />}>
        <Route element={<ITAdminLayout />}>
          <Route path={ROUTES.itAdminDashboard} element={withSuspense(<RoleDashboardPage />)} />
        </Route>
      </Route>

      {/* Fallback */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
};

export default AppRoutes;
