using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace VPM.Services
{
    /// <summary>
    /// Performance monitoring system for tracking Hub browser operations
    /// Measures timing of API calls, grid loading, and UI operations
    /// </summary>
    public class PerformanceMonitor
    {
        private readonly Dictionary<string, PerformanceMetric> _metrics = new Dictionary<string, PerformanceMetric>();
        private readonly object _lock = new object();
        private readonly Stopwatch _globalStopwatch = new Stopwatch();

        public PerformanceMonitor()
        {
            _globalStopwatch.Start();
        }

        /// <summary>
        /// Start measuring a named operation
        /// </summary>
        public PerformanceTimer StartOperation(string operationName)
        {
            return new PerformanceTimer(operationName, this);
        }

        /// <summary>
        /// Record a completed operation timing
        /// </summary>
        internal void RecordOperation(string operationName, long elapsedMs, string details = null)
        {
            lock (_lock)
            {
                if (!_metrics.ContainsKey(operationName))
                {
                    _metrics[operationName] = new PerformanceMetric { OperationName = operationName };
                }

                var metric = _metrics[operationName];
                metric.Measurements.Add(new Measurement { ElapsedMs = elapsedMs, Details = details, Timestamp = DateTime.UtcNow });
                metric.TotalMs += elapsedMs;
                metric.Count++;
                metric.AverageMs = metric.TotalMs / (double)metric.Count;
                metric.MaxMs = Math.Max(metric.MaxMs, elapsedMs);
                metric.MinMs = metric.MinMs == 0 ? elapsedMs : Math.Min(metric.MinMs, elapsedMs);
            }
        }

        /// <summary>
        /// Get all recorded metrics
        /// </summary>
        public Dictionary<string, PerformanceMetric> GetMetrics()
        {
            lock (_lock)
            {
                return new Dictionary<string, PerformanceMetric>(_metrics);
            }
        }

        /// <summary>
        /// Get a formatted report of all metrics
        /// </summary>
        public string GetReport()
        {
            lock (_lock)
            {
                var sb = new StringBuilder();
                sb.AppendLine("=== PERFORMANCE REPORT ===");
                sb.AppendLine($"Total Runtime: {_globalStopwatch.ElapsedMilliseconds}ms");
                sb.AppendLine();

                var sortedMetrics = _metrics.Values.OrderByDescending(m => m.TotalMs).ToList();

                sb.AppendLine("Operations by Total Time:");
                sb.AppendLine(new string('-', 100));
                sb.AppendLine($"{"Operation",-40} {"Count",8} {"Total (ms)",12} {"Avg (ms)",12} {"Min (ms)",12} {"Max (ms)",12}");
                sb.AppendLine(new string('-', 100));

                foreach (var metric in sortedMetrics)
                {
                    sb.AppendLine($"{metric.OperationName,-40} {metric.Count,8} {metric.TotalMs,12:F2} {metric.AverageMs,12:F2} {metric.MinMs,12:F2} {metric.MaxMs,12:F2}");
                }

                sb.AppendLine();
                sb.AppendLine("Top 10 Slowest Individual Operations:");
                sb.AppendLine(new string('-', 100));

                var slowestOps = sortedMetrics
                    .SelectMany(m => m.Measurements.Select(meas => new { Metric = m, Measurement = meas }))
                    .OrderByDescending(x => x.Measurement.ElapsedMs)
                    .Take(10)
                    .ToList();

                foreach (var op in slowestOps)
                {
                    var details = string.IsNullOrEmpty(op.Measurement.Details) ? "" : $" ({op.Measurement.Details})";
                    sb.AppendLine($"{op.Metric.OperationName,-40} {op.Measurement.ElapsedMs,12:F2}ms{details}");
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Get a detailed report for a specific operation
        /// </summary>
        public string GetDetailedReport(string operationName)
        {
            lock (_lock)
            {
                if (!_metrics.ContainsKey(operationName))
                    return $"No metrics found for operation: {operationName}";

                var metric = _metrics[operationName];
                var sb = new StringBuilder();

                sb.AppendLine($"=== DETAILED REPORT: {operationName} ===");
                sb.AppendLine($"Total Calls: {metric.Count}");
                sb.AppendLine($"Total Time: {metric.TotalMs:F2}ms");
                sb.AppendLine($"Average Time: {metric.AverageMs:F2}ms");
                sb.AppendLine($"Min Time: {metric.MinMs:F2}ms");
                sb.AppendLine($"Max Time: {metric.MaxMs:F2}ms");
                sb.AppendLine();
                sb.AppendLine("All Measurements:");
                sb.AppendLine(new string('-', 80));

                foreach (var measurement in metric.Measurements)
                {
                    var details = string.IsNullOrEmpty(measurement.Details) ? "" : $" - {measurement.Details}";
                    sb.AppendLine($"{measurement.Timestamp:HH:mm:ss.fff} | {measurement.ElapsedMs,12:F2}ms{details}");
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Clear all metrics
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _metrics.Clear();
                _globalStopwatch.Restart();
            }
        }
    }

    /// <summary>
    /// Disposable timer for measuring operation duration
    /// </summary>
    public class PerformanceTimer : IDisposable
    {
        private readonly string _operationName;
        private readonly PerformanceMonitor _monitor;
        private readonly Stopwatch _stopwatch;
        private string _details;
        private bool _disposed;

        public PerformanceTimer(string operationName, PerformanceMonitor monitor)
        {
            _operationName = operationName;
            _monitor = monitor;
            _stopwatch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Set additional details about this operation
        /// </summary>
        public PerformanceTimer WithDetails(string details)
        {
            _details = details;
            return this;
        }

        public void Dispose()
        {
            if (_disposed) return;

            _stopwatch.Stop();
            _monitor.RecordOperation(_operationName, _stopwatch.ElapsedMilliseconds, _details);
            _disposed = true;
        }
    }

    /// <summary>
    /// Metric data for a single operation
    /// </summary>
    public class PerformanceMetric
    {
        public string OperationName { get; set; }
        public int Count { get; set; }
        public long TotalMs { get; set; }
        public double AverageMs { get; set; }
        public long MinMs { get; set; }
        public long MaxMs { get; set; }
        public List<Measurement> Measurements { get; set; } = new List<Measurement>();
    }

    /// <summary>
    /// Individual measurement data
    /// </summary>
    public class Measurement
    {
        public long ElapsedMs { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
