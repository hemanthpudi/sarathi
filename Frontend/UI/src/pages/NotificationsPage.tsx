import React from 'react';
import {
  Box,
  Button,
  Chip,
  List,
  ListItem,
  ListItemButton,
  ListItemText,
  Paper,
  Stack,
  Typography,
} from '@mui/material';
import { PageHeader } from '../components/shared';
import { useNotifications } from '../hooks/useNotifications';

const NotificationsPage: React.FC = () => {
  const { notifications, unreadCount, markAsRead } = useNotifications();

  return (
    <Box>
      <PageHeader
        title="Notification Center"
        subtitle="Realtime updates, alerts, and workflow messages across your Sarathi workspace."
        actions={<Chip color="primary" label={`${unreadCount} unread`} />}
      />

      <Paper elevation={0} sx={{ borderRadius: 3, border: '1px solid', borderColor: 'divider', overflow: 'hidden' }}>
        <List disablePadding>
          {notifications.length === 0 ? (
            <ListItem>
              <ListItemText primary="No notifications yet." secondary="Alerts and sync updates will appear here." />
            </ListItem>
          ) : notifications.map((notification) => (
            <ListItem key={notification.id} disablePadding divider>
              <ListItemButton onClick={() => !notification.isRead ? void markAsRead(notification.id) : undefined}>
                <ListItemText
                  primary={(
                    <Stack direction="row" alignItems="center" spacing={1}>
                      <Typography fontWeight={notification.isRead ? 500 : 700}>{notification.title}</Typography>
                      {!notification.isRead && <Chip size="small" label="New" color="primary" />}
                      <Chip size="small" label={notification.category} variant="outlined" />
                    </Stack>
                  )}
                  secondary={(
                    <Stack spacing={0.75}>
                      <Typography variant="body2" color="text.primary">{notification.message}</Typography>
                      <Typography variant="caption" color="text.secondary">{new Date(notification.createdAtUtc).toLocaleString()}</Typography>
                      {notification.actionUrl && (
                        <Button href={notification.actionUrl} size="small" sx={{ alignSelf: 'flex-start', px: 0 }}>
                          Open
                        </Button>
                      )}
                    </Stack>
                  )}
                />
              </ListItemButton>
            </ListItem>
          ))}
        </List>
      </Paper>
    </Box>
  );
};

export default NotificationsPage;