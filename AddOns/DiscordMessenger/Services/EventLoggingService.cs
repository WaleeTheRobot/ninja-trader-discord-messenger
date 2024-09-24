
using NinjaTrader.Custom.AddOns.DiscordMessenger.Events;
using NinjaTrader.Custom.AddOns.DiscordMessenger.Models;
using System.Collections.Generic;

namespace NinjaTrader.Custom.AddOns.DiscordMessenger.Services
{
    public class EventLoggingService
    {
        private readonly EventLoggingEvents _eventLoggingEvents;
        private List<EventLog> _eventLogs = new List<EventLog>();

        public EventLoggingService(EventLoggingEvents eventLoggingEvents)
        {
            _eventLoggingEvents = eventLoggingEvents;
            _eventLoggingEvents.OnSendRecentEvent += HandleOnRecentEvent;
        }

        private void HandleOnRecentEvent(EventLog eventLog)
        {
            _eventLogs.Add(eventLog);

            // Limit
            if (_eventLogs.Count > 5)
            {
                _eventLogs.RemoveAt(0);
            }

            _eventLoggingEvents.RecentEventProcessed(_eventLogs);
        }
    }
}
