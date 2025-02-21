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
            return View();
        }

        [Route("CreatePost")]
        [HttpPost]
        public async Task<IActionResult> CreatePost(SocialMediaPost post)
        {
            post.PostId = Guid.NewGuid().ToString();
            post.PostDate = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            post.PostAuthor = User.Identity.Name;
            _repo.AddPost(post);
            return Index();
        }

        public static DateTimeOffset UnixTimeStampToDateTime(long unixTimeStamp)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTimeStamp);
        }
    }
}
