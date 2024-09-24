namespace NinjaTrader.Custom.AddOns.DiscordMessenger.Models
{
    public class Position
    {
        public string Instrument { get; set; }
        public int Quantity { get; set; }
        public double AveragePrice { get; set; }
        public string MarketPosition { get; set; }
    }

    public class OrderEntry
    {
        public string Instrument { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public string Type { get; set; }
        public string Action { get; set; }
    }
}
