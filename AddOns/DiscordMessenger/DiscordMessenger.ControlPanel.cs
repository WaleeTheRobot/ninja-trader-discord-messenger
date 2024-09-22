using NinjaTrader.Custom.AddOns.DiscordMessenger.Configs;
using NinjaTrader.Custom.AddOns.DiscordMessenger.Models;
using NinjaTrader.Custom.AddOns.DiscordMessenger.Utils;
using NinjaTrader.Gui.Chart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NinjaTrader.NinjaScript.Indicators
{
    public partial class DiscordMessenger : Indicator
    {
        private ChartTab _chartTab;
        private Chart _chartWindow;
        private Grid _chartTraderGrid, _chartTraderButtonsGrid, _mainGrid;
        private bool _panelActive;
        private TabItem _tabItem;

        private List<EventLog> _eventLogs = new List<EventLog>();
        private Ellipse _statusCircle;
        private Label _eventLogsListlabel;

        private Button _tradingStatusButton, _screenshotButton;

        private void LoadControlPanel()
        {
            if (ChartControl != null)
            {
                ChartControl.Dispatcher.InvokeAsync(() =>
                {
                    CreateWPFControls();
                });
            }
        }

        private void UnloadControlPanel()
        {
            if (ChartControl != null)
            {
                ChartControl.Dispatcher.InvokeAsync(() =>
                {
                    DisposeWPFControls();
                });
            }
        }

        private void HandleOnWebhookStatusUpdated(Status status)
        {
            if (ChartControl != null)
            {
                ChartControl.Dispatcher.InvokeAsync(() =>
                {
                    UpdateStatusCircle(status);
                    UpdateButtons(status);
                });
            }
        }

        private void CreateWPFControls()
        {
            _chartWindow = Window.GetWindow(ChartControl.Parent) as Gui.Chart.Chart;

            if (_chartWindow == null)
            {
                return;
            }

            // Chart Trader area grid
            _chartTraderGrid = (_chartWindow.FindFirst("ChartWindowChartTraderControl") as ChartTrader).Content as Grid;

            if (_chartTraderGrid == null)
            {
                return;
            }

            // Existing Chart Trader buttons
            _chartTraderButtonsGrid = _chartTraderGrid.Children[0] as Grid;

            if (_chartTraderButtonsGrid == null)
            {
                return;
            }

            // Create main grid
            _mainGrid = new Grid
            {
                Margin = new Thickness(0, 60, 0, 0),
                Background = Utils.GetSolidColorBrushFromHex(CustomColors.MAIN_GRID_BG_COLOR)
            };

            // Define row and column structure
            for (int i = 0; i < 5; i++)
            {
                _mainGrid.RowDefinitions.Add(new RowDefinition());
            }
            _mainGrid.ColumnDefinitions.Add(new ColumnDefinition());
            _mainGrid.ColumnDefinitions.Add(new ColumnDefinition());

            AddWebhookStatus();
            AddButtons();
            AddRecentEvents();

            if (TabSelected()) InsertWPFControls();

            _chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;
        }

        #region Webhook Status

        private void AddWebhookStatus()
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
                Foreground = Utils.GetSolidColorBrushFromHex(CustomColors.TEXT_COLOR),
                VerticalAlignment = VerticalAlignment.Center
            };

            statusPanel.Children.Add(_statusCircle);
            statusPanel.Children.Add(statusText);

            Grid.SetRow(statusPanel, 0);
            Grid.SetColumnSpan(statusPanel, 2);
            _mainGrid.Children.Add(statusPanel);
        }

        private void UpdateStatusCircle(Status status)
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

        #endregion

        #region Recent Events

        private void AddRecentEvents()
        {
            // Event Label
            TextBlock eventLabel = new TextBlock
            {
                Text = "Recent Events",
                FontSize = 14,
                Foreground = Utils.GetSolidColorBrushFromHex(CustomColors.TEXT_COLOR),
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4, 10, 0, 0)
            };

            Grid.SetRow(eventLabel, 3);
            Grid.SetColumnSpan(eventLabel, 2);
            _mainGrid.Children.Add(eventLabel);

            // Event Logs
            _eventLogsListlabel = new Label
            {
                Content = "",
                FontSize = 10,
                Foreground = Utils.GetSolidColorBrushFromHex(CustomColors.TEXT_COLOR),
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
            _mainGrid.Children.Add(_eventLogsListlabel);
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

        #endregion

        #region Buttons

        private void AddButtons()
        {
            // Trading Status button
            _tradingStatusButton = ButtonUtils.GetButton(new ButtonModel
            {
                Content = "Trading Status Disabled",
                ToggledContent = "Trading Status Enabled",
                BackgroundColor = CustomColors.BUTTON_BG_COLOR,
                HoverBackgroundColor = CustomColors.BUTTON_HOVER_BG_COLOR,
                ToggledBackgroundColor = CustomColors.BUTTON_TOGGLED_BG_COLOR,
                TextColor = CustomColors.TEXT_COLOR,
                ClickHandler = (Action<object, RoutedEventArgs>)TradingStatusButtonClick,
                IsToggleable = true,
                // Start in the toggled "Enabled" state
                InitialToggleState = true
            });

            Grid.SetRow(_tradingStatusButton, 1);
            Grid.SetColumnSpan(_tradingStatusButton, 2);
            _mainGrid.Children.Add(_tradingStatusButton);

            // Send Screenshot button
            _screenshotButton = ButtonUtils.GetButton(new ButtonModel
            {
                Content = "Send Screenshot",
                BackgroundColor = CustomColors.BUTTON_BG_COLOR,
                HoverBackgroundColor = CustomColors.BUTTON_HOVER_BG_COLOR,
                TextColor = CustomColors.TEXT_COLOR,
                ClickHandler = (Func<object, RoutedEventArgs, Task>)SendScreenshotButtonClickAsync,
                IsToggleable = false
            });

            Grid.SetRow(_screenshotButton, 2);
            Grid.SetColumnSpan(_screenshotButton, 2);
            _mainGrid.Children.Add(_screenshotButton);

            // Initially disable buttons
            UpdateButtons(Status.Failed);
        }

        private void UpdateButtons(Status status)
        {
            bool enable = status != Status.Success || status != Status.PartialSuccess;

            ButtonUtils.UpdateButtonState(_tradingStatusButton, enable);
            ButtonUtils.UpdateButtonState(_screenshotButton, enable);
        }

        private void TradingStatusButtonClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ButtonState state = (ButtonState)button.Tag;

            // _tradingStatusDisabled = !state.IsToggled;
        }

        private async Task SendScreenshotButtonClickAsync(object sender, RoutedEventArgs e)
        {
            /*await SendScreenshotAsync((success, message) =>
            {
                if (success)
                {
                    AddEventLog("Success", "Screenshot Sent");
                }
                else
                {
                    AddEventLog("Failed", "Screenshot Sent");
                    Print(message);
                }
            });*/
        }

        #endregion

        #region Control Handling

        private void DisposeWPFControls()
        {
            if (_chartWindow != null)
                _chartWindow.MainTabControl.SelectionChanged -= TabChangedHandler;

            RemoveWPFControls();
        }

        private void InsertWPFControls()
        {
            if (_panelActive)
                return;

            Grid.SetRow(_mainGrid, (_chartTraderGrid.RowDefinitions.Count - 1));
            _chartTraderGrid.Children.Add(_mainGrid);

            _panelActive = true;
        }

        private void RemoveWPFControls()
        {
            if (!_panelActive)
                return;

            if (_chartTraderButtonsGrid != null || _mainGrid != null)
            {
                _chartTraderGrid.Children.Remove(_mainGrid);
            }

            _panelActive = false;
        }

        private bool TabSelected()
        {
            bool tabSelected = false;

            foreach (TabItem tab in _chartWindow.MainTabControl.Items)
                if ((tab.Content as ChartTab).ChartControl == ChartControl && tab == _chartWindow.MainTabControl.SelectedItem)
                    tabSelected = true;

            return tabSelected;
        }

        private void TabChangedHandler(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0)
                return;

            _tabItem = e.AddedItems[0] as TabItem;
            if (_tabItem == null)
                return;

            _chartTab = _tabItem.Content as ChartTab;
            if (_chartTab == null)
                return;

            if (TabSelected())
                InsertWPFControls();
            else
                RemoveWPFControls();
        }

        #endregion
    }
}
