﻿using NinjaTrader.Custom.AddOns.DiscordMessenger.Configs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using OrderEntry = NinjaTrader.Custom.AddOns.DiscordMessenger.Models.OrderEntry;
using Position = NinjaTrader.Custom.AddOns.DiscordMessenger.Models.Position;

namespace NinjaTrader.Custom.AddOns.DiscordMessenger.Services
{
    public class DiscordMessengerService
    {
        private readonly EventManager _eventManager;
        private readonly HttpClient _httpClient;

        private List<string> _webhookUrls;
        private string _screenshotLocation;

        private string _screenshotPath;
        private object _embedContent;

        private int _finalEmbedColor;
        private Assembly _newtonsoftJsonAssembly;
        private Type _jsonConvertType;

        public DiscordMessengerService(EventManager eventManager)
        {
            _eventManager = eventManager;
            _eventManager.OnOrderEntryProcessed += HandleOnOrderEntryProcessed;
            _eventManager.OnScreenshotProcessed += HandleOnScreenshotProcessed;
            _eventManager.OnAutoScreenshotProcessedWaiting += HandleOnAutoScreenshotProcessedWaiting;
            _httpClient = new HttpClient();

            _webhookUrls = Config.Instance.WebhookUrls;
            _screenshotLocation = Config.Instance.ScreenshotLocation;
            _screenshotPath = "";
            _embedContent = null;

            var solidColorBrush = Config.Instance.EmbededColor as SolidColorBrush;
            if (solidColorBrush != null)
            {
                var color = solidColorBrush.Color;
                _finalEmbedColor = (color.R << 16) | (color.G << 8) | color.B;
            }

            // Load Newtonsoft from NT using reflection
            _newtonsoftJsonAssembly = Assembly.LoadFrom(@"C:\Program Files\NinjaTrader 8\bin\Newtonsoft.Json.dll");
            _jsonConvertType = _newtonsoftJsonAssembly.GetType("Newtonsoft.Json.JsonConvert");
        }

        private void HandleOnOrderEntryProcessed(List<Position> positions, List<OrderEntry> orderEntries)
        {
            _embedContent = GetEmbedContent(positions, orderEntries);

            Task.Run(async () =>
            {
                // We want the chart to update the orders prior to the screenshot
                await Task.Delay(1000);
                _eventManager.TakeScreenshot(ProcessType.Auto);
            });
        }

        // Screenshot taken and now waiting for auto processing
        private void HandleOnAutoScreenshotProcessedWaiting()
        {
            _ = SendMessageAsync((success, message) =>
            {
                if (success)
                {
                    // AddEventLog("Success", "Trading Status Sent");
                    _eventManager.PrintMessage("SUCCESS");
                }
                else
                {
                    // AddEventLog("Failed", "Trading Status Sent");
                    // Print(message);
                    _eventManager.PrintMessage("FAILED");
                }

                _screenshotPath = "";
                _embedContent = null;
            });
        }

        private async Task SendMessageAsync(Action<bool, string> callback)
        {
            var serializeMethod = _jsonConvertType.GetMethod("SerializeObject", new[] { typeof(object) });

            var messagePayload = new
            {
                embeds = new[] { _embedContent }
            };
            var jsonPayload = (string)serializeMethod.Invoke(null, new object[] { messagePayload });

            await SendHttpRequestAsync(jsonPayload, callback);
        }

        private async Task SendHttpRequestAsync(string jsonPayload, Action<bool, string> callback)
        {
            try
            {
                using (var formData = new MultipartFormDataContent())
                {
                    if (!string.IsNullOrEmpty(jsonPayload))
                    {
                        var jsonContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                        formData.Add(jsonContent, "payload_json");
                    }

                    // Reassign here in case user quickly creates multiple screenshots
                    // Sometimes the files wont delete if its too fast
                    var screenshotPath = _screenshotPath;

                    var fileStream = new FileStream(screenshotPath, FileMode.Open, FileAccess.Read);
                    var fileContent = new StreamContent(fileStream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    formData.Add(fileContent, "file", Path.GetFileName(screenshotPath));

                    HttpResponseMessage response = await _httpClient.PostAsync(_webhookUrls[0], formData);

                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            File.Delete(screenshotPath);
                            callback(true, "Screenshot sent and file deleted successfully.");
                        }
                        catch (Exception deleteEx)
                        {
                            callback(true, $"Screenshot sent successfully, but failed to delete file: {deleteEx.Message}");
                        }
                    }
                    else
                    {
                        File.Delete(screenshotPath);
                        callback(false, $"Failed to send screenshot. Status code: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                callback(false, $"An error occurred: {ex.Message}");
            }
        }

        private async Task HandleOnScreenshotProcessed(ProcessType processType, string screenshotName)
        {
            string filePath = Path.Combine(_screenshotLocation, screenshotName);
            _screenshotPath = filePath;

            if (processType == ProcessType.Auto)
            {
                _eventManager.AutoScreenshotProcessedWaiting();
            }
            else
            {
                // Send the screenshot asynchronously and handle the callback
                await SendScreenshotAsync((success, message) =>
                {
                    if (success)
                    {
                        _eventManager.PrintMessage("Screenshot sent successfully.");
                    }
                    else
                    {
                        _eventManager.PrintMessage("Failed to send screenshot.");
                    }
                });
            }
        }

        private async Task SendScreenshotAsync(Action<bool, string> callback)
        {
            // Ensure the file exists before sending
            if (!await EnsureFileExists())
            {
                callback(false, "Failed to send screenshot. File not found after retries.");
                return;
            }

            // Send the screenshot via HTTP request
            await SendHttpRequestAsync(null, callback);
        }

        private async Task<bool> EnsureFileExists(int retryCount = 5, int delayMilliseconds = 500)
        {
            for (int i = 0; i < retryCount; i++)
            {
                if (File.Exists(_screenshotPath))
                {
                    return true;
                }
                await Task.Delay(delayMilliseconds);
            }
            return false;
        }

        private object GetEmbedContent(List<Position> positions, List<OrderEntry> orderEntries)
        {
            var embed = new
            {
                title = "Trading Status",
                color = _finalEmbedColor,
                fields = new List<object>()
            };

            // Group positions by instrument
            if (positions.Count == 0)
            {
                embed.fields.Add(new
                {
                    name = "**Positions**",
                    value = "No Positions",
                    inline = false
                });
            }
            else
            {
                var positionGroups = positions.GroupBy(p => p.Instrument);
                foreach (var group in positionGroups)
                {
                    var positionDetails = new StringBuilder();
                    foreach (var position in group)
                    {
                        positionDetails.AppendLine($"Quantity: {position.Quantity}");
                        positionDetails.AppendLine($"Avg Price: {position.AveragePrice}");
                        positionDetails.AppendLine($"Position: {position.MarketPosition}");
                    }

                    embed.fields.Add(new
                    {
                        name = $"**{group.Key} Positions**",
                        value = $"```{positionDetails.ToString()}```",
                        inline = false
                    });
                }
            }

            // Group orders by instrument
            if (orderEntries.Count == 0)
            {
                embed.fields.Add(new
                {
                    name = "**Active Orders**",
                    value = "No Active Orders",
                    inline = false
                });
            }
            else
            {
                var orderGroups = orderEntries.GroupBy(o => o.Instrument);
                foreach (var group in orderGroups)
                {
                    var orderDetails = new StringBuilder();
                    foreach (var order in group)
                    {
                        orderDetails.AppendLine($"Quantity: {order.Quantity}");
                        orderDetails.AppendLine($"Price: {order.Price}");
                        orderDetails.AppendLine($"Action: {order.Action}");
                        orderDetails.AppendLine($"Type: {order.Type}");
                        orderDetails.AppendLine("");
                    }

                    embed.fields.Add(new
                    {
                        name = $"**{group.Key} Active Orders**",
                        value = $"```{orderDetails.ToString()}```",
                        inline = false
                    });
                }
            }

            return embed;
        }
    }
}
