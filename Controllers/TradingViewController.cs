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

            Console.WriteLine($"ALERT RECEIVED | {signal.Signal} | {signal.Price} | {signal.Symbol}");
            
            if (signal.Secret != "VK_XAU_2026")
            {
                return Unauthorized();
            }
            var httpClient = _httpClientFactory.CreateClient();

            decimal entry;
            decimal sl;
            decimal tp1;
            decimal tp2;

            if (signal.Signal?.ToUpper() == "BUY")
            {
                entry = signal.Price - 3.0m;
                sl = entry - 6.0m;
                tp1 = entry + 9.5m;
                tp2 = tp1 + 3.5m;
            }
            else
            {
                entry = signal.Price + 3.0m;
                sl = entry + 6.0m;
                tp1 = entry - 9.5m;
                tp2 = tp1 - 3.5m;
            }
            string message =
                    $"""
                    *{(signal.Signal == "BUY" ? "🟢" : "🔴")} XAUUSD {signal.Signal}*

                    🎯 Entry  ➜  *{entry:F2}*

                    🛑 Stop Loss  ➜  *{sl:F2}*

                    💰 Take Profit 1  ➜  *{tp1:F2}*

                    💰 Take Profit 2  ➜  *{tp2:F2}*

                    ⚠️ _Risk Management Is Mandatory_

                    #VKTradingLab
                    """;
            string url =
                $"https://api.telegram.org/bot{_settings.BotToken}/sendMessage";

            var response = await httpClient.PostAsJsonAsync(
                url,
                new
                {
                    chat_id = _settings.ChatId,
                    text = message,
                    parse_mode = "Markdown"
                });

            var telegramResponse =
                await response.Content.ReadAsStringAsync();

            Console.WriteLine(telegramResponse);
            Console.WriteLine("TELEGRAM SENT");

            return Ok();
        }

        [HttpGet("test-alert")]
        public async Task<IActionResult> TestAlert()
        {
            var testSignal = new TradingViewXAUUSDSignal
            {
                Secret = "VK_XAU_2026",
                Signal = "BUY",
                Price = 3400,
                Symbol = "XAUUSD"
            };

            return await Receive(testSignal);
        }
    }
}
