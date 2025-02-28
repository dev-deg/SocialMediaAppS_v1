using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SocialMediaAppSWD_v1.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            AuthenticationProperties authenticationProperties = new AuthenticationProperties
            { RedirectUri = Url.Action("GoogleResponse") };
            return Challenge(authenticationProperties, GoogleDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var authenticationResult = await HttpContext.AuthenticateAsync
                (CookieAuthenticationDefaults.AuthenticationScheme);

            //If the user failed to login
            if (!authenticationResult.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            //If the user logged in successfully
            ClaimsIdentity claimsIdentity = new ClaimsIdentity
                (authenticationResult.Principal.Identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
