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
                case "VK_XAU_EMA":

                    var emaSignal = signal.Signal?.Trim().ToUpperInvariant();

                    if (emaSignal != "BUY" && emaSignal != "SELL")
                    {
                        return BadRequest("Invalid EMA signal. Expected BUY or SELL.");
                    }

                    decimal entry1;
                    decimal entry2;
                    decimal sl;
                    decimal tp1;
                    decimal tp2;
                    decimal tp3;
                    decimal tp4;

                    if (emaSignal == "BUY")
                    {
                        entry1 = signal.Entry - 1.5m;
                        entry2 = signal.Entry - 4.0m;
                        entry1 = RoundForTelegram(entry1);
                        entry2 = RoundForTelegram(entry2);

                        sl = entry1 - 9.0m;
                        tp1 = entry1 + 8.0m;
                        tp2 = entry1 + 13.0m;
                        tp3 = entry1 + 18.0m;
                        tp4 = entry1 + 23.0m;
                    }
                    else
                    {
                        entry1 = signal.Entry + 1.5m;
                        entry2 = signal.Entry + 4.0m;
                        entry1 = RoundForTelegram(entry1);
                        entry2 = RoundForTelegram(entry2);

                        sl = entry1 + 9.0m;
                        tp1 = entry1 - 8.0m;
                        tp2 = entry1 - 13.0m;
                        tp3 = entry1 - 18.0m;
                        tp4 = entry1 - 23.0m;
                    }

                    message =
                    $"""
                        *{(emaSignal == "BUY" ? "🟢" : "🔴")} #XAUUSD {emaSignal}*
                        🎯 Entry Zone ➜ *{entry1:0.##}* - *{entry2:0.##}*

                        💰 TP 1 ➜ *{tp1:0.##}*
                        💰 TP 2 ➜ *{tp2:0.##}*
                        💰 TP 3 ➜ *{tp3:0.##}*
                        💰 TP 4 ➜ *{tp4:0.##}*

                        🛑 SL ➜ *{sl:0.##}*

                        ⚠️ _Risk Management is Mandatory_
                        🙏 _Use Correct Lot SIZE Based On Your CAPITAL_
                        📊 *Strategy* ➜ __E20M30A__

                        #VKTradingLab..✍
                     """;

                    await _telegram.SendToXAUUSD(message);

                    Console.WriteLine($"EMA {emaSignal} TELEGRAM SENT");

                    break;

                case "VK_XAU_STACK":

                    var signalType = signal.Signal?.Trim().ToUpperInvariant();

                    // CLOSE BUY
                    // Green -> Red
                    if (signalType == "CLOSE_BUY")
                    {
                        message =
                           $"""
                            *🔴 #XAUUSD — CLOSE BUY*

                            ⚠️ _Close any existing BUY trade quickly._
                            🔄 _Trend changed from Bullish → Bearish_
                            📍 Closing Price ➜ *{signal.Entry:0.##}*

                            #VKTradingLab..✍
                            """;

                        await _telegram.SendToXAUUSD(message);

                        Console.WriteLine("CLOSE BUY TELEGRAM SENT");

                        return Ok(new
                        {
                            success = true,
                            message = "Close Buy alert sent"
                        });
                    }

                    // CLOSE SELL
                    // Red -> Green
                    if (signalType == "CLOSE_SELL")
                    {
                        message =
                           $"""
                            *🟢 #XAUUSD — CLOSE SELL*

                            ⚠️ _Close any existing SELL trade quickly._
                            🔄 _Trend changed from Bearish → Bullish_
                            📍 Closing Price ➜ *{signal.Entry:0.##}*

                            #VKTradingLab..✍
                            """;

                        await _telegram.SendToXAUUSD(message);

                        Console.WriteLine("CLOSE SELL TELEGRAM SENT");

                        return Ok(new
                        {
                            success = true,
                            message = "Close Sell alert sent"
                        });
                    }

                    decimal stackEntry1;
                    decimal stackEntry2;
                    decimal stackSl;
                    decimal stackTp1;
                    decimal stackTp2;
                    decimal stackTp3;
                    decimal stackTp4;

                    if (signal.Signal.Equals("BUY", StringComparison.OrdinalIgnoreCase))
                    {
                        stackEntry1 = signal.Entry - 2.37m;
                        stackEntry1 = RoundForTelegram(stackEntry1);
                        stackEntry2 = signal.Entry - 5.0m;
                        stackEntry2 = RoundForTelegram(stackEntry2);
                        stackSl = stackEntry1 - 10.5m;
                        stackTp1 = stackEntry1 + 8.0m;
                        stackTp2 = stackEntry1 + 13.0m;
                        stackTp3 = stackEntry1 + 17.0m;
                        stackTp4 = stackEntry1 + 21.0m;
                    }
                    else
                    {
                        stackEntry1 = signal.Entry + 2.37m;
                        stackEntry1 = RoundForTelegram(stackEntry1);
                        stackEntry2 = signal.Entry + 5.0m;
                        stackEntry2 = RoundForTelegram(stackEntry2);
                        stackSl = stackEntry1 + 10.5m;
                        stackTp1 = stackEntry1 - 8.0m;
                        stackTp2 = stackEntry1 - 13.0m;
                        stackTp3 = stackEntry1 - 17.0m;
                        stackTp4 = stackEntry1 - 21.0m;
                    }

                    message =
                    $"""
                    *{(signal.Signal.Equals("BUY", StringComparison.OrdinalIgnoreCase) ? "🟢" : "🔴")} XAUUSD {signal.Signal.ToUpperInvariant()}*
                    🎯 Entry Zone ➜ *{stackEntry1:0.##}* - *{stackEntry2:0.##}*

                    💰 TP 1 ➜ *{stackTp1:0.##}*
                    💰 TP 2 ➜ *{stackTp2:0.##}*
                    💰 TP 3 ➜ *{stackTp3:0.##}*
                    💰 TP 4 ➜ *{stackTp4:0.##}*

                    🛑 SL ➜ *{stackSl:0.##}*

                    ⚠️ _Risk Management is Mandatory_
                    🙏 _Use Correct Lot SIZE Based On Your CAPITAL_

                    #VKTradingLab..✍
                    """;

                    await _telegram.SendToXAUUSD(message);

                    break;

                //case "VK_NIFTY_2026":

                //    decimal niftyEntry = signal.Entry;
                //    decimal niftySl;
                //    decimal niftyTp;

                //    if (signal.Signal.Equals("BUY", StringComparison.OrdinalIgnoreCase))
                //    {
                //        niftySl = niftyEntry - 67;
                //        niftyTp = niftyEntry + 97;
                //    }
                //    else
                //    {
                //        niftySl = niftyEntry + 67;
                //        niftyTp = niftyEntry - 97;
                //    }

                //    string premiumMessage =
                //   $"""
                //    *{(signal.Signal == "BUY" ? "🟢" : "🔴")} NIFTY {(signal.Signal == "BUY" ? "CE" : "PE")} SIGNAL*
                //    ═══════════════════════

                //    🎯 *ENTRY* ➜ *{niftyEntry:F2}* 

                //    🛑 *STOP LOSS* ➜ *{niftySl:F2}* 

                //    💰 *TARGET*  ➜ *{niftyTp:F2}* 

                //    ═══════════════════════

                //    ⚠️ _Risk Management Is Mandatory_

                //    📊 _Wait for Entry Trigger_

                //    #VKTradingLab..✍
                //    """;

                //    string freeMessage =
                //   $"""
                //    *{(signal.Signal == "BUY" ? "🟢" : "🔴")} NIFTY {(signal.Signal == "BUY" ? "CE" : "PE")} SIGNAL*
                //    ═══════════════════════

                //    🎯 *ENTRY* ➜ *{niftyEntry:F2}* 

                //    🛑 *STOP LOSS* ➜ `🔒 Premium 🔒`

                //    💰 *TARGET* ➜ *{niftyTp:F2}* 

                //    ═══════════════════════

                //    🌟 _Want Accurate SL & Live Trade Management?_

                //    👇🏻 *Join VK Trading Lab Premium* 👇🏻

                //    👉*https://cosmofeed.com/vig/69b245b75079310013132506*

                //    #VKTradingLab..✍
                //    """;

                //    await Task.WhenAll(
                //        _telegram.SendToNifty_SensexPremium(premiumMessage),
                //        _telegram.SendToNifty_SensexFree(freeMessage));

                //    break;

                default:
                    return Unauthorized("Invalid Secret");
            }

            Console.WriteLine("TELEGRAM SENT");

            return Ok();
        }

        [HttpGet("test-stack")]
        public async Task<IActionResult> TestStack()
        {
            var testSignal = new TradingViewSignal
            {
                Secret = "VK_XAU_STACK",
                Signal = "BUY",
                Entry = 4107,
                Symbol = "XAUUSD"
            };

            return await Receive(testSignal);
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
