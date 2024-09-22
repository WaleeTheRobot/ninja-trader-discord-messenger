using NinjaTrader.Custom.AddOns.DiscordMessenger.Configs;
using NinjaTrader.Custom.AddOns.DiscordMessenger.Models;
using System;
using System.Collections.Generic;

namespace NinjaTrader.Custom.AddOns.DiscordMessenger
{
    public class EventManager
    {
        // TradingStatusService
        public event Action OnOrderEntryUpdated;
        public event Action<List<Position>, List<OrderEntry>> OnOrderEntryProcessed;

        // DiscordMessengerService


        // WebhookCheckerService
        public event Action OnStartWebhookChecker;
        public event Action OnStopWebhookChecker;
        public event Action<Status> OnWebhookStatusUpdated;

        // Debug
        public event Action<string> OnPrintMessage;

        #region TradingStatusService

        public void UpdateOrderEntry()
        {
            OnOrderEntryUpdated?.Invoke();
        }

        public void OrderEntryProcessed(List<Position> positions, List<OrderEntry> orderEntries)
        {
            OnOrderEntryProcessed?.Invoke(positions, orderEntries);
        }

        #endregion

        #region DiscordMessengerService



        #endregion

        #region WebhookCheckerService

        public void StartWebhookChecker()
        {
            OnStartWebhookChecker?.Invoke();
        }

        public void StopWebhookChecker()
        {
            OnStopWebhookChecker?.Invoke();
        }

        public void UpdateWebhookStatus(Status status)
        {
            OnWebhookStatusUpdated?.Invoke(status);
        }

        #endregion

        // Helps with debugging
        public void PrintMessage(string eventMessage)
        {
            OnPrintMessage?.Invoke(eventMessage);
        }
    }
}
