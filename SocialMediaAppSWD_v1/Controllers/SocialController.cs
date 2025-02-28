using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaAppSWD_v1.DataAccess;
using SocialMediaAppSWD_v1.Models;

namespace SocialMediaAppSWD_v1.Controllers
{
    public class SocialController : Controller
    {
        private FirestoreRepository _repo;
        public SocialController(FirestoreRepository repo) {
            _repo = repo;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View(_repo.GetPosts().Result);
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

        [Route("DeletePost")]
        [HttpDelete]
        public async Task<IActionResult> DeletePost(IFormCollection form)
        {
            await _repo.DeletePost(form["id"]);
            return RedirectToAction("Index", "Social"); 
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
