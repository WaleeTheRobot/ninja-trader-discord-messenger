using NinjaTrader.Custom.AddOns.DiscordMessenger.Configs;
using NinjaTrader.Custom.AddOns.DiscordMessenger.Events;
using NinjaTrader.Custom.AddOns.DiscordMessenger.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace NinjaTrader.Custom.AddOns.DiscordMessenger.Services
{
    public class WebhookCheckerService
    {
        private readonly EventManager _eventManager;
        private readonly WebhookCheckerEvents _webhookCheckerEvents;
        private readonly EventLoggingEvents _eventLoggingEvents;
        private HttpClient _httpClient;
        private Timer _timer;

        private List<string> _webhookUrls;

        public WebhookCheckerService(
            EventManager eventManager,
            WebhookCheckerEvents webhookCheckerEvents,
            EventLoggingEvents eventLoggingEvents
            )
        {
            _eventManager = eventManager;

            _webhookCheckerEvents = webhookCheckerEvents;
            _webhookCheckerEvents.OnStartWebhookChecker += HandleStartWebhookChecker;
            _webhookCheckerEvents.OnStopWebhookChecker += HandleStopWebhookChecker;

            _eventLoggingEvents = eventLoggingEvents;

            _httpClient = new HttpClient();

            _webhookUrls = Config.Instance.WebhookUrls;
        }

        private void HandleStartWebhookChecker()
        {
            _timer = new Timer(CheckWebhookStatus, null, 0, 60000);
        }

        private void HandleStopWebhookChecker()
        {
            _timer?.Dispose();
            _httpClient?.Dispose();
        }

        private async void CheckWebhookStatus(object state)
        {
            List<string> failedWebhookUrls = new List<string>();

            int successCount = 0;
            int failCount = 0;
            int totalWebhookUrls = _webhookUrls.Count;

            foreach (var webhookUrl in _webhookUrls)
            {
                try
                {
                    HttpResponseMessage response = await _httpClient.GetAsync(webhookUrl);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        successCount++;
                    }
                    else
                    {
                        failedWebhookUrls.Add(webhookUrl);
                        failCount++;
                    }
                }
                catch
                {
                    failedWebhookUrls.Add(webhookUrl);
                    failCount++;
                }
            }

            Status currentStatus;

            if (failCount == totalWebhookUrls)
            {
                currentStatus = Status.Failed;
            }
            else if (successCount == totalWebhookUrls)
            {
                currentStatus = Status.Success;
            }
            else
            {
                currentStatus = Status.PartialSuccess;

                foreach (var url in failedWebhookUrls)
                {
                    _eventManager.PrintMessage($"Webhook Failed: {url}");
                }

                _eventLoggingEvents.SendRecentEvent(new EventLog
                {
                    Status = Status.PartialSuccess,
                    Message = "Webhook Check Failed"
                });
            }

            _webhookCheckerEvents.UpdateWebhookStatus(currentStatus);
        }
    }
}
