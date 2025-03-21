using System.Threading.Tasks;

namespace SocialMediaAppSWD_v1.Interfaces
{
    public interface ISecretManagerService
    {
        Task<string> GetSecretAsync(string secretName);
        Task LoadSecretsIntoConfigurationAsync(IConfiguration configuration);
    }
}
