using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NoticeBoard.API.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : Controller
    {
        [HttpGet("login")]
        public IActionResult Login()
        {
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = "/auth/me"
            },
            GoogleDefaults.AuthenticationScheme);
        }

        // test
        [HttpGet("test-cookie")]
        [AllowAnonymous]
        public IActionResult TestCookie()
        {
            var cookie = Request.Cookies[".AspNetCore.Cookies"];

            return Ok(new { CookiePresent = cookie != null, CookieValue = cookie });
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value });

            return Ok(claims);
        }

        [HttpGet("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return Ok("Logged out");
        }
    }
}
