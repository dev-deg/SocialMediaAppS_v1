using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using SocialMediaAppSWD_v1.Interfaces;

namespace SocialMediaAppSWD_v1.Services;

public class FileUploadService: IFileUploadService
{
    private readonly ILogger<FileUploadService> _logger;
    private readonly StorageClient _storageClient;
    private readonly string _bucketName;
    
    public FileUploadService(ILogger<FileUploadService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _bucketName = configuration["GoogleCloudStorage:BucketName"];
        GoogleCredential credentials = GoogleCredential.FromFile(configuration["GoogleCloudStorage:ServiceAccountCredentials"]);
        _storageClient = StorageClient.Create(credentials);
    }
    
    
    public Task<string> UploadFileAsync(IFormFile file, string fileName)
    {
        throw new NotImplementedException();
    }
}