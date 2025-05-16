using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NoticeBoard.Web.Models;

namespace NoticeBoard.Web.Controllers
{
    public class AnnouncementsController : Controller
    {
        private readonly HttpClient _httpClient;

        public AnnouncementsController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
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
