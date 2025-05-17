using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        private readonly ILogger<AnnouncementsController> _logger;

        public AnnouncementsController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<AnnouncementsController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _configuration = configuration;
            _logger = logger;
        }

        private static readonly Dictionary<string, List<string>> CategoryMap = new()
        {
            ["Побутова техніка"] = new() { "Холодильники", "Пральні машини", "Бойлери", "Печі", "Витяжки", "Мікрохвильові печі" },
            ["Комп'ютерна техніка"] = new() { "ПК", "Ноутбуки", "Монітори", "Принтери", "Сканери" },
            ["Смартфони"] = new() { "Android смартфони", "iOS/Apple смартфони" },
            ["Інше"] = new() { "Одяг", "Взуття", "Аксесуари", "Спортивне обладнання", "Іграшки" }
        };

        private void PopulateDropdowns(CreateAnnouncementViewModel model)
        {
            model.Categories = CategoryMap.Keys
                .Select(c => new SelectListItem { Value = c, Text = c, Selected = c == model.Category })
                .ToList();

            model.SubCategories = !string.IsNullOrEmpty(model.Category) && CategoryMap.ContainsKey(model.Category)
                ? CategoryMap[model.Category].Select(sub => new SelectListItem { Value = sub, Text = sub, Selected = sub == model.SubCategory }).ToList()
                : new List<SelectListItem>();
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

                Response.Cookies.Append("ApiJwt", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax
                });

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

        public async Task<IActionResult> Index(string? category, string? subcategory)
        {
            ViewBag.ApiBaseUrl = _configuration["ApiBaseUrl"];

            var response = await _httpClient.GetAsync("announcements");
            Console.WriteLine(">>> URL: " + _httpClient.BaseAddress + "announcements");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var announcements = JsonConvert.DeserializeObject<List<AnnouncementViewModel>>(json);

            ViewBag.Categories = announcements.Select(a => a.Category).Distinct().ToList();
            ViewBag.SubCategories = announcements
                .Where(a => string.IsNullOrEmpty(category) || a.Category == category)
                .Select(a => a.SubCategory)
                .Distinct()
                .ToList();

            if (!string.IsNullOrEmpty(category))
                announcements = announcements.Where(a => a.Category == category).ToList();

            if (!string.IsNullOrEmpty(subcategory))
                announcements = announcements.Where(a => a.SubCategory == subcategory).ToList();

            return View(announcements);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            ViewBag.ApiBaseUrl = _configuration["ApiBaseUrl"];

            var response = await _httpClient.GetAsync($"announcements/{id}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var announcement = JsonConvert.DeserializeObject<AnnouncementViewModel>(json);

            if (announcement == null)
                return NotFound();

            return View(announcement);
        }

        public IActionResult Create()
        {
            var model = new CreateAnnouncementViewModel();
            PopulateDropdowns(model);

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAnnouncementViewModel model, string action)
        {
            if (action == "categoryChange")
            {
                PopulateDropdowns(model);
                ModelState.Clear();

                return View(model);
            }

            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        _logger.LogWarning("Model error in {Field}: {Error}", state.Key, error.ErrorMessage);
                    }
                }

                PopulateDropdowns(model);

                return View(model);
            }

            var apiModel = new
            {
                title = model.Title,
                description = model.Description,
                status = model.Status,
                category = model.Category,
                subCategory = model.SubCategory
            };

            var json = JsonConvert.SerializeObject(apiModel);
            _logger.LogInformation("[WEB] Sent JSON: {Json}", json);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var jwt = Request.Cookies["ApiJwt"];
            if (!string.IsNullOrEmpty(jwt))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
            }

            var response = await _httpClient.PostAsync("announcements", content);

            _httpClient.DefaultRequestHeaders.Authorization = null;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var created = JsonConvert.DeserializeObject<dynamic>(responseContent);
                Guid createdId = created.id;

                return RedirectToAction("Details", new { id = createdId });
            }

            ModelState.AddModelError(string.Empty, "Failed to create announcement.");
            PopulateDropdowns(model);

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var jwt = Request.Cookies["ApiJwt"];

            if (!string.IsNullOrEmpty(jwt))
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

            var response = await _httpClient.DeleteAsync($"announcements/{id}");
            _httpClient.DefaultRequestHeaders.Authorization = null;

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            ModelState.AddModelError(string.Empty, "Failed to delete announcement.");

            return RedirectToAction("Details", new { id });
        }

        [Authorize]
        public async Task<IActionResult> Edit(Guid id)
        {
            var response = await _httpClient.GetAsync($"announcements/{id}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var announcement = JsonConvert.DeserializeObject<AnnouncementViewModel>(json);

            if (announcement == null)
                return NotFound();

            var model = new CreateAnnouncementViewModel
            {
                Title = announcement.Title,
                Description = announcement.Description,
                Status = announcement.Status,
                Category = announcement.Category,
                SubCategory = announcement.SubCategory
            };

            PopulateDropdowns(model);
            ViewBag.AnnouncementId = id;

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CreateAnnouncementViewModel model, string action)
        {
            if (action == "categoryChange")
            {
                PopulateDropdowns(model);
                ModelState.Clear();
                ViewBag.AnnouncementId = id;

                return View(model);
            }

            if (!ModelState.IsValid)
            {
                PopulateDropdowns(model);
                ViewBag.AnnouncementId = id;

                return View(model);
            }

            var apiModel = new
            {
                title = model.Title,
                description = model.Description,
                status = model.Status,
                category = model.Category,
                subCategory = model.SubCategory
            };

            var json = JsonConvert.SerializeObject(apiModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var jwt = Request.Cookies["ApiJwt"];
            if (!string.IsNullOrEmpty(jwt))
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

            var response = await _httpClient.PutAsync($"announcements/{id}", content);
            _httpClient.DefaultRequestHeaders.Authorization = null;

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Details", new { id });

            ModelState.AddModelError(string.Empty, "Failed to update announcement.");
            PopulateDropdowns(model);
            ViewBag.AnnouncementId = id;

            return View(model);
        }
    }
}
