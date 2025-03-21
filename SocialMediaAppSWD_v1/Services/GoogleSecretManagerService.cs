using Google.Cloud.SecretManager.V1;
using Microsoft.Extensions.Logging;
using SocialMediaAppSWD_v1.Interfaces;

namespace SocialMediaAppSWD_v1.Services
{
    public class GoogleSecretManagerService : ISecretManagerService
    {
        private readonly string _projectId;
        private readonly SecretManagerServiceClient _client;
        private readonly ILogger<GoogleSecretManagerService> _logger;

        public GoogleSecretManagerService(string projectId, ILogger<GoogleSecretManagerService> logger = null)
        {
            _projectId = projectId;
            _client = SecretManagerServiceClient.Create();
            _logger = logger;
            
            _logger?.LogInformation("GoogleSecretManagerService initialized for project: {ProjectId}", projectId);
        }

        public async Task<string> GetSecretAsync(string secretName)
        {
            _logger?.LogDebug("Retrieving secret: {SecretName}", secretName);
            
            try
            {
                var secretVersionName = new SecretVersionName(_projectId, secretName, "latest");
                var result = await _client.AccessSecretVersionAsync(secretVersionName);
                
                _logger?.LogDebug("Successfully retrieved secret: {SecretName}", secretName);
                return result.Payload.Data.ToStringUtf8();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving secret: {SecretName}", secretName);
                throw;
            }
        }

        public async Task LoadSecretsIntoConfigurationAsync(IConfiguration configuration)
        {
            _logger?.LogInformation("Loading secrets into configuration");
            
            try
            {
                var secrets = new Dictionary<string, string>
                {
                    { "Authentication:Google:ClientId", await GetSecretAsync("Authentication-Google-ClientId") },
                    { "Authentication:Google:ClientSecret", await GetSecretAsync("Authentication-Google-ClientSecret") },
                    { "Authentication:Google:StorageBucketName", await GetSecretAsync("Authentication-Google-StorageBucketName") },
                    { "Redis:ConnectionString", await GetSecretAsync("Redis-ConnectionString") }
                };

                foreach (var secret in secrets)
                {
                    configuration[secret.Key] = secret.Value;
                    _logger?.LogDebug("Secret loaded into configuration: {ConfigKey}", secret.Key);
                }
                
                _logger?.LogInformation("Successfully loaded {Count} secrets into configuration", secrets.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading secrets into configuration");
                throw;
            }
        }
    }
}
