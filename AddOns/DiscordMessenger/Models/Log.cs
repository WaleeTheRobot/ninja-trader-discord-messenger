using System;

namespace NinjaTrader.Custom.AddOns.DiscordMessenger.Models
{
    public class EventLog
    {
        public string Status { get; set; }
        public DateTime Time { get; set; }
        public string Message { get; set; }
    }
}
