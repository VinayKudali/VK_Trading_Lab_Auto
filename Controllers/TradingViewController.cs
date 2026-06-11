using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VK_Trading_Lab_Auto.Models;

namespace VK_Trading_Lab_Auto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TradingViewController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TelegramSettings _settings;

        public TradingViewController(
            IHttpClientFactory httpClientFactory,
            IOptions<TelegramSettings> options)
        {
            _httpClientFactory = httpClientFactory;
            _settings = options.Value;
        }

        [HttpPost]
        public async Task<IActionResult> Receive([FromBody] TradingViewXAUUSDSignal signal)
        {
            if (signal.Secret != "VK_XAU_2026")
            {
                return Unauthorized();
            }
            var httpClient = _httpClientFactory.CreateClient();

            decimal entry;
            decimal sl;
            decimal tp;

            if (signal.Signal?.ToUpper() == "BUY")
            {
                entry = signal.Price - 3.5m;
                sl = entry - 5.5m;
                tp = entry + 9.5m;
            }
            else
            {
                entry = signal.Price + 3.5m;
                sl = entry + 5.5m;
                tp = entry - 9.5m;
            }
            string message =
                    $"""
                    {(signal.Signal == "BUY" ? "🟢" : "🔴")} XAUUSD {signal.Signal}

                    Signal Price : {signal.Price:F2}

                    Entry : {entry:F2}
                    SL    : {sl:F2}
                    TP    : {tp:F2}

                    #VKTradingLab
                    """;
            string url =
                $"https://api.telegram.org/bot{_settings.BotToken}/sendMessage";

            var response = await httpClient.PostAsJsonAsync( url,
            new
            {
                chat_id = _settings.ChatId,
                text = message
            });

            var telegramResponse =
                await response.Content.ReadAsStringAsync();

            Console.WriteLine(telegramResponse);

            return Ok();
        }
    }
}
