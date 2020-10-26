﻿using GemTracker.Shared.Domain.DTOs;
using GemTracker.Shared.Extensions;
using System;
using Telegram.Bot.Types.ReplyMarkups;

namespace GemTracker.Shared.Domain
{
    public class Msg
    {
        public static string ForTwitter(Gem gem)
        {
            var emoji = gem.Recently == TokenAction.ADDED
                ? "✅"
                : "❌";

            var result =
                $"{emoji} {gem.Recently.GetDescription()} - Uniswap\n\n" +
                $"💎 Token: {gem.Name}\n" +
                $"🚨 Symbol: ${gem.Symbol}\n" +
                $"🦄 Uniswap: https://uniswap.info/token/{gem.Id} \n" +
                $"🔎 EthScan: https://etherscan.io/token/{gem.Id} \n" +
                $"Join: https://t.me/GemTrackerClub \n" +
                $"( $BTC $ETH $ALTS $UNI #uniswap #cryptocurrency #gem)\n";

            return result;
        }

        private static string RecentlyEmoji(Gem gem)
            => gem.Recently == TokenAction.ADDED
                ? "✅"
                : "❌";

        private static InlineKeyboardButton[] UniswapButtons(Gem gem)
        {
            var buyOrNot
                = gem.Recently == TokenAction.ADDED
                ? InlineKeyboardButton.WithUrl("📉 Buy", $"https://app.uniswap.org/#/swap?outputCurrency={gem.Id}")
                : InlineKeyboardButton.WithUrl("🛑 DON'T Buy it", $"https://uniswap.info/");

            return new[]
                {
                    InlineKeyboardButton.WithUrl("📋 Info", $"https://uniswap.info/token/{gem.Id}"),
                    buyOrNot,
                    InlineKeyboardButton.WithUrl("📈 Sell", $"https://app.uniswap.org/#/swap?inputCurrency={gem.Id}"),
                };
        }

        private static InlineKeyboardButton[] EtherscanButtons(Gem gem)
            => new[]
                {
                    InlineKeyboardButton.WithUrl("🔎 EthScan", $"https://etherscan.io/token/{gem.Id}"),
                    InlineKeyboardButton.WithUrl("📋 Contract", $"https://etherscan.io/address/{gem.Id}"),
                    InlineKeyboardButton.WithUrl("🤑 Hodlers", $"https://etherscan.io/token/{gem.Id}#balances"),
                };

        private static string Content(Gem gem)
        {
            var networkEffectVisible
                = gem.Recently == TokenAction.ADDED
                ?
                $"📣 *Network effect:*\n" +
                $"Twitter: [${gem.Symbol}](https://twitter.com/search?q=%24{gem.Symbol}) | [{gem.Name}](https://twitter.com/search?q={gem.Name})\n" +
                $"Reddit:  [${gem.Symbol}](https://www.reddit.com/search/?q=%24{gem.Symbol}) | [{gem.Name}](https://www.reddit.com/search/?q={gem.Name})\n" +
                $"4chan:   [${gem.Symbol}](https://boards.4channel.org/biz/catalog#s=%24{gem.Symbol}) | [{gem.Name}](https://boards.4channel.org/biz/catalog#s={gem.Name})\n\n"
                :
                string.Empty;

            var statisticsVisible
                = gem.Recently == TokenAction.ADDED
                ?
                $"🧮 *Statistics*\n" +
                $"EthPlorer [{gem.Id}](https://ethplorer.io/address/{gem.Id})\n" +
                $"blockchair [{gem.Id}](https://blockchair.com/ethereum/erc-20/token/{gem.Id}?from=gemtracker)\n\n"
                :
                string.Empty;

            var chartsVisible
                = gem.Recently == TokenAction.ADDED
                ?
                $"📊 *Charts*\n" +
                $"ChartEx [${gem.Symbol}](https://chartex.pro/?symbol=UNISWAP:{gem.Symbol}) |" +
                $" [${gem.Symbol}/$WETH](https://chartex.pro/?symbol=UNISWAP:{gem.Symbol}/WETH)\n\n"
                :
                string.Empty;

            var warningAfterDelete
                = gem.Recently == TokenAction.DELETED
                ?
                $"🛑 *WARNING*\n" +
                $"Address: [{gem.Id}](https://etherscan.io/token/{gem.Id}) \n\n" +
                $"Make sure to delete the allowance using fe. [revoke.cash](https://revoke.cash)\n\n"
                :
                string.Empty;

            var result =
                $"{RecentlyEmoji(gem)} *{gem.Recently.GetDescription()}* - Uniswap (v2)\n\n" +
                $"🦄 Uniswap\n" +
                $"💎 Token: *{gem.Name}*\n" +
                $"🚨 Symbol: *{gem.Symbol}*\n\n" +
                networkEffectVisible +
                statisticsVisible +
                chartsVisible +
                warningAfterDelete;

            return result;
        }

        public static Tuple<IReplyMarkup, string> ForPremiumTelegram(Gem gem)
        {
            var banner =
                Content(gem);

            var buttons = new InlineKeyboardMarkup(new[]
            {
                UniswapButtons(gem),
                EtherscanButtons(gem),
                new []
                {
                    InlineKeyboardButton.WithUrl("📧 Support", $"https://t.me/tkowalczyk")
                }
            });

            return new Tuple<IReplyMarkup, string>(buttons, banner);
        }
        public static Tuple<IReplyMarkup, string> ForFreeTelegram(Gem gem)
        {
            var banner =
                Content(gem) +
                $"👨‍👦‍👦 Our community:\n" +
                $"Chat - @GemTrackerCommunity\n" +
                $"Info - @GemTrackerAnnouncements\n";

            var buttons = new InlineKeyboardMarkup(new[]
            {
                UniswapButtons(gem),
                EtherscanButtons(gem),
                new []
                {
                    InlineKeyboardButton.WithUrl("⚙️ How to use?", $"https://gemtracker.club/#howtouse"),
                    InlineKeyboardButton.WithUrl("💰 Premium access?", $"https://gemtracker.club/#premium")
                },
                new []
                {
                    InlineKeyboardButton.WithUrl("📧 Author", $"https://twitter.com/tomkowalczyk")
                }
            });
            return new Tuple<IReplyMarkup, string>(buttons, banner);
        }
    }
}