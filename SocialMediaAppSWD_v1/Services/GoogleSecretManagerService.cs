using Google.Cloud.SecretManager.V1;
using SocialMediaAppSWD_v1.Interfaces;

namespace SocialMediaAppSWD_v1.Services
{
    public class GoogleSecretManagerService : ISecretManagerService
    {
        private readonly string _projectId;
        private readonly SecretManagerServiceClient _client;
        private readonly ICloudLoggingService _logger;

        public GoogleSecretManagerService(string projectId, ICloudLoggingService logger)
        {
            _projectId = projectId;
            _client = SecretManagerServiceClient.Create();
            _logger = logger;
            
            _logger.LogInformationAsync($"GoogleSecretManagerService initialized for project: {projectId}").Wait();
        }

        public async Task<string> GetSecretAsync(string secretName)
        {
            await _logger.LogDebugAsync($"Retrieving secret: {secretName}");
            
            try
            {
                var secretVersionName = new SecretVersionName(_projectId, secretName, "latest");
                var result = await _client.AccessSecretVersionAsync(secretVersionName);
                
                await _logger.LogDebugAsync($"Successfully retrieved secret: {secretName}");
                return result.Payload.Data.ToStringUtf8();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Error retrieving secret: {secretName}", ex);
                throw;
            }
        }

        public async Task LoadSecretsIntoConfigurationAsync(IConfiguration configuration)
        {
            await _logger.LogInformationAsync("Loading secrets into configuration");
            
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
                    await _logger.LogDebugAsync($"Secret loaded into configuration: {secret.Key}");
                }
                
                await _logger.LogInformationAsync($"Successfully loaded {secrets.Count} secrets into configuration");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Error loading secrets into configuration", ex);
                throw;
            }
        }
    }
}
