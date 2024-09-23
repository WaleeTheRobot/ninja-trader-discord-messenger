using NinjaTrader.Custom.AddOns.DiscordMessenger.Models;
using NinjaTrader.Custom.AddOns.DiscordMessenger.UserInterfaces.Configs;
using NinjaTrader.Custom.AddOns.DiscordMessenger.UserInterfaces.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NinjaTrader.Custom.AddOns.DiscordMessenger.UserInterfaces.Components
{
    public class RecentEventsGrid : Grid, IComponentSetup
    {
        private EventManager _eventManager;
        private List<EventLog> _eventLogs = new List<EventLog>();
        private Label _eventLogsListlabel;

        public RecentEventsGrid(EventManager eventManager)
        {
            _eventManager = eventManager;
            //_eventManager.OnUpdateUserInterface += HandleUpdateUserInterface;
            InitializeComponent();
        }

        public void InitializeComponent()
        {
            // Event Label
            TextBlock eventLabel = new TextBlock
            {
                Text = "Recent Events",
                FontSize = 14,
                Foreground = UserInterfaceUtils.GetSolidColorBrushFromHex(CustomColors.TEXT_COLOR),
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4, 10, 0, 0)
            };

            Grid.SetRow(eventLabel, 3);
            Grid.SetColumnSpan(eventLabel, 2);
            this.Children.Add(eventLabel);

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

            Grid.SetRow(_eventLogsListlabel, 4);
            Grid.SetColumnSpan(_eventLogsListlabel, 2);
            this.Children.Add(_eventLogsListlabel);
        }

        private void AddEventLog(string status, string eventMessage)
        {
            var dateTime = DateTime.Now;

            _eventLogs.Add(new EventLog
            {
                Time = dateTime,
                Status = status,
                Message = eventMessage
            });

            // Limit
            if (_eventLogs.Count > 5)
            {
                _eventLogs.RemoveAt(0);
            }

            UpdateEventLogDisplay();
        }

        private void UpdateEventLogDisplay()
        {
            _eventLogsListlabel.Dispatcher.Invoke(() =>
            {
                _eventLogsListlabel.Content = string.Empty;

                foreach (var log in _eventLogs.AsEnumerable().Reverse())
                {
                    string logEntry = $"{log.Status} | {log.Time:HH:mm:ss} | {log.Message}";
                    _eventLogsListlabel.Content += logEntry + "\n";
                }
            });
        }
    }
}
