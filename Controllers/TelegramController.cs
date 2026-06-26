using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VK_Trading_Lab_Auto.Models;

namespace VK_Trading_Lab_Auto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelegramController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TelegramSettings _settings;

        public TelegramController(IHttpClientFactory httpClientFactory, IOptions<TelegramSettings> options)
        {
            _httpClientFactory = httpClientFactory;
            _settings = options.Value;
        }

        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            var httpClient = _httpClientFactory.CreateClient();

            string botToken = _settings.BotToken;
            string chatId = _settings.XAUUSDChatId;

            string message = "🚀 VK Trading Lab Test Signal";

            string url =
                $"https://api.telegram.org/bot{botToken}/sendMessage";

            var payload = new
            {
                chat_id = chatId,
                text = message
            };

            var response =
                await httpClient.PostAsJsonAsync(url, payload);

            var result =
                await response.Content.ReadAsStringAsync();

            return Ok(result);
        }
    }
}
