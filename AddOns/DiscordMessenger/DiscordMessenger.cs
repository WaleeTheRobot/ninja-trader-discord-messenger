#region Using declarations
using NinjaTrader.Cbi;
using NinjaTrader.Custom.AddOns.DiscordMessenger;
using NinjaTrader.Custom.AddOns.DiscordMessenger.Configs;
using NinjaTrader.Custom.AddOns.DiscordMessenger.Services;
using NinjaTrader.Gui;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Media;
using System.Xml.Serialization;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
    public partial class DiscordMessenger : Indicator
    {
        public const string GROUP_NAME = "Discord Messenger";

        private Brush _embededColor;
        private bool _autoSend;
        private bool _orderUpdateTriggered;

        private EventManager _eventManager;

        #region Properties

        [NinjaScriptProperty]
        [Display(Name = "Version", Description = "Discord Messenger version.", Order = 0, GroupName = GROUP_NAME)]
        [ReadOnly(true)]
        public string Version
        {
            get { return "2.0.0"; }
            set { }
        }

        [NinjaScriptProperty]
        [Display(Name = "Webhook URLs", Description = "The URLs for your Discord server webhook. Separate a URL by a comma for multiple URLs.", Order = 1, GroupName = GROUP_NAME)]
        public string WebhookUrls { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Account Name", Description = "The account name used for the message.", Order = 2, GroupName = GROUP_NAME)]
        public string AccountName { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Screenshot Location", Description = "The location for the screenshot.", Order = 3, GroupName = GROUP_NAME)]
        public string ScreenshotLocation { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Embeded Color", Description = "The color for the embeded Discord message.", Order = 4, GroupName = GROUP_NAME)]
        public Brush EmbededColor
        {
            get { return _embededColor; }
            set { _embededColor = value; }
        }

        [Browsable(false)]
        public string EmbededColorSerialize
        {
            get { return Serialize.BrushToString(_embededColor); }
            set { _embededColor = Serialize.StringToBrush(value); }
        }

        #endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Send account position and active orders to Discord channels";
                Name = "_Discord Messenger";
                Calculate = Calculate.OnEachTick;
                IsOverlay = true;
                DisplayInDataBox = false;
                DrawOnPricePanel = false;
                DrawHorizontalGridLines = false;
                DrawVerticalGridLines = false;
                PaintPriceMarkers = false;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                IsSuspendedWhileInactive = true;

                // Properties
                WebhookUrls = "";
                AccountName = "Playback101";
                ScreenshotLocation = "C:\\screenshots";
                EmbededColor = Brushes.DodgerBlue;
            }
            else if (State == State.Configure)
            {
                _autoSend = true;
                _orderUpdateTriggered = false;

                Account account = Account.All.FirstOrDefault(a => a.Name == AccountName);

                if (account != null)
                {
                    account.OrderUpdate += OnOrderUpdate;

                    // Set initial config
                    SetConfig(account);

                    // Initialize manager and services
                    _eventManager = new EventManager();
                    _eventManager.OnPrintMessage += HandlePrintMessage;
                    _eventManager.OnWebhookStatusUpdated += HandleOnWebhookStatusUpdated;
                    _eventManager.OnAutoButtonClicked += HandleAutoButtonClicked;
                    _eventManager.OnTakeScreenshot += HandleScreenshot;

                    new TradingStatusService(_eventManager);
                    new DiscordMessengerService(_eventManager);
                    new WebhookCheckerService(_eventManager);
                }
                else
                {
                    Print("Account not found");
                }
            }
            else if (State == State.DataLoaded)
            {
                LoadControlPanel();
            }
            else if (State == State.Realtime)
            {
                if (_eventManager != null)
                {
                    _eventManager.StartWebhookChecker();
                }
            }
            else if (State == State.Terminated)
            {
                UnloadControlPanel();

                if (_eventManager != null)
                {
                    _eventManager.StopWebhookChecker();
                }
            }
        }

        public override string DisplayName
        {
            get
            {
                return "Discord Messenger";
            }
        }

        private void OnOrderUpdate(object sender, OrderEventArgs e)
        {
            _orderUpdateTriggered = true;
        }

        protected override void OnBarUpdate()
        {
            if (State != State.Realtime || !_autoSend)
            {
                return;
            }

            if (_orderUpdateTriggered)
            {
                _orderUpdateTriggered = false;
                _eventManager.UpdateOrderEntry();
            }
        }

        private void SetConfig(Account account)
        {
            // Ensure there's no trailing slash in the screenshot location
            string trimmedScreenshotLocation = ScreenshotLocation.TrimEnd('\\');

            Config.Instance.WebhookUrls = GetWebhookUrls();
            Config.Instance.Account = account;
            Config.Instance.AccountName = AccountName;
            Config.Instance.ScreenshotLocation = trimmedScreenshotLocation;
            Config.Instance.EmbededColor = EmbededColor;
        }

        private List<string> GetWebhookUrls()
        {
            return string.IsNullOrEmpty(WebhookUrls)
                ? new List<string>()
                : WebhookUrls.Split(',').Select(url => url.Trim()).ToList();
        }

        private void HandleAutoButtonClicked(bool isEnabled)
        {
            _autoSend = isEnabled;
        }

        // Used for debugging event messages
        private void HandlePrintMessage(string eventMessage)
        {
            Print(eventMessage);
        }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
    public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
    {
        private DiscordMessenger[] cacheDiscordMessenger;
        public DiscordMessenger DiscordMessenger()
        {
            return DiscordMessenger(Input);
        }

        public DiscordMessenger DiscordMessenger(ISeries<double> input)
        {
            if (cacheDiscordMessenger != null)
                for (int idx = 0; idx < cacheDiscordMessenger.Length; idx++)
                    if (cacheDiscordMessenger[idx] != null && cacheDiscordMessenger[idx].EqualsInput(input))
                        return cacheDiscordMessenger[idx];
            return CacheIndicator<DiscordMessenger>(new DiscordMessenger(), input, ref cacheDiscordMessenger);
        }
    }
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
    public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
    {
        public Indicators.DiscordMessenger DiscordMessenger()
        {
            return indicator.DiscordMessenger(Input);
        }

        public Indicators.DiscordMessenger DiscordMessenger(ISeries<double> input)
        {
            return indicator.DiscordMessenger(input);
        }
    }
}

namespace NinjaTrader.NinjaScript.Strategies
{
    public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
    {
        public Indicators.DiscordMessenger DiscordMessenger()
        {
            return indicator.DiscordMessenger(Input);
        }

        public Indicators.DiscordMessenger DiscordMessenger(ISeries<double> input)
        {
            return indicator.DiscordMessenger(input);
        }
    }
}

#endregion
