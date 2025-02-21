using Google.Cloud.Firestore;
using SocialMediaAppSWD_v1.Models;

namespace SocialMediaAppSWD_v1.DataAccess;

public class FirestoreRepository
{
    private readonly ILogger<FirestoreRepository> _logger;
    private FirestoreDb _db;
    
    public FirestoreRepository(ILogger<FirestoreRepository> logger, IConfiguration configuration)
    {
        _logger = logger;
        _db = FirestoreDb.Create(configuration.GetValue<string>("Authentication:Google:ProjectId"));
    }

    public async void AddPost(SocialMediaPost post)
    {
        await _db.Collection("posts").AddAsync(post);
        _logger.LogInformation($"Post {post.PostId} added to Firestore");
    }
}