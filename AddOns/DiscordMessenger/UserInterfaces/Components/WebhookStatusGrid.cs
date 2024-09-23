using NinjaTrader.Custom.AddOns.DiscordMessenger.Configs;
using NinjaTrader.Custom.AddOns.DiscordMessenger.UserInterfaces.Configs;
using NinjaTrader.Custom.AddOns.DiscordMessenger.UserInterfaces.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NinjaTrader.Custom.AddOns.DiscordMessenger.UserInterfaces.Components
{
    public class WebhookStatusGrid : Grid, IComponentSetup
    {
        private EventManager _eventManager;
        private Ellipse _statusCircle;

        public WebhookStatusGrid(EventManager eventManager)
        {
            _eventManager = eventManager;
            _eventManager.OnUpdateStatus += HandleUpdateStatus;
            InitializeComponent();
        }

        public void InitializeComponent()
        {
            StackPanel statusPanel = new StackPanel
            {
                Margin = new Thickness(4),
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };

            _statusCircle = new Ellipse
            {
                Width = 15,
                Height = 15,
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CustomColors.STATUS_FAILED)),
                Margin = new Thickness(0, 0, 10, 0)
            };

            TextBlock statusText = new TextBlock
            {
                Text = "Discord Webhook Status",
                Foreground = UserInterfaceUtils.GetSolidColorBrushFromHex(CustomColors.TEXT_COLOR),
                VerticalAlignment = VerticalAlignment.Center
            };

            statusPanel.Children.Add(_statusCircle);
            statusPanel.Children.Add(statusText);

            Grid.SetRow(statusPanel, 0);
            Grid.SetColumnSpan(statusPanel, 2);

            this.Children.Add(statusPanel);
        }

        private void HandleUpdateStatus(Status status)
        {
            switch (status)
            {
                case Status.Success:
                    _statusCircle.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CustomColors.STATUS_SUCCESS));
                    break;
                case Status.Failed:
                    _statusCircle.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CustomColors.STATUS_FAILED));
                    break;
                case Status.PartialSuccess:
                    _statusCircle.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CustomColors.STATUS_PARTIAL_SUCCESS));
                    break;
            }
        }
    }
}
