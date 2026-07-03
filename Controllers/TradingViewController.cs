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
        private readonly TelegramService _telegram;


        public TradingViewController(
            IHttpClientFactory httpClientFactory,
            IOptions<TelegramSettings> options, TelegramService telegram)
        {
            _httpClientFactory = httpClientFactory;
            _settings = options.Value;
            _telegram = telegram;
        }

        [HttpPost]
        public async Task<IActionResult> Receive([FromBody] TradingViewSignal signal)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new
                    {
                        Field = x.Key,
                        Errors = x.Value.Errors.Select(e => e.ErrorMessage)
                    });

                return BadRequest(errors);
            }

            Console.WriteLine($"ALERT RECEIVED | {signal.Signal} | {signal.Entry} | {signal.Symbol}");

            if (string.IsNullOrWhiteSpace(signal.Secret))
            {
                return Unauthorized();
            }

            string message;

            switch (signal.Secret)
            {
                case "VK_XAU_2026":

                    decimal xauEntry;
                    decimal xauSl;
                    decimal xauTp1;
                    decimal xauTp2;
                    decimal xauTp3;

                    if (signal.Signal.Equals("BUY", StringComparison.OrdinalIgnoreCase))
                    {
                        xauEntry = signal.Entry - 2.5m;
                        xauSl = xauEntry - 13.0m;
                        xauTp1 = xauEntry + 7.5m;
                        xauTp2 = xauEntry + 12.0m;
                        xauTp3 = xauTp1 + 5.0m;
                    }
                    else
                    {
                        xauEntry = signal.Entry + 2.5m;
                        xauSl = xauEntry + 13.0m;
                        xauTp1 = xauEntry - 7.5m;
                        xauTp2 = xauEntry - 12.0m;
                        xauTp3 = xauTp1 - 5.0m;
                    }

                    // Round values for Telegram
                    xauEntry = RoundForTelegram(xauEntry);
                    xauSl = RoundForTelegram(xauSl);
                    xauTp1 = RoundForTelegram(xauTp1);
                    xauTp2 = RoundForTelegram(xauTp2);
                    xauTp3 = RoundForTelegram(xauTp3);

                    message =
                       $"""
                        *{(signal.Signal == "BUY" ? "🟢" : "🔴")} XAUUSD {signal.Signal}*

                        🎯 Entry ➜ *{xauEntry:0.##}*

                        🛑 Stop Loss ➜ *{xauSl:0.##}*

                        💰 Take Profit 1 ➜ *{xauTp1:0.##}*

                        💰 Take Profit 2 ➜ *{xauTp2:0.##}*

                        💰 Take Profit 3 ➜ *{xauTp3:0.##}*

                        ⚠️ _Risk Management Is Mandatory_

                        ⚠️ _If not triggered within 30 mins, Ignore the signal._

                        #VKTradingLab..✍
                        """;

                    await _telegram.SendToXAUUSD(message);

                    break;

                case "VK_NIFTY_2026":

                    decimal niftyEntry = signal.Entry;
                    decimal niftySl;
                    decimal niftyTp;

                    if (signal.Signal.Equals("BUY", StringComparison.OrdinalIgnoreCase))
                    {
                        niftySl = niftyEntry - 67;
                        niftyTp = niftyEntry + 97;
                    }
                    else
                    {
                        niftySl = niftyEntry + 67;
                        niftyTp = niftyEntry - 97;
                    }

                    string premiumMessage =
                   $"""
                    *{(signal.Signal == "BUY" ? "🟢" : "🔴")} NIFTY {(signal.Signal == "BUY" ? "CE" : "PE")} SIGNAL*
                    ═══════════════════════

                    🎯 *ENTRY* ➜ *{niftyEntry:F2}* 

                    🛑 *STOP LOSS* ➜ *{niftySl:F2}* 

                    💰 *TARGET*  ➜ *{niftyTp:F2}* 

                    ═══════════════════════

                    ⚠️ _Risk Management Is Mandatory_

                    📊 _Wait for Entry Trigger_

                    #VKTradingLab..✍
                    """;

                    string freeMessage =
                   $"""
                    *{(signal.Signal == "BUY" ? "🟢" : "🔴")} NIFTY {(signal.Signal == "BUY" ? "CE" : "PE")} SIGNAL*
                    ═══════════════════════

                    🎯 *ENTRY* ➜ *{niftyEntry:F2}* 

                    🛑 *STOP LOSS* ➜ `🔒 Premium 🔒`

                    💰 *TARGET* ➜ *{niftyTp:F2}* 

                    ═══════════════════════

                    🌟 _Want Accurate SL & Live Trade Management?_

                    👇🏻 *Join VK Trading Lab Premium* 👇🏻

                    👉*https://cosmofeed.com/vig/69b245b75079310013132506*

                    #VKTradingLab..✍
                    """;

                    await Task.WhenAll(
                        _telegram.SendToNifty_SensexPremium(premiumMessage),
                        _telegram.SendToNifty_SensexFree(freeMessage));

                    break;

                default:
                    return Unauthorized("Invalid Secret");
            }

            Console.WriteLine("TELEGRAM SENT");

            return Ok();
        }

        [HttpGet("test-alert")]
        public async Task<IActionResult> TestAlert()
        {
            var testSignal = new TradingViewSignal
            {
                Secret = "VK_XAU_2026",
                Signal = "BUY",
                Entry = 3400,
                Symbol = "XAUUSD"
            };

            return await Receive(testSignal);
        }

        [HttpGet("test-premium")]
        public async Task<IActionResult> TestPremium()
        {
            var testSignal = new TradingViewSignal
            {
                Secret = "VK_NIFTY_2026",
                Signal = "BUY",
                Entry = 24580,
                Symbol = "NIFTY"
            };

            return await Receive(testSignal);
        }

        [HttpGet("test-free")]
        public async Task<IActionResult> TestFree()
        {
            var testSignal = new TradingViewSignal
            {
                Secret = "VK_NIFTY_2026",
                Signal = "BUY",
                Entry = 24580,
                Symbol = "NIFTY"
            };

            return await Receive(testSignal);
        }

        [HttpGet("test-all")]
        public async Task<IActionResult> TestAll()
        {
            var testSignal = new TradingViewSignal
            {
                Secret = "VK_NIFTY_2026",
                Signal = "BUY",
                Entry = 24580,
                Symbol = "NIFTY"
            };

            return await Receive(testSignal);
        }

        private decimal RoundForTelegram(decimal value)
        {
            decimal fraction = value % 1;

            if (fraction >= 0.49m && fraction <= 0.51m)
                return Math.Floor(value) + 0.5m;

            return fraction > 0.5m
                ? Math.Ceiling(value)
                : Math.Floor(value);
        }

    }
}
