import React from 'react';
import { Box, Stack, Typography } from '@mui/material';

export interface SimpleBarChartItem {
  label: string;
  value: number;
}

interface SimpleBarChartProps {
  items: SimpleBarChartItem[];
  color?: string;
  emptyMessage?: string;
  valueFormatter?: (value: number) => string;
}

const SimpleBarChart: React.FC<SimpleBarChartProps> = ({
  items,
  color = 'linear-gradient(90deg, #0f766e 0%, #14b8a6 100%)',
  emptyMessage = 'No chart data available.',
  valueFormatter = (value) => value.toFixed(0),
}) => {
  if (!items.length) {
    return (
      <Typography variant="body2" color="text.secondary">
        {emptyMessage}
      </Typography>
    );
  }

  const maxValue = Math.max(...items.map((item) => item.value), 1);

  return (
    <Stack spacing={1.5}>
      {items.map((item) => (
        <Box key={`${item.label}-${item.value}`}>
          <Stack direction="row" justifyContent="space-between" spacing={1} sx={{ mb: 0.5 }}>
            <Typography variant="body2" color="text.secondary">
              {item.label}
            </Typography>
            <Typography variant="body2" color="text.primary">
              {valueFormatter(item.value)}
            </Typography>
          </Stack>
          <Box
            sx={{
              height: 10,
              borderRadius: 999,
              bgcolor: 'action.hover',
              overflow: 'hidden',
            }}
          >
            <Box
              sx={{
                width: `${Math.max((item.value / maxValue) * 100, 6)}%`,
                height: '100%',
                borderRadius: 999,
                background: color,
              }}
            />
          </Box>
        </Box>
      ))}
    </Stack>
  );
};

export default SimpleBarChart;