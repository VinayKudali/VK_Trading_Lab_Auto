namespace VK_Trading_Lab_Auto.Models
{
    public class TradingViewSignal
    {
        public required string Secret { get; set; }

        public required string Symbol { get; set; }

        public required string Signal { get; set; }

        public required decimal Entry { get; set; }

        public decimal? StopLoss { get; set; }

        public decimal? Target { get; set; }

        public string? ChannelType { get; set; }
    }
}
