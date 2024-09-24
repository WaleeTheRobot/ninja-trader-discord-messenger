﻿using NinjaTrader.Custom.AddOns.DiscordMessenger.Models;
using System;
using System.Collections.Generic;

namespace NinjaTrader.Custom.AddOns.DiscordMessenger.Events
{
    public class TradingStatusEvents
    {
        private readonly EventManager _eventManager;
        public event Action OnOrderEntryUpdated;
        public event Action<List<Position>, List<OrderEntry>> OnOrderEntryProcessed;

        public TradingStatusEvents(EventManager eventManager)
        {
            _eventManager = eventManager;
        }

        public void UpdateOrderEntry()
        {
            _eventManager.InvokeEvent(OnOrderEntryUpdated);
        }

        public void OrderEntryProcessed(List<Position> positions, List<OrderEntry> orderEntries)
        {
            _eventManager.InvokeEvent(OnOrderEntryProcessed, positions, orderEntries);
        }
    }
}