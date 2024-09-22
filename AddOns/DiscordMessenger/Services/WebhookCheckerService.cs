using NinjaTrader.Custom.AddOns.DiscordMessenger.Configs;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace NinjaTrader.Custom.AddOns.DiscordMessenger.Services
{
    public class WebhookCheckerService
    {
        private readonly EventManager _eventManager;
        private HttpClient _httpClient;
        private Timer _timer;

        private List<string> _webhookUrls;

        public WebhookCheckerService(EventManager eventManager)
        {
            _eventManager = eventManager;
            _eventManager.OnStartWebhookChecker += HandleStartWebhookChecker;
            _eventManager.OnStopWebhookChecker += HandleStopWebhookChecker;
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
            int successCount = 0;
            int failCount = 0;

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
                        failCount++;
                    }
                }
                catch
                {
                    failCount++;
                }
            }

            Status currentStatus;

            if (successCount == _webhookUrls.Count)
            {
                currentStatus = Status.Success;
            }
            else if (failCount == _webhookUrls.Count)
            {
                currentStatus = Status.Failed;
            }
            else
            {
                currentStatus = Status.PartialSuccess;
            }

            _eventManager.UpdateWebhookStatus(currentStatus);
        }
    }
}
