using NinjaTrader.Custom.AddOns.DiscordMessenger.Configs;
using System;

namespace NinjaTrader.Custom.AddOns.DiscordMessenger.Models
{
    public class EventLog
    {
        public Status Status { get; set; }
        public DateTime Time { get; set; }
        public string Message { get; set; }

        public EventLog()
        {
            Time = DateTime.Now;
        }
    }
}
