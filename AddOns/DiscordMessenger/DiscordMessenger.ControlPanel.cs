#region Using declarations
using NinjaTrader.Gui.Chart;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
#endregion

// TODO
namespace NinjaTrader.NinjaScript.Strategies
{
    public partial class DiscordMessenger : Strategy
    {
        private ChartTab _chartTab;
        private Chart _chartWindow;
        private Grid _chartTraderGrid, _chartTraderButtonsGrid, _mainGrid;
        private bool _panelActive;
        private TabItem _tabItem;

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
                Margin = new Thickness(0, 50, 0, 0),
            };

            _mainGrid.RowDefinitions.Add(new RowDefinition());

            Button sendMessageButton = GetButton("Send Message", "#5C64F2", "#4F75FF", "#FFFFFF");
            sendMessageButton.Click += SendMessageButtonClick;

            Grid.SetRow(sendMessageButton, 0);
            _mainGrid.Children.Add(sendMessageButton);

            if (TabSelected()) InsertWPFControls();

            _chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;
        }

        private void SendMessageButtonClick(object sender, RoutedEventArgs e)
        {
            // Logic for manually sending
            Print("Sending Message...");
        }

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

            // Set the padding for the button
            style.Setters.Add(new Setter(Button.PaddingProperty, new Thickness(3)));

            return style;
        }

        #endregion

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
    }
}
