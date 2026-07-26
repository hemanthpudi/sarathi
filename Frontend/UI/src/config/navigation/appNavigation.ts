import type { SvgIconComponent } from '@mui/icons-material';
import {
  AdminPanelSettingsRounded,
  DashboardRounded,
  MenuBookRounded,
  NotificationsRounded,
} from '@mui/icons-material';
import { ROUTES } from '../../routes/paths';
import { normalizeRole, type CanonicalRole } from '../../utils/roles';

export interface AppNavItem {
  label: string;
  path: string;
  icon: SvgIconComponent;
}

const sharedItems: AppNavItem[] = [
  {
    label: 'Home',
    path: ROUTES.home,
    icon: DashboardRounded,
  },
  {
    label: 'Reports',
    path: ROUTES.reports,
    icon: MenuBookRounded,
  },
  {
    label: 'Notifications',
    path: ROUTES.notifications,
    icon: NotificationsRounded,
  },
];

const roleItems: Record<CanonicalRole, AppNavItem[]> = {
  Administrator: [],
  ProjectManager: [],
  ITAdmin: [],
};

export const appBrand = {
  name: 'Sarathi AI',
  subtitle: 'Delivery Governance Platform',
};

export const getNavigationItems = (role: string | null): AppNavItem[] => {
  const normalizedRole = normalizeRole(role);
  const roleSpecific = normalizedRole ? roleItems[normalizedRole] : [];

  return [...sharedItems, ...roleSpecific];
};

export const roleBadgeIcon = AdminPanelSettingsRounded;
