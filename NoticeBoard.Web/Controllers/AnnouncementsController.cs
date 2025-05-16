using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NoticeBoard.Web.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace NoticeBoard.Web.Controllers
{
    public class AnnouncementsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AnnouncementsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _configuration = configuration;
        }

        public async Task<IActionResult> AuthCallback(string token)
        {
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Index");

            var handler = new JwtSecurityTokenHandler();
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };

            try
            {
                var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme, principal,
                    new AuthenticationProperties { IsPersistent = true });

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync("announcements");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var announcements = JsonConvert.DeserializeObject<List<AnnouncementViewModel>>(json);

            return View(announcements);
        }
    }
}
