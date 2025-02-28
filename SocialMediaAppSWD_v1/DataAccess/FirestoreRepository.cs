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
        
    }
    public async Task UpdatePostContent(string postId, string postContent)
    {
        
    }
    
}