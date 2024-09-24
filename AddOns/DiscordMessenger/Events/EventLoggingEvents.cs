using NinjaTrader.Custom.AddOns.DiscordMessenger.Models;
using System;
using System.Collections.Generic;

namespace NinjaTrader.Custom.AddOns.DiscordMessenger.Events
{
    public class EventLoggingEvents
    {
        private readonly EventManager _eventManager;
        public event Action<EventLog> OnSendRecentEvent;
        public event Action<List<EventLog>> OnRecentEventProcessed;

        public EventLoggingEvents(EventManager eventManager)
        {
            _eventManager = eventManager;
        }

        public void SendRecentEvent(EventLog eventLog)
        {
            _eventManager.InvokeEvent(OnSendRecentEvent, eventLog);
        }

        public void RecentEventProcessed(List<EventLog> eventLogs)
        {
            _eventManager.InvokeEvent(OnRecentEventProcessed, eventLogs);
        }
    }
}
