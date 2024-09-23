using NinjaTrader.Custom.AddOns.DiscordMessenger.Configs;
using NinjaTrader.Custom.AddOns.DiscordMessenger.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        // ControlPanel
        public event Action<Status> OnUpdateStatus;
        public event Action<EventLog> OnUpdateEventLog;
        public event Action<bool> OnAutoButtonClicked;
        public event Func<ProcessType, string, Task> OnTakeScreenshot;
        public event Func<ProcessType, string, Task> OnScreenshotProcessed;
        public event Action OnAutoScreenshotProcessedWaiting;

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

        #region ControlPanel

        public void UpdateEventLog(EventLog eventLog)
        {
            OnUpdateEventLog?.Invoke(eventLog);
        }

        public void UpdateStatus(Status status)
        {
            OnUpdateStatus?.Invoke(status);
        }

        public void AutoButtonClicked(bool isEnabled)
        {
            OnAutoButtonClicked?.Invoke(isEnabled);
        }

        public void TakeScreenshot(ProcessType processType, string screenshotName)
        {
            OnTakeScreenshot?.Invoke(processType, screenshotName);
        }

        public void ScreenshotProcessed(ProcessType processType, string screenshotName)
        {
            OnScreenshotProcessed?.Invoke(processType, screenshotName);
        }

        // Screenshot done and waiting for auto processing
        public void AutoScreenshotProcessedWaiting()
        {
            OnAutoScreenshotProcessedWaiting?.Invoke();
        }

        #endregion

        // Helps with debugging
        public void PrintMessage(string eventMessage)
        {
            OnPrintMessage?.Invoke(eventMessage);
        }
    }
}
