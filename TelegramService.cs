namespace VK_Trading_Lab_Auto
{
    using global::VK_Trading_Lab_Auto.Models;
    using Microsoft.Extensions.Options;
    using System.Net.Http.Json;

    public class TelegramService
    {
        private readonly IHttpClientFactory _factory;
        private readonly TelegramSettings _settings;

        public TelegramService(
            IHttpClientFactory factory,
            IOptions<TelegramSettings> options)
        {
            _factory = factory;
            _settings = options.Value;
            
        }

        public async Task SendMessage(
            string chatId,
            string message)
        {
            var client = _factory.CreateClient();

            string url =
                $"https://api.telegram.org/bot{_settings.BotToken}/sendMessage";

            var response =
                await client.PostAsJsonAsync(
                    url,
                    new
                    {
                        chat_id = chatId,
                        text = message,
                        parse_mode = "Markdown"
                    });

            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

        public async Task SendToXAUUSD(string message)
        {
            await SendMessage(
                _settings.XAUUSDChatId,
                message);
        }

        public async Task SendToNifty_SensexPremium(string message)
        {
            await SendMessage(
                _settings.Nifty_SensexPremiumChatId,
                message);
        }

        public async Task SendToNifty_SensexFree(string message)
        {
            await SendMessage(
                _settings.Nifty_SensexFreeChatId,
                message);
        }
    }
}
