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
        public string BackgroundColor { get; set; }
        public string HoverBackgroundColor { get; set; }
        public string TextColor { get; set; }
        public Delegate ClickHandler { get; set; }
    }

    public static class ButtonUtils
    {
        public static Button GetButton(ButtonConfig config)
        {
            Style customButtonStyle = CreateCustomButtonStyle(config.BackgroundColor, config.HoverBackgroundColor);

            Button button = new Button
            {
                Content = config.Content,
                FontSize = 16,
                Visibility = Visibility.Visible,
                Foreground = Utils.GetSolidColorBrushFromHex(config.TextColor),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Style = customButtonStyle
            };

            if (config.ClickHandler != null)
            {
                button.Click += async (sender, e) =>
                {
                    // Check if the handler is a synchronous Action
                    if (config.ClickHandler is Action<object, RoutedEventArgs> syncHandler)
                    {
                        syncHandler(sender, e);
                    }
                    // Check if the handler is an asynchronous Func
                    else if (config.ClickHandler is Func<object, RoutedEventArgs, Task> asyncHandler)
                    {
                        await asyncHandler(sender, e);
                    }
                };
            }

            // Add an event handler for the IsEnabledChanged event
            button.IsEnabledChanged += (sender, e) => UpdateButtonState(button, button.IsEnabled);

            return button;
        }

        public static Style CreateCustomButtonStyle(string hexBgColor, string hexBgColorHover)
        {
            Style style = new Style(typeof(Button));

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

            // Define triggers for different states
            Trigger mouseover = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            mouseover.Setters.Add(new Setter(Button.BackgroundProperty, Utils.GetSolidColorBrushFromHex(hexBgColorHover)));

            Trigger pressed = new Trigger { Property = Button.IsPressedProperty, Value = true };
            pressed.Setters.Add(new Setter(Button.BackgroundProperty, Utils.GetSolidColorBrushFromHex(hexBgColorHover)));

            Trigger disabledTrigger = new Trigger { Property = Button.IsEnabledProperty, Value = false };
            disabledTrigger.Setters.Add(new Setter(Button.BackgroundProperty, Utils.GetSolidColorBrushFromHex(Colors.ButtonDisabledBgColor)));
            disabledTrigger.Setters.Add(new Setter(Button.OpacityProperty, 0.5));

            // Add triggers to the template
            template.Triggers.Add(mouseover);
            template.Triggers.Add(pressed);
            template.Triggers.Add(disabledTrigger);

            // Set the template
            style.Setters.Add(new Setter(Button.TemplateProperty, template));

            // Set default property values
            style.Setters.Add(new Setter(Button.BackgroundProperty, Utils.GetSolidColorBrushFromHex(hexBgColor)));
            style.Setters.Add(new Setter(Button.BorderBrushProperty, Brushes.Transparent));
            style.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(0)));
            style.Setters.Add(new Setter(Button.PaddingProperty, new Thickness(3)));
            style.Setters.Add(new Setter(Button.MarginProperty, new Thickness(3)));

            return style;
        }

        public static void UpdateButtonState(Button button, bool isEnabled)
        {
            if (isEnabled)
            {
                button.Background = Utils.GetSolidColorBrushFromHex(Colors.ButtonBgColor);
                button.Opacity = 1;
            }
            else
            {
                button.Background = Utils.GetSolidColorBrushFromHex(Colors.ButtonDisabledBgColor);
                button.Opacity = 0.5;
            }

            button.IsEnabled = isEnabled;
        }
    }
}
