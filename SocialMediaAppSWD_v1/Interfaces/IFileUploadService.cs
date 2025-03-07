namespace SocialMediaAppSWD_v1.Interfaces;

public interface IFileUploadService
{
    Task<string> UploadFileAsync(IFormFile file, string fileName);

    Task DeletePostImageAsync(string imageUrl);
}