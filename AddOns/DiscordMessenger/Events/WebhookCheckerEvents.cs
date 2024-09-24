using NinjaTrader.Custom.AddOns.DiscordMessenger.Configs;
using System;

namespace NinjaTrader.Custom.AddOns.DiscordMessenger.Events
{
    public class WebhookCheckerEvents
    {
        private readonly EventManager _eventManager;
        public event Action OnStartWebhookChecker;
        public event Action OnStopWebhookChecker;
        public event Action<Status> OnWebhookStatusUpdated;

        public WebhookCheckerEvents(EventManager eventManager)
        {
            _eventManager = eventManager;
        }

        public void StartWebhookChecker()
        {
            _eventManager.InvokeEvent(OnStartWebhookChecker);
        }

        public void StopWebhookChecker()
        {
            _eventManager.InvokeEvent(OnStopWebhookChecker);
        }

        public void UpdateWebhookStatus(Status status)
        {
            _eventManager.InvokeEvent(OnWebhookStatusUpdated, status);
        }
    }
}
