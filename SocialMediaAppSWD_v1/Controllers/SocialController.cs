using Microsoft.AspNetCore.Mvc;

namespace SocialMediaAppSWD_v1.Controllers
{
    public class SocialController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
