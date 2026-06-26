namespace VK_Trading_Lab_Auto.Models
{
    public class TradingViewSignal
    {
        public string Secret { get; set; }

        public string Symbol { get; set; }

        public string Signal { get; set; }

        public decimal Entry { get; set; }

        public decimal StopLoss { get; set; }

        public decimal Target { get; set; }

        public string ChannelType { get; set; }
    }
}
