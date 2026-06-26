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

                    if (signal.Signal.Equals("BUY", StringComparison.OrdinalIgnoreCase))
                    {
                        xauEntry = signal.Entry - 3.5m;
                        xauSl = xauEntry - 12m;
                        xauTp1 = xauEntry + 12.5m;
                        xauTp2 = xauTp1 + 3.5m;
                    }
                    else
                    {
                        xauEntry = signal.Entry + 3.5m;
                        xauSl = xauEntry + 12m;
                        xauTp1 = xauEntry - 12.5m;
                        xauTp2 = xauTp1 - 3.5m;
                    }

                    message =
                       $"""
                        *{(signal.Signal == "BUY" ? "🟢" : "🔴")} XAUUSD {signal.Signal}*

                        🎯 Entry ➜ *{xauEntry:F2}*

                        🛑 Stop Loss ➜ *{xauSl:F2}*

                        💰 Take Profit 1 ➜ *{xauTp1:F2}*

                        💰 Take Profit 2 ➜ *{xauTp2:F2}*

                        ⚠️ _Risk Management Is Mandatory_

                        #VKTradingLab
                        """;

                    await _telegram.SendToXAUUSD(message);

                    break;

                case "VK_NIFTY_2026":

                    decimal niftyEntry = signal.Entry;
                    decimal niftySl;
                    decimal niftyTp;

                    if (signal.Signal.Equals("BUY", StringComparison.OrdinalIgnoreCase))
                    {
                        niftySl = niftyEntry - 70;
                        niftyTp = niftyEntry + 100;
                    }
                    else
                    {
                        niftySl = niftyEntry + 70;
                        niftyTp = niftyEntry - 100;
                    }

                    string premiumMessage =
                   $"""
                    ═══════════════════════

                    *{(signal.Signal == "BUY" ? "🟢" : "🔴")} NIFTY {(signal.Signal == "BUY" ? "CE" : "PE")} SIGNAL*

                    ═══════════════════════

                    🎯 *ENTRY*
                    `{niftyEntry:F2}`

                    🛑 *STOP LOSS*
                    `{niftySl:F2}`

                    💰 *TARGET*
                    `{niftyTp:F2}`

                    ═══════════════════════

                    ⚠️ _Risk Management Is Mandatory_

                    📊 _Wait for Entry Trigger_

                    #VKTradingLab
                    """;

                    string freeMessage =
                   $"""
                    ═══════════════════════

                    *{(signal.Signal == "BUY" ? "🟢" : "🔴")} NIFTY {(signal.Signal == "BUY" ? "CE" : "PE")} SIGNAL*

                    ═══════════════════════

                    🎯 *ENTRY*
                    `{niftyEntry:F2}`

                    🛑 *STOP LOSS*
                    `🔒 Premium Members Only`

                    💰 *TARGET*
                    `{niftyTp:F2}`

                    ═══════════════════════

                    🌟 _Want Accurate SL & Live Trade Management?_

                    👉 *Join VK Trading Lab Premium*

                    #VKTradingLab
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
    
}
}
