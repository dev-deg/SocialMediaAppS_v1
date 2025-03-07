using Google.Cloud.Firestore;
using SocialMediaAppSWD_v1.Interfaces;
using SocialMediaAppSWD_v1.Models;
namespace SocialMediaAppSWD_v1.DataAccess;

public class FirestoreRepository
{
    private readonly ILogger<FirestoreRepository> _logger;
    private FirestoreDb _db;
    private readonly IFileUploadService _fileUploadService;
    public FirestoreRepository(ILogger<FirestoreRepository> logger, IConfiguration configuration, IFileUploadService fileUploadService)
    {
        _logger = logger;
        _db = FirestoreDb.Create(configuration.GetValue<string>("Authentication:Google:ProjectId"));
        _fileUploadService = fileUploadService;
    }

    public async Task AddPost(SocialMediaPost post)
    {
        await _db.Collection("posts").AddAsync(post);
        _logger.LogInformation($"Post {post.PostId} added to Firestore");
    }

    public async Task<List<SocialMediaPost>> GetPosts()
    {
        List<SocialMediaPost> posts = new List<SocialMediaPost>();
        Query allPostsQuery = _db.Collection(("posts"));
        QuerySnapshot allPostsQuerySnapshot = await allPostsQuery.GetSnapshotAsync();
        foreach (DocumentSnapshot document in allPostsQuerySnapshot)
        {
            SocialMediaPost post = document.ConvertTo<SocialMediaPost>();
            posts.Add(post);
        }
        _logger.LogInformation($"{posts.Count} loaded successfully.");
        return posts;
    }

    public async Task DeletePost(string postId)
    {
        if (string.IsNullOrWhiteSpace(postId))
        {
            throw new ArgumentException("Post ID cannot be null or empty", nameof(postId));
        }

        try
        {
            // Find the document with matching PostId
            Query query = _db.Collection("posts").WhereEqualTo("PostId", postId);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            if (querySnapshot.Count == 0)
            {
                throw new KeyNotFoundException($"Post with ID {postId} was not found");
            }

            // Get post data before deletion to check for image
            DocumentSnapshot document = querySnapshot.Documents[0];
            SocialMediaPost post = document.ConvertTo<SocialMediaPost>();
        
            // Delete the document from Firestore
            DocumentReference docRef = document.Reference;
            await docRef.DeleteAsync();
        
            // Delete associated image if present
            if (!string.IsNullOrEmpty(post.PostImageUrl))
            {
                await _fileUploadService.DeletePostImageAsync(post.PostImageUrl);
            }
        
            _logger.LogInformation($"Post {postId} was successfully deleted");
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex.Message);
            throw;
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            _logger.LogError(ex, $"Unexpected error while deleting post {postId}: {ex.Message}");
            throw;
        }
    }

public async Task UpdatePostContent(string postId, string postContent)
{
    if (string.IsNullOrWhiteSpace(postId))
    {
        throw new ArgumentException("Post ID cannot be null or empty", nameof(postId));
    }
    
    if (string.IsNullOrWhiteSpace(postContent))
    {
        throw new ArgumentException("Post content cannot be null or empty", nameof(postContent));
    }
    
    try
    {
        // Find the document with matching PostId
        Query query = _db.Collection("posts").WhereEqualTo("PostId", postId);
        QuerySnapshot querySnapshot = await query.GetSnapshotAsync();
        
        if (querySnapshot.Count == 0)
        {
            throw new KeyNotFoundException($"Post with ID {postId} was not found");
        }
        
        // Update the document
        DocumentReference docRef = querySnapshot.Documents[0].Reference;
        await docRef.UpdateAsync("PostContent", postContent);
        _logger.LogInformation($"Content for post {postId} was successfully updated");
    }
    catch (KeyNotFoundException ex)
    {
        _logger.LogWarning(ex.Message);
        throw;
    }
    catch (Exception ex) when (ex is not ArgumentException)
    {
        _logger.LogError(ex, $"Unexpected error while updating post {postId}: {ex.Message}");
        throw;
    }
}
    
}