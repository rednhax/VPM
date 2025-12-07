using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using VPM.Services;

namespace VPM.Windows
{
    /// <summary>
    /// Performance monitoring window for Hub browser operations
    /// </summary>
    public partial class PerformanceWindow : Window
    {
        private readonly PerformanceMonitor _performanceMonitor;
        private ObservableCollection<OperationSummary> _operationSummaries;

        public PerformanceWindow(PerformanceMonitor performanceMonitor)
        {
            InitializeComponent();
            _performanceMonitor = performanceMonitor;
            _operationSummaries = new ObservableCollection<OperationSummary>();
            OperationsListBox.ItemsSource = _operationSummaries;
            RefreshData();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Clear all performance metrics?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _performanceMonitor.Clear();
                RefreshData();
                StatusText.Text = "Metrics cleared";
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var report = _performanceMonitor.GetReport();
                var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), 
                    $"PerformanceReport_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");
                
                File.WriteAllText(filePath, report);
                StatusText.Text = $"Report exported to: {filePath}";
                MessageBox.Show($"Report exported to:\n{filePath}", "Export Complete");
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Export failed: {ex.Message}";
                MessageBox.Show($"Export failed: {ex.Message}", "Error");
            }
        }

        private void RefreshData()
        {
            try
            {
                var metrics = _performanceMonitor.GetMetrics();
                
                // Update summary
                var report = _performanceMonitor.GetReport();
                SummaryTextBox.Text = report;

                // Update operations list
                _operationSummaries.Clear();
                foreach (var metric in metrics.Values.OrderByDescending(m => m.TotalMs))
                {
                    _operationSummaries.Add(new OperationSummary
                    {
                        OperationName = metric.OperationName,
                        Count = metric.Count,
                        TotalMs = metric.TotalMs,
                        AverageMs = metric.AverageMs,
                        MinMs = metric.MinMs,
                        MaxMs = metric.MaxMs,
                        Summary = $"Count: {metric.Count} | Avg: {metric.AverageMs:F2}ms | Total: {metric.TotalMs:F2}ms"
                    });
                }

                LastUpdatedText.Text = DateTime.Now.ToString("HH:mm:ss");
                StatusText.Text = $"Loaded {metrics.Count} operations";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error: {ex.Message}";
            }
        }

        private void OperationsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (OperationsListBox.SelectedItem is OperationSummary summary)
            {
                var detailedReport = _performanceMonitor.GetDetailedReport(summary.OperationName);
                DetailsTextBox.Text = detailedReport;
                DetailsTextBox.Visibility = Visibility.Visible;
            }
        }
    }

    /// <summary>
    /// Summary of an operation for display
    /// </summary>
    public class OperationSummary
    {
        public string OperationName { get; set; }
        public int Count { get; set; }
        public long TotalMs { get; set; }
        public double AverageMs { get; set; }
        public long MinMs { get; set; }
        public long MaxMs { get; set; }
        public string Summary { get; set; }
    }
}
