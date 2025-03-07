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
        _bucketName = configuration["Authentication:Google:BucketName"];
        GoogleCredential credentials = GoogleCredential.FromFile(configuration["Authentication:Google:ServiceAccountCredentials"]);
        _storageClient = StorageClient.Create(credentials);
    }
    
    
    public async Task<string> UploadFileAsync(IFormFile file, string fileName)
    {
        try
        {
            if (!file.ContentType.StartsWith("image/"))
            {
                _logger.LogWarning($"Invalid file type: {file.ContentType}. Only images are allowed.");
                throw new InvalidOperationException("Only image files are allowed.");
            }
            
            fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            _logger.LogInformation($"Uploading file {fileName} to bucket {_bucketName}");

            // Create a memory stream to read the file
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            // Upload the file to Google Cloud Storage
            var uploadedObject = await _storageClient.UploadObjectAsync(
                bucket: _bucketName,
                objectName: fileName,
                contentType: file.ContentType,
                source: memoryStream);

            // Generate the public URL for the file
            string publicUrl = $"https://storage.googleapis.com/{_bucketName}/{fileName}";
        
            _logger.LogInformation($"File uploaded successfully. Public URL: {publicUrl}");
            return publicUrl;
        }
        catch (Google.GoogleApiException gex) 
        {
            _logger.LogError($"Google Cloud Storage API error: {gex.Message}, Error: {gex.Error?.Message}, Code: {gex.Error?.Code}");
            throw new ApplicationException("Cloud storage service error occurred.", gex);
        }
        catch (IOException ioex)
        {
            _logger.LogError($"IO error while uploading file: {ioex.Message}");
            throw new ApplicationException("Failed to read or process file data.", ioex);
        }
        catch (ArgumentException aex)
        {
            _logger.LogError($"Invalid argument for file upload: {aex.Message}");
            throw new ApplicationException("Invalid file upload parameters.", aex);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error uploading file: {ex.Message}");
            throw new ApplicationException("An unexpected error occurred during file upload.", ex);
        }
    }
    public async Task DeletePostImageAsync(string imageUrl)
    {
        try
        {
            // Extract filename from URL
            string fileName = imageUrl.Substring(imageUrl.LastIndexOf('/') + 1);
        
            // Get storage client from DI or create it
        
            // Delete the object from GCS
            await _storageClient.DeleteObjectAsync(_bucketName, fileName);
            _logger.LogInformation($"Post image {fileName} was successfully deleted from bucket {_bucketName}");
        }
        catch (Google.GoogleApiException ex)
        {
            _logger.LogError(ex, $"Google Cloud Storage error while deleting image: {ex.Message}");
            // We don't want to fail the post deletion if image deletion fails
            // Just log the error and continue
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error while deleting post image: {ex.Message}");
            // Same here, don't let image deletion failure affect post deletion
        }
    }
}