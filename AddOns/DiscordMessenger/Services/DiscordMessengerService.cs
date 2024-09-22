using NinjaTrader.Custom.AddOns.DiscordMessenger.Configs;
using NinjaTrader.Custom.AddOns.DiscordMessenger.Models;
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

namespace NinjaTrader.Custom.AddOns.DiscordMessenger.Services
{
    public class DiscordMessengerService
    {
        private readonly EventManager _eventManager;
        private readonly HttpClient _httpClient;

        private List<string> _webhookUrls;
        private string _screenshotLocation;
        private string _screenshotName;

        private int _finalEmbededColor;
        private Assembly _newtonsoftJsonAssembly;
        private Type _jsonConvertType;

        public DiscordMessengerService(EventManager eventManager)
        {
            _eventManager = eventManager;
            _eventManager.OnOrderEntryProcessed += HandleOnOrderEntryProcessed;
            _httpClient = new HttpClient();

            _webhookUrls = Config.Instance.WebhookUrls;
            _screenshotLocation = Config.Instance.ScreenshotLocation;
            _screenshotName = "test.jpg";

            var solidColorBrush = Config.Instance.EmbededColor as SolidColorBrush;
            if (solidColorBrush != null)
            {
                var color = solidColorBrush.Color;
                _finalEmbededColor = (color.R << 16) | (color.G << 8) | color.B;
            }

            // Load Newtonsoft from NT using reflection
            _newtonsoftJsonAssembly = Assembly.LoadFrom(@"C:\Program Files\NinjaTrader 8\bin\Newtonsoft.Json.dll");
            _jsonConvertType = _newtonsoftJsonAssembly.GetType("Newtonsoft.Json.JsonConvert");
        }

        private void HandleOnOrderEntryProcessed(List<Position> positions, List<OrderEntry> orderEntries)
        {
            _ = SendMessageAsync(positions, orderEntries, (success, message) =>
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
            });
        }

        private async Task SendMessageAsync(List<Position> positions, List<OrderEntry> orderEntries, Action<bool, string> callback)
        {
            //await TakeScreenshot();

            var embedContent = GetEmbedContent(positions, orderEntries);
            string filePath = Path.Combine(_screenshotLocation, _screenshotName);
            var serializeMethod = _jsonConvertType.GetMethod("SerializeObject", new[] { typeof(object) });

            // Create the message payload with embeds
            var messagePayload = new
            {
                embeds = new[] { embedContent }
            };
            var jsonPayload = (string)serializeMethod.Invoke(null, new object[] { messagePayload });

            // Ensure the file exists before sending
            /*if (!await EnsureFileExists(filePath))
            {
                callback(false, "Failed to send message. Screenshot file not found after retries.");
                return;
            }*/

            await SendHttpRequestAsync(filePath, jsonPayload, callback);

            /* await TakeScreenshot();

             var embedContent = GetEmbedContent();
             string filePath = Path.Combine(ScreenshotLocation, _screenshotName);
             var serializeMethod = _jsonConvertType.GetMethod("SerializeObject", new[] { typeof(object) });

             // Create the message payload with embeds
             var messagePayload = new
             {
                 embeds = new[] { embedContent }
             };
             var jsonPayload = (string)serializeMethod.Invoke(null, new object[] { messagePayload });

             // Ensure the file exists before sending
             if (!await EnsureFileExists(filePath))
             {
                 callback(false, "Failed to send message. Screenshot file not found after retries.");
                 return;
             }

             await SendHttpRequestAsync(filePath, jsonPayload, callback);*/
        }

        private async Task SendHttpRequestAsync(string filePath, string jsonPayload, Action<bool, string> callback)
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

                    var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    var fileContent = new StreamContent(fileStream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    formData.Add(fileContent, "file", Path.GetFileName(filePath));

                    HttpResponseMessage response = await _httpClient.PostAsync(_webhookUrls[0], formData);

                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            //File.Delete(filePath);
                            callback(true, "Screenshot sent and file deleted successfully.");
                        }
                        catch (Exception deleteEx)
                        {
                            callback(true, $"Screenshot sent successfully, but failed to delete file: {deleteEx.Message}");
                        }
                    }
                    else
                    {
                        callback(false, $"Failed to send screenshot. Status code: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                callback(false, $"An error occurred: {ex.Message}");
            }
        }

        private async Task TakeScreenshot()
        {
            /*await Dispatcher.InvokeAsync(() =>
            {
                if (_chartWindow != null)
                {
                    RenderTargetBitmap screenCapture = _chartWindow.GetScreenshot(ShareScreenshotType.Chart);
                    _outputFrame = BitmapFrame.Create(screenCapture);

                    _screenshotName = $"{DateTime.Now:yyyyMMddHHmmss}.png";

                    if (!Directory.Exists(ScreenshotLocation))
                    {
                        Directory.CreateDirectory(ScreenshotLocation);
                    }

                    if (screenCapture != null)
                    {
                        PngBitmapEncoder png = new PngBitmapEncoder();
                        png.Frames.Add(_outputFrame);

                        using (Stream stream = File.Create(Path.Combine(ScreenshotLocation, _screenshotName)))
                        {
                            png.Save(stream);
                        }
                    }
                }
            });*/
        }

        private async Task<bool> EnsureFileExists(string filePath, int retryCount = 5, int delayMilliseconds = 500)
        {
            for (int i = 0; i < retryCount; i++)
            {
                if (File.Exists(filePath))
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
                color = _finalEmbededColor,
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
