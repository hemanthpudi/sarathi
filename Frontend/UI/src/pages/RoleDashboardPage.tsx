import React, { Suspense, lazy } from 'react';
import '../reference-ui.css';
import { useAuth } from '../hooks/useAuth';
import { normalizeRole } from '../utils/roles';

type EmbeddedDashboardRole = 'Administrator' | 'Project Manager' | 'IT Manager';

const ZipDashboard = lazy(() => import('../reference-ui/App'));

const RoleDashboardPage: React.FC = () => {
  const { logout, role, user } = useAuth();
  const dashboardRole: EmbeddedDashboardRole = normalizeRole(role) === 'ProjectManager'
    ? 'Project Manager'
    : normalizeRole(role) === 'ITAdmin'
      ? 'IT Manager'
      : 'Administrator';

  return (
    <Suspense fallback={<div className="p-6 text-sm text-slate-500">Loading dashboard...</div>}>
      <ZipDashboard
        embeddedRole={dashboardRole}
        embeddedEmail={user?.email ?? ''}
        onEmbeddedLogout={() => void logout()}
      />
    </Suspense>
  );
};

export default RoleDashboardPage;
