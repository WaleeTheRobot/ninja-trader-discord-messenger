#region Using declarations
using NinjaTrader.Cbi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
    public class Position
    {
        public string Instrument { get; set; }
        public int Quantity { get; set; }
        public double AveragePrice { get; set; }
        public string MarketPosition { get; set; }
    }

    public class OrderEntry
    {
        public string Instrument { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public string Type { get; set; }
        public string Action { get; set; }
    }

    public partial class DiscordMessenger : Strategy
    {
        public const string GROUP_NAME = "Discord Messenger";

        private Account _account;
        private List<Position> _positions;
        private List<OrderEntry> _orderEntries;
        private bool _orderUpdateTriggered;
        private bool _positionUpdateTriggered;
        private bool _sendMessage;

        #region Properties

        [NinjaScriptProperty]
        [Display(Name = "Version", Description = "Discord Messenger version.", Order = 0, GroupName = GROUP_NAME)]
        [ReadOnly(true)]
        public string Version
        {
            get { return "0.0.0"; }
            set { }
        }

        [NinjaScriptProperty]
        [Display(Name = "Webhook URL", Description = "The URL for your Discord server webhook.", Order = 1, GroupName = GROUP_NAME)]
        public string WebhookUrl { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Account Name", Description = "The account name used for the message.", Order = 2, GroupName = GROUP_NAME)]
        public string AccountName { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Screenshot Location", Description = "The location for the screenshot.", Order = 3, GroupName = GROUP_NAME)]
        public string ScreenshotLocation { get; set; }

        #endregion
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Send account position to Discord channel";
                Name = "Discord Messenger";
                Calculate = Calculate.OnEachTick;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = true;
                ExitOnSessionCloseSeconds = 30;
                IsFillLimitOnTouch = false;
                MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
                OrderFillResolution = OrderFillResolution.Standard;
                Slippage = 0;
                StartBehavior = StartBehavior.WaitUntilFlat;
                TimeInForce = TimeInForce.Gtc;
                TraceOrders = false;
                RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;
                StopTargetHandling = StopTargetHandling.PerEntryExecution;
                BarsRequiredToTrade = 20;
                // Disable this property for performance gains in Strategy Analyzer optimizations
                // See the Help Guide for additional information
                IsInstantiatedOnEachOptimizationIteration = true;

                // Properties
                WebhookUrl = "Your Discord Webhook URL";
                AccountName = "Playback101";
                ScreenshotLocation = "C:\\screenshots";
            }
            else if (State == State.Configure)
            {
                _positions = new List<Position>();
                _orderEntries = new List<OrderEntry>();
                _orderUpdateTriggered = true;
                _positionUpdateTriggered = true;
                _sendMessage = false;
                _account = Account.All.FirstOrDefault(a => a.Name == AccountName);

                if (_account != null)
                {
                    Print("Account found: " + _account.Name);

                    _account.OrderUpdate += OnOrderUpdate;
                    _account.PositionUpdate += OnPositionUpdate;
                }
                else
                {
                    Print("Account not found");
                }
            }
            else if (State == State.DataLoaded)
            {
                ControlPanelSetStateDataLoaded();
            }
            else if (State == State.Terminated)
            {
                ControlPanelSetStateTerminated();
            }
        }

        public override string DisplayName
        {
            get { return "Discord Messenger"; }
        }

        private void OnOrderUpdate(object sender, OrderEventArgs e)
        {
            _orderUpdateTriggered = true;
        }

        private void OnPositionUpdate(object sender, PositionEventArgs e)
        {
            _positionUpdateTriggered = true;
        }

        protected override void OnBarUpdate()
        {
            CheckPositions();
            CheckOrders();
            CheckSendMessage();
        }

        private void CheckPositions()
        {
            if (_positionUpdateTriggered)
            {
                int totalPositions = _account.Positions.Count;
                _positions = new List<Position>();
                _positionUpdateTriggered = false;
                _sendMessage = true;

                // No position
                if (totalPositions == 0)
                {
                    return;
                }

                for (int i = 0; i < totalPositions; i++)
                {
                    Position currentPosition = new Position
                    {
                        Instrument = _account.Positions[i].Instrument.MasterInstrument.Name,
                        Quantity = _account.Positions[i].Quantity,
                        AveragePrice = Math.Round(_account.Positions[i].AveragePrice, 2),
                        MarketPosition = _account.Positions[i].MarketPosition.ToString(),
                    };

                    _positions.Add(currentPosition);
                }
            }
        }

        private void CheckOrders()
        {
            if (_orderUpdateTriggered)
            {
                int totalOrders = _account.Orders.Count;
                _orderEntries = new List<OrderEntry>();
                _orderUpdateTriggered = false;
                _sendMessage = true;

                // No active orders
                if (totalOrders == 0)
                {
                    return;
                }

                for (int i = 0; i < totalOrders; i++)
                {
                    if (
                        _account.Orders[i].OrderState != OrderState.Accepted &&
                        _account.Orders[i].OrderState != OrderState.Working
                    )
                    {
                        continue;
                    }

                    double price;

                    // Check for proper price for limit order since order may have a price for both
                    if (
                        _account.Orders[i].OrderType == OrderType.StopLimit ||
                        _account.Orders[i].OrderType == OrderType.StopMarket
                    )
                    {
                        price = _account.Orders[i].StopPrice;
                    }
                    else
                    {
                        price = _account.Orders[i].LimitPrice;
                    }

                    // Check if an order with the same type and price already exists
                    var existingOrder = _orderEntries.FirstOrDefault(
                        entry => entry.Type == _account.Orders[i].OrderType.ToString() && entry.Price == price
                    );

                    if (existingOrder != null)
                    {
                        // Update the quantity if a matching order is found
                        existingOrder.Quantity += _account.Orders[i].Quantity;
                    }
                    else
                    {
                        // Add new order entry if no match is found
                        OrderEntry orderEntry = new OrderEntry
                        {
                            Instrument = _account.Orders[i].Instrument.MasterInstrument.Name,
                            Quantity = _account.Orders[i].Quantity,
                            Price = Math.Round(price, 2),
                            Type = _account.Orders[i].OrderType.ToString(),
                            Action = _account.Orders[i].OrderAction.ToString()
                        };

                        _orderEntries.Add(orderEntry);
                    }
                }

                // Sort descending order by price so it appears natural to the chart
                _orderEntries = _orderEntries.OrderByDescending(order => order.Price).ToList();
            }
        }

        private void CheckSendMessage()
        {
            if (_sendMessage)
            {
                _sendMessage = false;

                _ = SendMessageAsync((success, message) =>
                {
                    if (success)
                    {
                        AddEventLog("Success", "Trading Status Sent");
                        Print(message);
                    }
                    else
                    {
                        AddEventLog("Failed", "Trading Status Sent");
                        Print(message);
                    }
                });
            }
        }
    }
}
