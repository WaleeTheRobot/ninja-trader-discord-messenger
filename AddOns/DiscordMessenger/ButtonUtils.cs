using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace NinjaTrader.Custom.AddOns.DiscordMessenger
{
    public class ButtonConfig
    {
        public string Content { get; set; }
        public string ToggledContent { get; set; }
        public string BackgroundColor { get; set; }
        public string HoverBackgroundColor { get; set; }
        public string ToggledBackgroundColor { get; set; }
        public string TextColor { get; set; }
        public Delegate ClickHandler { get; set; }
        public bool IsToggleable { get; set; }
        public bool InitialToggleState { get; set; }
    }

    public class ButtonState
    {
        public bool IsToggled { get; set; }
        public ButtonConfig Config { get; set; }
    }

    public static class ButtonUtils
    {
        public static Button GetButton(ButtonConfig config)
        {
            Button button = new Button
            {
                Content = config.IsToggleable && config.InitialToggleState ? config.ToggledContent : config.Content,
                FontSize = 16,
                Visibility = Visibility.Visible,
                Foreground = Utils.GetSolidColorBrushFromHex(config.TextColor),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Tag = new ButtonState { IsToggled = config.IsToggleable && config.InitialToggleState, Config = config },
                Background = Utils.GetSolidColorBrushFromHex(config.BackgroundColor),
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(3),
                Margin = new Thickness(3)
            };

            button.Style = CreateCustomButtonStyle(button);

            if (config.ClickHandler != null)
            {
                button.Click += async (sender, e) =>
                {
                    if (config.IsToggleable)
                    {
                        ToggleButton(button);
                    }

                    if (config.ClickHandler is Action<object, RoutedEventArgs> syncHandler)
                    {
                        syncHandler(sender, e);
                    }
                    else if (config.ClickHandler is Func<object, RoutedEventArgs, Task> asyncHandler)
                    {
                        await asyncHandler(sender, e);
                    }
                };
            }

            button.IsEnabledChanged += (sender, e) => UpdateButtonState(button, button.IsEnabled);
            button.MouseEnter += (sender, e) => button.Background = Utils.GetSolidColorBrushFromHex(config.HoverBackgroundColor);
            button.MouseLeave += (sender, e) => UpdateButtonState(button, button.IsEnabled);

            UpdateButtonState(button, false);

            return button;
        }

        private static Style CreateCustomButtonStyle(Button button)
        {
            Style style = new Style(typeof(Button));

            var config = ((ButtonState)button.Tag).Config;

            style.Setters.Add(new Setter(Button.TemplateProperty, CreateButtonTemplate()));

            style.Triggers.Add(new Trigger
            {
                Property = Button.IsMouseOverProperty,
                Value = true,
                Setters = { new Setter(Button.BackgroundProperty, Utils.GetSolidColorBrushFromHex(config.HoverBackgroundColor)) }
            });

            style.Triggers.Add(new Trigger
            {
                Property = Button.IsPressedProperty,
                Value = true,
                Setters = { new Setter(Button.BackgroundProperty, Utils.GetSolidColorBrushFromHex(config.HoverBackgroundColor)) }
            });

            style.Triggers.Add(new Trigger
            {
                Property = Button.IsEnabledProperty,
                Value = false,
                Setters =
                {
                    new Setter(Button.BackgroundProperty, Utils.GetSolidColorBrushFromHex(Colors.ButtonDisabledBgColor)),
                    new Setter(Button.OpacityProperty, 0.5)
                }
            });

            return style;
        }

        private static ControlTemplate CreateButtonTemplate()
        {
            ControlTemplate template = new ControlTemplate(typeof(Button));

            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.SetBinding(Border.BackgroundProperty, new Binding("Background") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
            border.SetBinding(Border.BorderBrushProperty, new Binding("BorderBrush") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
            border.SetBinding(Border.BorderThicknessProperty, new Binding("BorderThickness") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
            border.SetBinding(Border.PaddingProperty, new Binding("Padding") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });

            FrameworkElementFactory contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            border.AppendChild(contentPresenter);

            template.VisualTree = border;

            return template;
        }

        public static void UpdateButtonState(Button button, bool isEnabled)
        {
            var state = (ButtonState)button.Tag;
            var config = state.Config;

            if (isEnabled)
            {
                if (config.IsToggleable)
                {
                    // Toggled is enabling
                    button.Background = Utils.GetSolidColorBrushFromHex(state.IsToggled ? config.BackgroundColor : config.ToggledBackgroundColor);
                    button.Content = state.IsToggled && config.IsToggleable ? config.ToggledContent : config.Content;
                }
                else
                {
                    button.Background = Utils.GetSolidColorBrushFromHex(config.BackgroundColor);
                    button.Content = state.IsToggled && config.IsToggleable ? config.ToggledContent : config.Content;
                }


                button.Opacity = 1;
            }
            else
            {
                button.Background = Utils.GetSolidColorBrushFromHex(Colors.ButtonDisabledBgColor);
                button.Opacity = 0.5;
            }

            button.IsEnabled = isEnabled;
        }

        private static void ToggleButton(Button button)
        {
            var state = (ButtonState)button.Tag;
            state.IsToggled = !state.IsToggled;
            UpdateButtonState(button, button.IsEnabled);
        }
    }
}