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
using System.Windows.Media.Imaging;

namespace NinjaTrader.NinjaScript.Strategies
{
    public partial class DiscordMessenger : Strategy
    {
        private readonly HttpClient _client = new HttpClient();
        private BitmapFrame _outputFrame;
        private string _screenshotName = "";
        private int _finalEmbededColor;

        private Assembly _newtonsoftJsonAssembly;
        private Type _jsonConvertType;

        private void ConfigureMessengerManager()
        {
            var solidColorBrush = EmbededColor as SolidColorBrush;
            if (solidColorBrush != null)
            {
                var color = solidColorBrush.Color;
                _finalEmbededColor = (color.R << 16) | (color.G << 8) | color.B;
            }

            // Load Newtonsoft from Ninja using reflection
            _newtonsoftJsonAssembly = Assembly.LoadFrom(@"C:\Program Files\NinjaTrader 8\bin\Newtonsoft.Json.dll");
            _jsonConvertType = _newtonsoftJsonAssembly.GetType("Newtonsoft.Json.JsonConvert");
        }

        private async Task SendMessageAsync(Action<bool, string> callback)
        {
            await TakeScreenshot();

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

            await SendHttpRequestAsync(filePath, jsonPayload, callback);
        }

        private async Task SendScreenshotAsync(Action<bool, string> callback)
        {
            await TakeScreenshot();

            string filePath = Path.Combine(ScreenshotLocation, _screenshotName);

            // Ensure the file exists before sending
            if (!await EnsureFileExists(filePath))
            {
                callback(false, "Failed to send screenshot. File not found after retries.");
                return;
            }

            await SendHttpRequestAsync(filePath, null, callback);
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

                    HttpResponseMessage response = await _client.PostAsync(WebhookUrl, formData);

                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            File.Delete(filePath);
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

        private object GetEmbedContent()
        {
            var embed = new
            {
                title = "Trading Status",
                color = _finalEmbededColor,
                fields = new List<object>()
            };

            // Group positions by instrument
            if (_positions.Count == 0)
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
                var positionGroups = _positions.GroupBy(p => p.Instrument);
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
            if (_orderEntries.Count == 0)
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
                var orderGroups = _orderEntries.GroupBy(o => o.Instrument);
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

        private async Task TakeScreenshot()
        {
            await Dispatcher.InvokeAsync(() =>
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
            });
        }
    }
}
