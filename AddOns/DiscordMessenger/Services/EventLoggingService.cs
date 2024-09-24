
using NinjaTrader.Custom.AddOns.DiscordMessenger.Models;
using System.Collections.Generic;

namespace NinjaTrader.Custom.AddOns.DiscordMessenger.Services
{
    public class EventLoggingService
    {
        private readonly EventManager _eventManager;
        private List<EventLog> _eventLogs = new List<EventLog>();

        public EventLoggingService(EventManager eventManager)
        {
            _eventManager = eventManager;
            _eventManager.OnSendRecentEvent += HandleOnRecentEvent;
        }

        private void HandleOnRecentEvent(EventLog eventLog)
        {
            _eventLogs.Add(eventLog);

            // Limit
            if (_eventLogs.Count > 5)
            {
                _eventLogs.RemoveAt(0);
            }

            _eventManager.RecentEventProcessed(_eventLogs);
        }
    }
}
