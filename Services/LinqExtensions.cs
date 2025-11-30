using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace VPM.Services
{
    /// <summary>
    /// Extension methods for LINQ operations with enhanced error handling
    /// </summary>
    public static class LinqExtensions
    {
        /// <summary>
        /// Safe Take method that logs and prevents negative count values
        /// </summary>
        public static IEnumerable<T> TakeSafe<T>(this IEnumerable<T> source, int count, string caller = "")
        {
            if (count < 0)
            {
                string errorMsg = $"[TakeSafe] Negative count detected: {count}";
                if (!string.IsNullOrEmpty(caller))
                {
                    errorMsg += $" (called from: {caller})";
                }
                
                // Log to debug output
                Debug.WriteLine(errorMsg);
                
                // Log to file for investigation
                try
                {
                    string logPath = "C:\\vpm_take_errors.log";
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    string stackTrace = Environment.StackTrace;
                    string logEntry = $"[{timestamp}] {errorMsg}\nStack Trace:\n{stackTrace}\n---\n";
                    System.IO.File.AppendAllText(logPath, logEntry);
                }
                catch { }
                
                // Return empty instead of throwing
                return Enumerable.Empty<T>();
            }
            
            return source.Take(count);
        }
    }
}
