﻿﻿using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using SocialMediaAppSWD_v1.Interfaces;

namespace SocialMediaAppSWD_v1.Services;

public class FileUploadService: IFileUploadService
{
    private readonly ICloudLoggingService _logger;
    private readonly StorageClient _storageClient;
    private readonly string _bucketName;
    
    public FileUploadService(ICloudLoggingService logger, IConfiguration configuration)
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
                await _logger.LogWarningAsync($"Invalid file type: {file.ContentType}. Only images are allowed.");
                throw new InvalidOperationException("Only image files are allowed.");
            }
            
            fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            await _logger.LogInformationAsync($"Uploading file {fileName} to bucket {_bucketName}");

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
        
            await _logger.LogInformationAsync($"File uploaded successfully. Public URL: {publicUrl}");
            return publicUrl;
        }
        catch (Google.GoogleApiException gex) 
        {
            await _logger.LogErrorAsync("Google Cloud Storage API error", gex);
            throw new ApplicationException("Cloud storage service error occurred.", gex);
        }
        catch (IOException ioex)
        {
            await _logger.LogErrorAsync("IO error while uploading file", ioex);
            throw new ApplicationException("Failed to read or process file data.", ioex);
        }
        catch (ArgumentException aex)
        {
            await _logger.LogErrorAsync("Invalid argument for file upload", aex);
            throw new ApplicationException("Invalid file upload parameters.", aex);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync("Unexpected error uploading file", ex);
            throw new ApplicationException("An unexpected error occurred during file upload.", ex);
        }
    }

    public async Task DeletePostImageAsync(string imageUrl)
    {
        try
        {
            string fileName = imageUrl.Substring(imageUrl.LastIndexOf('/') + 1);
            await _storageClient.DeleteObjectAsync(_bucketName, fileName);
            await _logger.LogInformationAsync($"Post image {fileName} was successfully deleted from bucket {_bucketName}");
        }
        catch (Google.GoogleApiException ex)
        {
            await _logger.LogErrorAsync("Google Cloud Storage error while deleting image", ex);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync("Unexpected error while deleting post image", ex);
        }
    }
}
