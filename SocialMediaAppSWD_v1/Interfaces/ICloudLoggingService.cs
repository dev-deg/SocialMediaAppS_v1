using Google.Cloud.Logging.V2;
using Google.Cloud.Logging.Type;

namespace SocialMediaAppSWD_v1.Interfaces
{
    public interface ICloudLoggingService
    {
        Task LogAsync(LogSeverity severity, string message, Dictionary<string, string> labels = null);
        Task LogInformationAsync(string message, Dictionary<string, string> labels = null);
        Task LogWarningAsync(string message, Dictionary<string, string> labels = null);
        Task LogErrorAsync(string message, Exception exception = null, Dictionary<string, string> labels = null);
        Task LogDebugAsync(string message, Dictionary<string, string> labels = null);
        Task LogCriticalAsync(string message, Exception exception = null, Dictionary<string, string> labels = null);
    }
}
