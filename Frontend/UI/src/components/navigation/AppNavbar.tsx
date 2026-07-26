import React from 'react';
import {
  Badge,
  Box,
  Button,
  Chip,
  IconButton,
  List,
  ListItem,
  ListItemButton,
  ListItemText,
  Paper,
  Popover,
  Stack,
  Typography,
} from '@mui/material';
import { MenuRounded, LogoutRounded, NotificationsRounded } from '@mui/icons-material';
import { useAuth } from '../../hooks/useAuth';
import { useNotifications } from '../../hooks/useNotifications';
import { getRoleDisplayName } from '../../utils/roles';

interface AppNavbarProps {
  onMenuClick: () => void;
}

const AppNavbar: React.FC<AppNavbarProps> = ({ onMenuClick }) => {
  const { user, role, logout } = useAuth();
  const { notifications, unreadCount, markAsRead } = useNotifications();
  const [anchorEl, setAnchorEl] = React.useState<HTMLButtonElement | null>(null);

  const handleLogout = async () => {
    await logout();
  };

  const handleNotificationOpen = (event: React.MouseEvent<HTMLButtonElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleNotificationClose = () => {
    setAnchorEl(null);
  };

  const open = Boolean(anchorEl);

  return (
    <Paper
      elevation={0}
      sx={{
        p: { xs: 1.25, md: 1.5 },
        borderRadius: 3,
        border: '1px solid',
        borderColor: 'divider',
        bgcolor: 'background.paper',
      }}
    >
      <Stack direction="row" alignItems="center" justifyContent="space-between" spacing={2}>
        <Stack direction="row" alignItems="center" spacing={1.5}>
          <IconButton
            color="primary"
            onClick={onMenuClick}
            sx={{ display: { lg: 'none' } }}
            aria-label="Open navigation menu"
          >
            <MenuRounded />
          </IconButton>

          <Box>
            <Typography fontSize={15} fontWeight={700} color="text.primary">
              {user?.name ?? 'Sarathi User'}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {user?.email ?? ''}
            </Typography>
          </Box>
        </Stack>

        <Stack direction="row" alignItems="center" spacing={1}>
          <IconButton color="primary" onClick={handleNotificationOpen} aria-label="Open notifications">
            <Badge badgeContent={unreadCount} color="error">
              <NotificationsRounded />
            </Badge>
          </IconButton>
          {role && <Chip size="small" color="primary" label={getRoleDisplayName(role)} />}
          <Button
            variant="outlined"
            color="primary"
            size="small"
            onClick={handleLogout}
            startIcon={<LogoutRounded fontSize="small" />}
          >
            Logout
          </Button>
        </Stack>
      </Stack>

      <Popover
        open={open}
        anchorEl={anchorEl}
        onClose={handleNotificationClose}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
        transformOrigin={{ vertical: 'top', horizontal: 'right' }}
        PaperProps={{ sx: { width: 360, mt: 1, borderRadius: 3 } }}
      >
        <Box sx={{ p: 2, borderBottom: '1px solid', borderColor: 'divider' }}>
          <Typography variant="h6">Notifications</Typography>
          <Typography variant="body2" color="text.secondary">{unreadCount} unread</Typography>
        </Box>
        <List disablePadding sx={{ maxHeight: 420, overflowY: 'auto' }}>
          {notifications.length === 0 ? (
            <ListItem>
              <ListItemText primary="No notifications yet." secondary="Realtime updates will show here." />
            </ListItem>
          ) : notifications.slice(0, 8).map((notification) => (
            <ListItem key={notification.id} disablePadding divider>
              <ListItemButton onClick={() => !notification.isRead ? void markAsRead(notification.id) : undefined}>
                <ListItemText
                  primary={(
                    <Stack direction="row" alignItems="center" spacing={1}>
                      <Typography fontWeight={notification.isRead ? 500 : 700}>{notification.title}</Typography>
                      {!notification.isRead && <Chip size="small" label="New" color="primary" />}
                    </Stack>
                  )}
                  secondary={(
                    <>
                      <Typography variant="body2" color="text.primary">{notification.message}</Typography>
                      <Typography variant="caption" color="text.secondary">{new Date(notification.createdAtUtc).toLocaleString()}</Typography>
                    </>
                  )}
                />
              </ListItemButton>
            </ListItem>
          ))}
        </List>
      </Popover>
    </Paper>
  );
};

export default AppNavbar;
