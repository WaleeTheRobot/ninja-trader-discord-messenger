#region Using declarations
using NinjaTrader.Gui.Chart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
#endregion

namespace NinjaTrader.NinjaScript.Strategies
{
    public class EventLog
    {
        public string Status { get; set; }
        public DateTime Time { get; set; }
        public string Message { get; set; }
    }

    // TODO: WIP
    public partial class DiscordMessenger : Strategy
    {
        private ChartTab _chartTab;
        private Chart _chartWindow;
        private Grid _chartTraderGrid, _chartTraderButtonsGrid, _mainGrid;
        private bool _panelActive;
        private TabItem _tabItem;

        private List<EventLog> _eventLogs = new List<EventLog>();
        private bool _webhookStatus = false;
        private Ellipse _statusCircle;
        private Label _eventLogsListlabel;

        private void ControlPanelSetStateDataLoaded()
        {
            if (ChartControl != null)
            {
                ChartControl.Dispatcher.InvokeAsync(() =>
                {
                    CreateWPFControls();
                });
            }
        }

        private void ControlPanelSetStateTerminated()
        {
            if (ChartControl != null)
            {
                ChartControl.Dispatcher.InvokeAsync(() =>
                {
                    DisposeWPFControls();
                });
            }
        }

        private SolidColorBrush GetSolidColorBrushFromHex(string hexColor)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hexColor));
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
                Background = GetSolidColorBrushFromHex("#2C2C34")
            };

            // Define row and column structure
            for (int i = 0; i < 5; i++)
            {
                _mainGrid.RowDefinitions.Add(new RowDefinition());
            }
            _mainGrid.ColumnDefinitions.Add(new ColumnDefinition());
            _mainGrid.ColumnDefinitions.Add(new ColumnDefinition());

            // Discord Webhook Status Row
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
                Fill = new SolidColorBrush(Colors.Red),
                Margin = new Thickness(0, 0, 10, 0)
            };

            TextBlock statusText = new TextBlock
            {
                Text = "Discord Webhook Status",
                Foreground = GetSolidColorBrushFromHex("#E7E7E7"),
                VerticalAlignment = VerticalAlignment.Center
            };

            statusPanel.Children.Add(_statusCircle);
            statusPanel.Children.Add(statusText);

            Grid.SetRow(statusPanel, 0);
            Grid.SetColumnSpan(statusPanel, 2);
            _mainGrid.Children.Add(statusPanel);

            // Trading Status button
            Button tradingStatusButton = GetButton("Trading Status", "#5C64F2", "#4F75FF", "#FFFFFF");

            Grid.SetRow(tradingStatusButton, 1);
            Grid.SetColumnSpan(tradingStatusButton, 2);
            _mainGrid.Children.Add(tradingStatusButton);

            // Send Screenshot button
            Button sendScreenshotButton = GetButton("Send Screenshot", "#5C64F2", "#4F75FF", "#FFFFFF");
            sendScreenshotButton.Click += SendScreenshotButtonClick;

            Grid.SetRow(sendScreenshotButton, 2);
            Grid.SetColumnSpan(sendScreenshotButton, 2);
            _mainGrid.Children.Add(sendScreenshotButton);

            // Event Label
            TextBlock eventLabel = new TextBlock
            {
                Text = "Recent Events",
                FontSize = 14,
                Foreground = GetSolidColorBrushFromHex("#E7E7E7"),
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
                Foreground = GetSolidColorBrushFromHex("#E7E7E7"),
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

            if (TabSelected()) InsertWPFControls();

            _chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;

            UpdateStatusCircle();
        }

        private void SendScreenshotButtonClick(object sender, RoutedEventArgs e)
        {
            //TakeScreenshot();
        }

        public void AddEventLog(string status, string eventMessage)
        {
            var dateTime = DateTime.Now;

            _eventLogs.Add(new EventLog
            {
                Time = dateTime,
                Status = status,
                Message = eventMessage
            });

            Print(string.Format("{0} {1} {2}", dateTime, status, eventMessage));

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

        #region Status

        private void UpdateStatusCircle()
        {
            if (_webhookStatus)
            {
                _statusCircle.Fill = new SolidColorBrush(Colors.Green);
            }
            else
            {
                _statusCircle.Fill = new SolidColorBrush(Colors.Red);
            }
        }

        #endregion

        #region Buttons

        private Button GetButton(string content, string hexBgColor, string hexBgColorHover, string hexTextColor)
        {
            Style customButtonStyle = CreateCustomButtonStyle(hexBgColor, hexBgColorHover);

            Button button = new Button
            {
                Content = content,
                FontSize = 18,
                Visibility = Visibility.Visible,
                Foreground = GetSolidColorBrushFromHex(hexTextColor),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Style = customButtonStyle
            };

            return button;
        }

        public Style CreateCustomButtonStyle(string hexBgColor, string hexBgColorHover)
        {
            Style style = new Style(typeof(Button));

            ControlTemplate template = new ControlTemplate(typeof(Button));

            // Create a Border as the root element
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.SetBinding(Border.BackgroundProperty, new Binding("Background") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
            border.SetBinding(Border.BorderBrushProperty, new Binding("BorderBrush") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
            border.SetBinding(Border.BorderThicknessProperty, new Binding("BorderThickness") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });

            // Bind the Button's Padding to the Border's Padding
            border.SetBinding(Border.PaddingProperty, new Binding("Padding") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });

            // Add ContentPresenter inside the Border
            FrameworkElementFactory contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            border.AppendChild(contentPresenter);

            template.VisualTree = border;

            // Define triggers for different states
            Trigger mouseover = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            mouseover.Setters.Add(new Setter(Button.BackgroundProperty, GetSolidColorBrushFromHex(hexBgColorHover)));

            Trigger pressed = new Trigger { Property = Button.IsPressedProperty, Value = true };
            pressed.Setters.Add(new Setter(Button.BackgroundProperty, GetSolidColorBrushFromHex(hexBgColorHover)));

            style.Triggers.Add(mouseover);
            style.Triggers.Add(pressed);

            // Set the template
            style.Setters.Add(new Setter(Button.TemplateProperty, template));

            // Set default property values
            style.Setters.Add(new Setter(Button.BackgroundProperty, GetSolidColorBrushFromHex(hexBgColor)));
            style.Setters.Add(new Setter(Button.BorderBrushProperty, Brushes.Transparent));
            style.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(0)));

            style.Setters.Add(new Setter(Button.PaddingProperty, new Thickness(3)));
            style.Setters.Add(new Setter(Button.MarginProperty, new Thickness(3)));

            return style;
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
