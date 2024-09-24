using NinjaTrader.Custom.AddOns.DiscordMessenger.Configs;
using NinjaTrader.Custom.AddOns.DiscordMessenger.Events;
using NinjaTrader.Custom.AddOns.DiscordMessenger.UserInterfaces.Configs;
using NinjaTrader.Custom.AddOns.DiscordMessenger.UserInterfaces.Models;
using NinjaTrader.Custom.AddOns.DiscordMessenger.UserInterfaces.Utils;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NinjaTrader.Custom.AddOns.DiscordMessenger.UserInterfaces.Components
{
    public class ButtonsGrid : Grid, IComponentSetup
    {
        private ControlPanelEvents _controlPanelEvents;
        private TradingStatusEvents _tradingStatusEvents;
        private Button _autoButton, _tradingStatusButton, _screenshotButton;

        public ButtonsGrid(ControlPanelEvents controlPanelEvents, TradingStatusEvents tradingStatusEvents)
        {
            _controlPanelEvents = controlPanelEvents;
            _controlPanelEvents.OnUpdateStatus += HandleUpdateStatus;

            _tradingStatusEvents = tradingStatusEvents;

            InitializeComponent();
        }

        public void InitializeComponent()
        {
            this.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            this.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            this.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            _autoButton = ButtonUtils.GetButton(new ButtonModel
            {
                Content = "Auto Send Disabled",
                ToggledContent = "Auto Send Enabled",
                BackgroundColor = CustomColors.BUTTON_AUTO_BG_COLOR,
                HoverBackgroundColor = CustomColors.BUTTON_AUTO_HOVER_BG_COLOR,
                ToggledBackgroundColor = CustomColors.BUTTON_AUTO_TOGGLED_BG_COLOR,
                TextColor = CustomColors.TEXT_COLOR,
                ClickHandler = (Action<object, RoutedEventArgs>)HandleAutoButtonClick,
                IsToggleable = true,
                InitialToggleState = true
            });

            Grid.SetRow(_autoButton, 0);
            this.Children.Add(_autoButton);

            _tradingStatusButton = ButtonUtils.GetButton(new ButtonModel
            {
                Content = "Send Trading Status",
                BackgroundColor = CustomColors.BUTTON_BG_COLOR,
                HoverBackgroundColor = CustomColors.BUTTON_HOVER_BG_COLOR,
                TextColor = CustomColors.TEXT_COLOR,
                ClickHandler = (Action<object, RoutedEventArgs>)HandleTradingStatusButtonClick,
                IsToggleable = false
            });

            Grid.SetRow(_tradingStatusButton, 1);
            this.Children.Add(_tradingStatusButton);

            _screenshotButton = ButtonUtils.GetButton(new ButtonModel
            {
                Content = "Send Screenshot",
                BackgroundColor = CustomColors.BUTTON_BG_COLOR,
                HoverBackgroundColor = CustomColors.BUTTON_HOVER_BG_COLOR,
                TextColor = CustomColors.TEXT_COLOR,
                ClickHandler = (Action<object, RoutedEventArgs>)HandleScreenshotButtonClick,
                IsToggleable = false
            });

            Grid.SetRow(_screenshotButton, 2);
            this.Children.Add(_screenshotButton);

            // Initially disable buttons
            HandleUpdateStatus(Status.Failed);
        }

        private void HandleUpdateStatus(Status status)
        {
            bool enable = status == Status.Success || status == Status.PartialSuccess;

            ButtonUtils.UpdateButtonState(_autoButton, enable);
            ButtonUtils.UpdateButtonState(_tradingStatusButton, enable);
            ButtonUtils.UpdateButtonState(_screenshotButton, enable);
        }

        private void HandleAutoButtonClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ButtonState state = (ButtonState)button.Tag;

            _controlPanelEvents.AutoButtonClicked(!state.IsToggled);
        }

        private void HandleTradingStatusButtonClick(object sender, RoutedEventArgs e)
        {
            _tradingStatusEvents.UpdateOrderEntry();
        }

        private void HandleScreenshotButtonClick(object sender, RoutedEventArgs e)
        {
            _ = _controlPanelEvents.TakeScreenshot(ProcessType.Manual);
        }
    }
}
