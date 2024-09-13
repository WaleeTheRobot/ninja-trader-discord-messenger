using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NinjaTrader.NinjaScript.Strategies
{
    public partial class DiscordMessenger : Strategy
    {
        private readonly HttpClient _client = new HttpClient();

        private async Task SendMessageAsync(Action<bool, string> callback)
        {
            var embedContent = GetEmbedContent();

            var messagePayload = new
            {
                embeds = new[] { embedContent }
            };

            // Load Newtonsoft from Ninja using reflection
            var newtonsoftJsonAssembly = Assembly.LoadFrom(@"C:\Program Files\NinjaTrader 8\bin\Newtonsoft.Json.dll");
            var jsonConvertType = newtonsoftJsonAssembly.GetType("Newtonsoft.Json.JsonConvert");
            var serializeMethod = jsonConvertType.GetMethod("SerializeObject", new[] { typeof(object) });

            var jsonPayload = (string)serializeMethod.Invoke(null, new object[] { messagePayload });
            var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await _client.PostAsync(WebhookUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    Print("Message sent successfully.");
                    callback(true, "Message sent successfully.");
                }
                else
                {
                    Print($"Failed to send message. Status code: {response.StatusCode}");
                    callback(false, $"Failed to send message. Status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Print($"An error occurred: {ex.Message}");
                callback(false, $"An error occurred: {ex.Message}");
            }
        }


        private object GetEmbedContent()
        {
            var embed = new
            {
                title = "Trading Status",
                color = 3447003, // TODO: allow user to change this
                fields = new List<object>()
            };

            // Group positions by instrument
            if (_positions.Count == 0)
            {
                embed.fields.Add(new
                {
                    name = "Positions",
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
                    name = "Active Orders",
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
