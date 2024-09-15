using System.Windows.Media;

namespace NinjaTrader.Custom.AddOns.DiscordMessenger
{
    public static class Utils
    {
        public static SolidColorBrush GetSolidColorBrushFromHex(string hexColor)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hexColor));
        }
    }
}
