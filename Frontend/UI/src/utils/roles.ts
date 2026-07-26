export type CanonicalRole = 'Administrator' | 'ProjectManager' | 'ITAdmin';

const roleAliasMap: Record<string, CanonicalRole> = {
  administrator: 'Administrator',
  projectmanager: 'ProjectManager',
  'project manager': 'ProjectManager',
  itadmin: 'ITAdmin',
  'it admin': 'ITAdmin',
};

export const normalizeRole = (role: string | null | undefined): CanonicalRole | null => {
  if (!role) {
    return null;
  }

  const cleaned = role.trim().toLowerCase();
  return roleAliasMap[cleaned] ?? null;
};

export const getRoleDisplayName = (role: string | null | undefined): string => {
  const normalized = normalizeRole(role);

  switch (normalized) {
    case 'Administrator':
      return 'Administrator';
    case 'ProjectManager':
      return 'Project Manager';
    case 'ITAdmin':
      return 'IT Admin';
    default:
      return role?.trim() || 'Unknown';
  }
};
