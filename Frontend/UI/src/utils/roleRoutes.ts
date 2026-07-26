import { ROUTES } from '../routes/paths';
import { normalizeRole } from './roles';

export const getRoleRedirectPath = (role: string | null): string => {
  switch (normalizeRole(role)) {
    case 'Administrator':
      return ROUTES.adminDashboard;

    case 'ProjectManager':
      return ROUTES.pmRoleDashboard;

    case 'ITAdmin':
      return ROUTES.itAdminDashboard;

    default:
      return ROUTES.home;
  }
};
