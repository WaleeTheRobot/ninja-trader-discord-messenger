using NinjaTrader.Custom.AddOns.DiscordMessenger.Events;
using NinjaTrader.Custom.AddOns.DiscordMessenger.Models;
using NinjaTrader.Custom.AddOns.DiscordMessenger.UserInterfaces.Configs;
using NinjaTrader.Custom.AddOns.DiscordMessenger.UserInterfaces.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NinjaTrader.Custom.AddOns.DiscordMessenger.UserInterfaces.Components
{
    public class RecentEventsGrid : Grid, IComponentSetup
    {
        private EventLoggingEvents _eventLoggingEvents;
        private Label _eventLogsListlabel;

        public RecentEventsGrid(EventLoggingEvents eventLoggingEvents)
        {
            _eventLoggingEvents = eventLoggingEvents;
            _eventLoggingEvents.OnRecentEventProcessed += HandleRecentEventProcessed;
            InitializeComponent();
        }

        public void InitializeComponent()
        {
            StackPanel stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(4, 4, 0, 0)
            };

            // Event Label
            TextBlock eventLabel = new TextBlock
            {
                Text = "Recent Events",
                FontSize = 14,
                Foreground = UserInterfaceUtils.GetSolidColorBrushFromHex(CustomColors.TEXT_COLOR),
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 0)
            };

            stackPanel.Children.Add(eventLabel);

            // Event Logs
            _eventLogsListlabel = new Label
            {
                Content = "",
                FontSize = 10,
                Foreground = UserInterfaceUtils.GetSolidColorBrushFromHex(CustomColors.TEXT_COLOR),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 4, 0, 0),
                Height = 100,
                VerticalContentAlignment = VerticalAlignment.Top,
                Padding = new Thickness(5),
                BorderThickness = new Thickness(0),
                BorderBrush = Brushes.Transparent
            };

            stackPanel.Children.Add(_eventLogsListlabel);

            this.Children.Add(stackPanel);
        }

        private void HandleRecentEventProcessed(List<EventLog> eventLogs)
        {
            _eventLogsListlabel.Dispatcher.Invoke(() =>
            {
                _eventLogsListlabel.Content = string.Empty;

                foreach (var log in eventLogs.AsEnumerable().Reverse())
                {
                    string logEntry = $"{log.Status} | {log.Time:HH:mm:ss} | {log.Message}";
                    _eventLogsListlabel.Content += logEntry + "\n";
                }
            });
        }
    }
}
