using Google.Api;
using Google.Cloud.Logging.V2;
using Google.Cloud.Logging.Type;
using SocialMediaAppSWD_v1.Interfaces;
using System.Text.Json;

namespace SocialMediaAppSWD_v1.Services
{
    public class GoogleCloudLoggingService : ICloudLoggingService
    {
        private readonly LoggingServiceV2Client _loggingClient;
        private readonly string _projectId;
        private readonly string _logName;
        private readonly MonitoredResource _resource;
        private readonly ILogger<GoogleCloudLoggingService> _localLogger;
        private readonly IWebHostEnvironment _environment;

        public GoogleCloudLoggingService(
            IConfiguration configuration, 
            ILogger<GoogleCloudLoggingService> localLogger,
            IWebHostEnvironment environment)
        {
            _localLogger = localLogger;
            _environment = environment;
            
            try
            {
                _projectId = configuration["GoogleCloud:ProjectId"];
                if (string.IsNullOrEmpty(_projectId))
                {
                    throw new ArgumentNullException(nameof(_projectId), "Google Cloud Project ID is not configured");
                }

                _logName = configuration["GoogleCloud:LogName"] ?? "social-media-app-log";
                _loggingClient = LoggingServiceV2Client.Create();

                // Define the monitored resource (typically this would be a GCE or GKE instance)
                // For non-GCP environments, we use a generic global resource type
                _resource = new MonitoredResource
                {
                    Type = "global",
                    Labels = { { "project_id", _projectId } }
                };

                _localLogger.LogInformation("Google Cloud Logging service initialized for project: {ProjectId}", _projectId);
            }
            catch (Exception ex)
            {
                _localLogger.LogError(ex, "Failed to initialize Google Cloud Logging service");
                throw;
            }
        }

        public async Task LogAsync(LogSeverity severity, string message, Dictionary<string, string> labels = null)
        {
            try
            {
                var logName = new LogName(_projectId, _logName);
                
                var logEntry = new LogEntry
                {
                    LogName = logName.ToString(),
                    Severity = severity,
                    TextPayload = message,
                    Resource = _resource,
                    Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
                    Labels = { }  // Initialize empty labels dictionary
                };

                // Add standard labels
                var standardLabels = new Dictionary<string, string>
                {
                    { "environment", _environment.EnvironmentName },
                    { "application", "SocialMediaApp" }
                };

                // Merge user-provided labels with standard labels
                if (labels != null)
                {
                    foreach (var label in labels)
                    {
                        standardLabels[label.Key] = label.Value;
                    }
                }

                // Set the labels on the entry
                logEntry.Labels.Add(standardLabels);

                // Write the log entry
                await _loggingClient.WriteLogEntriesAsync(
                    logName.ToString(),
                    _resource,
                   null,
                    new[] { logEntry });
            }
            catch (Exception ex)
            {
                // Fall back to local logging if cloud logging fails
                _localLogger.LogError(ex, "Failed to write to Google Cloud Logging: {Message}", message);
            }
        }

        public async Task LogInformationAsync(string message, Dictionary<string, string> labels = null)
        {
            await LogAsync(LogSeverity.Info, message, labels);
        }

        public async Task LogWarningAsync(string message, Dictionary<string, string> labels = null)
        {
            await LogAsync(LogSeverity.Warning, message, labels);
        }

        public async Task LogErrorAsync(string message, Exception exception = null, Dictionary<string, string> labels = null)
        {
            Dictionary<string, string> enhancedLabels = labels ?? new Dictionary<string, string>();
            
            if (exception != null)
            {
                string exceptionDetails = JsonSerializer.Serialize(new
                {
                    Message = exception.Message,
                    StackTrace = exception.StackTrace,
                    Source = exception.Source,
                    InnerException = exception.InnerException?.Message
                });
                
                message = $"{message}. Exception: {exceptionDetails}";
            }
            
            await LogAsync(LogSeverity.Error, message, enhancedLabels);
        }

        public async Task LogDebugAsync(string message, Dictionary<string, string> labels = null)
        {
            await LogAsync(LogSeverity.Debug, message, labels);
        }

        public async Task LogCriticalAsync(string message, Exception exception = null, Dictionary<string, string> labels = null)
        {
            Dictionary<string, string> enhancedLabels = labels ?? new Dictionary<string, string>();
            
            if (exception != null)
            {
                string exceptionDetails = JsonSerializer.Serialize(new
                {
                    Message = exception.Message,
                    StackTrace = exception.StackTrace,
                    Source = exception.Source,
                    InnerException = exception.InnerException?.Message
                });
                
                message = $"{message}. Exception: {exceptionDetails}";
            }
            
            await LogAsync(LogSeverity.Critical, message, enhancedLabels);
        }
    }
}
