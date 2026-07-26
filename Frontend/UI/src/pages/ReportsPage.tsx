import React from 'react';
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Paper,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material';
import { DownloadRounded } from '@mui/icons-material';
import { PageHeader, SimpleBarChart } from '../components/shared';
import { reportsService, type ReportSectionDto, type ReportsOverviewDto } from '../services/reportsService';

const ReportsPage: React.FC = () => {
  const [data, setData] = React.useState<ReportsOverviewDto | null>(null);
  const [isLoading, setIsLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);
  const [exportingKey, setExportingKey] = React.useState<string | null>(null);

  React.useEffect(() => {
    const loadReports = async () => {
      setIsLoading(true);
      setError(null);

      try {
        const response = await reportsService.getOverview();
        setData(response);
      } catch (loadError) {
        const message = loadError instanceof Error ? loadError.message : 'Unable to load reports overview.';
        setError(message);
      } finally {
        setIsLoading(false);
      }
    };

    void loadReports();
  }, []);

  const handleExport = async (section: ReportSectionDto) => {
    setExportingKey(section.key);
    setError(null);

    try {
      const response = await reportsService.exportSection(section.key);
      const byteCharacters = atob(response.contentBase64);
      const byteNumbers = Array.from(byteCharacters, (char) => char.charCodeAt(0));
      const byteArray = new Uint8Array(byteNumbers);
      const blob = new Blob([byteArray], { type: response.contentType });
      const url = window.URL.createObjectURL(blob);
      const anchor = document.createElement('a');
      anchor.href = url;
      anchor.download = response.fileName;
      anchor.click();
      window.URL.revokeObjectURL(url);
    } catch (exportError) {
      const message = exportError instanceof Error ? exportError.message : 'Unable to export report.';
      setError(message);
    } finally {
      setExportingKey(null);
    }
  };

  return (
    <Box>
      <PageHeader
        title="Reports"
        subtitle="Executive-ready portfolio, sprint, work item, and KPI reports with export-ready data views."
      />

      {error && <Alert severity="error" sx={{ mb: 2.5 }}>{error}</Alert>}

      {isLoading || !data ? (
        <Stack alignItems="center" py={8}>
          <CircularProgress />
        </Stack>
      ) : (
        <Stack spacing={3}>
          {data.sections.map((section) => (
            <Paper key={section.key} elevation={0} sx={{ p: 2.5, borderRadius: 3, border: '1px solid', borderColor: 'divider' }}>
              <PageHeader
                title={section.title}
                subtitle={section.description}
                actions={(
                  <Button
                    variant="outlined"
                    startIcon={<DownloadRounded />}
                    onClick={() => handleExport(section)}
                    disabled={exportingKey === section.key}
                  >
                    {exportingKey === section.key ? 'Exporting...' : 'Export'}
                  </Button>
                )}
              />

              <Box
                sx={{
                  display: 'grid',
                  gap: 2,
                  mb: 2.5,
                  gridTemplateColumns: {
                    xs: '1fr',
                    md: 'repeat(3, minmax(0, 1fr))',
                  },
                }}
              >
                {section.summaries.map((summary) => (
                  <Paper key={`${section.key}-${summary.label}`} elevation={0} sx={{ p: 2, borderRadius: 3, bgcolor: 'background.default', border: '1px solid', borderColor: 'divider' }}>
                    <Typography variant="h5" sx={{ mb: 0.5 }}>{summary.value}</Typography>
                    <Typography variant="body2" color="text.secondary">{summary.label}</Typography>
                  </Paper>
                ))}
              </Box>

              <Box sx={{ mb: 2.5 }}>
                <SimpleBarChart
                  items={section.chartPoints}
                  emptyMessage="No chart data available for this report section."
                  valueFormatter={(value) => value % 1 === 0 ? value.toFixed(0) : value.toFixed(2)}
                />
              </Box>

              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      {section.columns.map((column) => (
                        <TableCell key={`${section.key}-${column}`}>{column}</TableCell>
                      ))}
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {section.rows.map((row, rowIndex) => (
                      <TableRow key={`${section.key}-row-${rowIndex}`} hover>
                        {section.columns.map((column) => (
                          <TableCell key={`${section.key}-row-${rowIndex}-${column}`}>{row.cells[column] ?? '-'}</TableCell>
                        ))}
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </Paper>
          ))}

          <Typography variant="caption" color="text.secondary">
            Generated at {new Date(data.generatedAtUtc).toLocaleString()}.
          </Typography>
        </Stack>
      )}
    </Box>
  );
};

export default ReportsPage;