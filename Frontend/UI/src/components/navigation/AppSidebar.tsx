import React from 'react';
import {
  Box,
  Drawer,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Stack,
  Typography,
} from '@mui/material';
import { LocalActivityRounded } from '@mui/icons-material';
import { NavLink, useLocation } from 'react-router-dom';
import { appBrand, getNavigationItems } from '../../config/navigation/appNavigation';
import { useAuth } from '../../hooks/useAuth';

const DRAWER_WIDTH = 276;

interface AppSidebarProps {
  mobileOpen: boolean;
  onCloseMobile: () => void;
}

const AppSidebarContent: React.FC<{ onNavigate?: () => void }> = ({ onNavigate }) => {
  const location = useLocation();
  const { role } = useAuth();
  const navigationItems = getNavigationItems(role);

  return (
    <Stack
      sx={{
        height: '100%',
        p: 2,
        bgcolor: 'background.default',
      }}
      spacing={2}
    >
      <Stack direction="row" alignItems="center" spacing={1.5} sx={{ px: 1, py: 1.25 }}>
        <Box
          sx={{
            width: 38,
            height: 38,
            borderRadius: 1.6,
            display: 'grid',
            placeItems: 'center',
            color: '#ffffff',
            background: 'linear-gradient(160deg,#2f66f6 0%, #3f41e8 100%)',
          }}
        >
          <LocalActivityRounded sx={{ fontSize: 22 }} />
        </Box>

        <Box>
          <Typography fontSize={27} fontWeight={800} lineHeight={1} color="text.primary">
            {appBrand.name}
          </Typography>
          <Typography variant="caption" color="text.secondary" sx={{ letterSpacing: 0.2 }}>
            {appBrand.subtitle}
          </Typography>
        </Box>
      </Stack>

      <List disablePadding sx={{ display: 'grid', gap: 0.5 }}>
        {navigationItems.map((item) => {
          const Icon = item.icon;
          const isActive =
            location.pathname === item.path ||
            (item.path !== '/home' && location.pathname.startsWith(item.path));

          return (
            <ListItemButton
              key={item.path}
              component={NavLink}
              to={item.path}
              onClick={onNavigate}
              sx={{
                borderRadius: 2,
                minHeight: 44,
                px: 1.25,
                color: isActive ? '#ffffff' : 'text.primary',
                bgcolor: isActive ? 'primary.main' : 'transparent',
                '&:hover': {
                  bgcolor: isActive ? 'primary.main' : 'action.hover',
                },
              }}
            >
              <ListItemIcon
                sx={{
                  minWidth: 34,
                  color: 'inherit',
                }}
              >
                <Icon fontSize="small" />
              </ListItemIcon>
              <ListItemText
                primary={item.label}
                primaryTypographyProps={{
                  fontSize: 14,
                  fontWeight: isActive ? 700 : 600,
                }}
              />
            </ListItemButton>
          );
        })}
      </List>
    </Stack>
  );
};

const AppSidebar: React.FC<AppSidebarProps> = ({ mobileOpen, onCloseMobile }) => {
  return (
    <>
      <Drawer
        variant="temporary"
        open={mobileOpen}
        onClose={onCloseMobile}
        ModalProps={{ keepMounted: true }}
        sx={{
          display: { xs: 'block', lg: 'none' },
          '& .MuiDrawer-paper': {
            width: DRAWER_WIDTH,
            borderRight: '1px solid',
            borderColor: 'divider',
          },
        }}
      >
        <AppSidebarContent onNavigate={onCloseMobile} />
      </Drawer>

      <Drawer
        variant="permanent"
        sx={{
          display: { xs: 'none', lg: 'block' },
          '& .MuiDrawer-paper': {
            width: DRAWER_WIDTH,
            borderRight: '1px solid',
            borderColor: 'divider',
            boxSizing: 'border-box',
          },
        }}
        open
      >
        <AppSidebarContent />
      </Drawer>
    </>
  );
};

export { DRAWER_WIDTH };
export default AppSidebar;
