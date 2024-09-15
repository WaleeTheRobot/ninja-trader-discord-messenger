# NinjaTrader Discord Messenger

<img src="./images/screenshot.png" alt="Screenshot" style="display: block; margin: 0 auto">

Share your trades with a discord server. The Discord Messenger automatically takes a screenshot of the chart and sends the account position and active orders to a Discord webhook url everytime an order or position is updated.

# Important

Make sure you have a valid Discord webhook url, which can be setup in the settings section of the server under integrations.

The trading status will only send if you have real-time data. For example, using live data and the playback for Market Replay.

You will need to add this as a strategy in NinjaTrader. Note that the strategy will be disabled if you click the `Close` button on the Chart Trader.

Screenshot - Will only take the screenshot of the last chart that had an updated order. For example, it will send the screenshot of ES if you are trading NQ with Discord Messenger on it and then select a chart with ES for a new position.

# Developing/Usage

You'll find the Discord Messenger under the strategies. Enable it similar to how you will enable a strategy. Make sure to update the `Account Name` to the account you want to use to send the messages.

For developing, you can copy the DiscordMessenger folder into your local NinjaTrader AddOns folder.

For usage, you can download the zip containing the word import in the release page. You can import this zip file similar to importing a normal NinjaTrader Add-On. https://github.com/WaleeTheRobot/ninja-trader-discord-messenger/releases

# Issues

Sometimes NinjaTrader will complain about an import failed. You can just open the zip file from the release and copy the DiscordMessenger folder into the AddOns folder on your computer after removing the previous DiscordMessenger folder if it exists. It's normally located at: `C:\Users\<username>\Documents\NinjaTrader 8\bin\Custom\AddOns`. Afterwards, open NinjaTrader and click `New` > `NinjaScript Editor`. Click the NinjaScript Editor and press `F5`. It'll take a few seconds and you'll hear a sound. The icon at the bottom left corner of it will disappear when it's done compiling. Close the NinjaScript Editor and you should be good to go.

# Control Panel

#### Discord Webhook Status

This checks the status of the webhook every minute. Green indicates that it can successfully connect to it and red indicates that there is an issue with the webhook.

#### Trading Status Button

This allows the user to disable the script from automatically sending the trading status and screenshot to the Discord webhook.

#### Send Screenshot Button

This allows the user to send a screenshot to the Discord webhook.

#### Recent Events

This is a quick visual to show the last few recent events to the Discord webhook URL.

# Examples

<img src="./images/message.png" alt="Message" style="display: block; margin: 0 auto">

Screenshot displayed with the positions and active orders. Note that the active orders are arranged so it correlates visually with the screenshot.

<img src="./images/multiple-instruments.png" alt="Multiple Instruments" style="display: block; margin: 0 auto">

Multiple instruments are grouped together.
