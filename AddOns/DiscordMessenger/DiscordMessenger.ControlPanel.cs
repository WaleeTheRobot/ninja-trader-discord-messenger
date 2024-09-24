using NinjaTrader.Custom.AddOns.DiscordMessenger.Configs;
using NinjaTrader.Custom.AddOns.DiscordMessenger.UserInterfaces.Components;
using NinjaTrader.Custom.AddOns.DiscordMessenger.UserInterfaces.Configs;
using NinjaTrader.Custom.AddOns.DiscordMessenger.UserInterfaces.Utils;
using NinjaTrader.Gui.Chart;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace NinjaTrader.NinjaScript.Indicators
{
    public partial class DiscordMessenger : Indicator
    {
        private ChartTab _chartTab;
        private Chart _chartWindow;
        private Grid _chartTraderGrid, _chartTraderButtonsGrid, _mainGrid;
        private bool _panelActive;
        private TabItem _tabItem;
        private WebhookStatusGrid _webhookStatusGrid;
        private ButtonsGrid _buttonsGrid;
        private RecentEventsGrid _recentEventsGrid;

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
                    _controlPanelEvents.UpdateStatus(status);
                });
            }
        }

        private void CreateWPFControls()
        {
            _chartWindow = Window.GetWindow(ChartControl.Parent) as Gui.Chart.Chart;
            if (_chartWindow == null) return;

            _chartTraderGrid = (_chartWindow.FindFirst("ChartWindowChartTraderControl") as ChartTrader).Content as Grid;
            if (_chartTraderGrid == null) return;

            _chartTraderButtonsGrid = _chartTraderGrid.Children[0] as Grid;
            if (_chartTraderButtonsGrid == null) return;

            _mainGrid = new Grid
            {
                Margin = new Thickness(0, 60, 0, 0),
                Background = UserInterfaceUtils.GetSolidColorBrushFromHex(CustomColors.MAIN_GRID_BG_COLOR)
            };

            _mainGrid.RowDefinitions.Add(new RowDefinition());
            _mainGrid.RowDefinitions.Add(new RowDefinition());
            _mainGrid.RowDefinitions.Add(new RowDefinition());

            // Instantiate and add the new components
            _webhookStatusGrid = new WebhookStatusGrid(_controlPanelEvents);
            Grid.SetRow(_webhookStatusGrid, 0);
            _mainGrid.Children.Add(_webhookStatusGrid);

            _buttonsGrid = new ButtonsGrid(_controlPanelEvents, _tradingStatusEvents);
            Grid.SetRow(_buttonsGrid, 1);
            _mainGrid.Children.Add(_buttonsGrid);

            _recentEventsGrid = new RecentEventsGrid(_eventLoggingEvents);
            Grid.SetRow(_recentEventsGrid, 2);
            _mainGrid.Children.Add(_recentEventsGrid);

            if (TabSelected()) InsertWPFControls();
            _chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;
        }

        private async Task HandleScreenshot(ProcessType processType)
        {
            await TakeScreenshot(processType);
        }

        private async Task TakeScreenshot(ProcessType processType)
        {
            string screenshotName = $"{DateTime.Now:yyyyMMddHHmmssfff}.png";

            await Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    if (_chartWindow != null)
                    {
                        RenderTargetBitmap screenCapture = _chartWindow.GetScreenshot(ShareScreenshotType.Chart);
                        BitmapFrame outputFrame = BitmapFrame.Create(screenCapture);

                        if (!Directory.Exists(ScreenshotLocation))
                        {
                            Directory.CreateDirectory(ScreenshotLocation);
                        }

                        if (screenCapture != null)
                        {
                            PngBitmapEncoder png = new PngBitmapEncoder();
                            png.Frames.Add(outputFrame);

                            using (Stream stream = File.Create(Path.Combine(ScreenshotLocation, screenshotName)))
                            {
                                png.Save(stream);
                            }

                            _ = _controlPanelEvents.ScreenshotProcessed(processType, screenshotName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _eventManager.PrintMessage($"Error taking screenshot: {ex.Message}");
                }
            });
        }

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
