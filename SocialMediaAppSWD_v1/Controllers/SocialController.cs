using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaAppSWD_v1.DataAccess;
using SocialMediaAppSWD_v1.Interfaces;
using SocialMediaAppSWD_v1.Models;

namespace SocialMediaAppSWD_v1.Controllers
{
    public class SocialController : Controller
    {
        private readonly FirestoreRepository _repo;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<SocialController> _logger;

        public SocialController(
            FirestoreRepository repo,
            IFileUploadService fileUploadService,
            ILogger<SocialController> logger)
        {
            _repo = repo;
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View(_repo.GetPosts().Result);
        }

        [Authorize]
        [Route("UploadImage")]
        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            try
            {
                if (image == null || image.Length == 0)
                {
                    return BadRequest("No file uploaded");
                }

                string imageUrl = await _fileUploadService.UploadFileAsync(image, null);
                
                // Return the URL of the uploaded image
                return Ok(new { imageUrl });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid file upload attempt: {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading image: {ex.Message}");
                return StatusCode(500, "Error uploading image");
            }
        }

        [Route("CreatePost")]
        [HttpPost]
        public async Task<IActionResult> CreatePost(SocialMediaPost post)
        {
            post.PostId = Guid.NewGuid().ToString();
            post.PostDate = DateTimeOffset.UtcNow;
            post.PostAuthor = User.Identity.Name;
            await _repo.AddPost(post);
            return RedirectToAction("Index", "Social");
        }

        [Authorize]
        [Route("DeletePost/{postId}")]
        [HttpPost]
        public async Task<IActionResult> DeleteMyPost(string postId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(postId))
                {
                    return BadRequest("Post ID is required");
                }

                // Get the post to verify ownership
                var posts = await _repo.GetPosts();
                var post = posts.FirstOrDefault(p => p.PostId == postId);
        
                if (post == null)
                {
                    return NotFound("Post not found");
                }
        
                // Check if the current user is the author of the post
                if (post.PostAuthor != User.Identity.Name)
                {
                    _logger.LogWarning($"User {User.Identity.Name} attempted to delete post {postId} created by {post.PostAuthor}");
                    return Forbid("You can only delete your own posts");
                }

                // Delete the post
                await _repo.DeletePost(postId);
                _logger.LogInformation($"Post {postId} successfully deleted by {User.Identity.Name}");
        
                return Ok(new { success = true, message = "Post deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting post {postId}: {ex.Message}");
                return StatusCode(500, "An error occurred while deleting the post");
            }
        }

        [Route("UpdatePost")]
        [HttpPut]
        public async Task<IActionResult> UpdatePost(IFormCollection form)
        {
            await _repo.UpdatePostContent(form["id"],form["content"]);
            return RedirectToAction("Index", "Social");
        }
    }
}