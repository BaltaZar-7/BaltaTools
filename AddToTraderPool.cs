#nullable disable
using ExpandedTradingFramework.Framework;
using Il2CppTLD.Trader;

namespace BaltaTools
{
    internal static class BaltaToolsTradeList
    {
        private static readonly string[] CuredHides =
        [
            "GEAR_BearHideDried",
            "GEAR_CougarHideDried",
            "GEAR_LeatherHideDried",
            "GEAR_MooseHideDried",
            "GEAR_RabbitPeltDried",
            "GEAR_WolfPeltDried"
        ];

        internal static readonly CustomTradeDefinition[] Trades =
        [
            CreateSurvivalHatchetTrade(),
            CreateFeatherPluckerTrade(),
        ];

        internal static CustomTradeDefinition[] GetEnabledTrades()
        {
            List<CustomTradeDefinition> enabledTrades = [];
            for (int i = 0; i < Trades.Length; i++)
            {
                CustomTradeDefinition trade = Trades[i];
                if (trade != null && trade.Enabled) enabledTrades.Add(trade);
            }
            return [.. enabledTrades];
        }

        private static CustomTradeDefinition CreateSurvivalHatchetTrade()
        {
            const string tradeId = "BT_SurvivalHatchet";
            const string rewardGearName = "GEAR_SurvivalHatchet";
            const int rewardAmount = 1;
            return new CustomTradeDefinition
            {
                Id = tradeId,
                Enabled = true,
                Description = "BaltaTools trade: Survival Hatchet.",
                IsSpecialRequest = false,
                SelectionMode = CustomTradeSelectionMode.DrawPool,
                DrawPool = CustomTradeDrawPool.Rare,
                MinTrust = 400,
                MaxPerTrade = 1,
                Repeatable = true,
                CostItems =
                [
                    GearAmount("GEAR_Knife", 1),
                    GearAmount("GEAR_MagnifyingLens", 1),
                    CategoryAmount("Cured Hides", CuredHides, 4, CategoryIcon.AnyHide),
                ],
                DisplayReward = CustomTradeDisplay.Gear(rewardGearName, rewardAmount),
                Reward = CustomTradeReward.Gear(rewardGearName, rewardAmount, CustomRewardStacking.Auto)
            };
        }

        private static CustomTradeDefinition CreateFeatherPluckerTrade()
        {
            const string tradeId = "BT_FeatherPlucker";
            const string rewardGearName = "GEAR_FeatherPlucker";
            const int rewardAmount = 1;
            return new CustomTradeDefinition
            {
                Id = tradeId,
                Enabled = true,
                Description = "BaltaTools trade: Feather Plucker.",
                IsSpecialRequest = false,
                SelectionMode = CustomTradeSelectionMode.DrawPool,
                DrawPool = CustomTradeDrawPool.Rare,
                MinTrust = 300,
                MaxPerTrade = 1,
                Repeatable = true,
                CostItems =
                [
                    GearAmount("GEAR_PtarmiganFeathers", 20),
                    GearAmount("GEAR_CougarClaw", 1),
                ],
                DisplayReward = CustomTradeDisplay.Gear(rewardGearName, rewardAmount),
                Reward = CustomTradeReward.Gear(rewardGearName, rewardAmount, CustomRewardStacking.Auto)
            };
        }

        private static CustomTradeItem GearAmount(string gearName, int amount, CustomTradeIcon displayIcon = null)
        {
            return new CustomTradeItem
            {
                ExchangeType = ExchangeItemType.GearAmount,
                GearName = gearName,
                Amount = amount,
                DisplayIcon = displayIcon ?? CustomTradeIcon.None()
            };
        }

        private static CustomTradeItem CategoryAmount(string categoryName, string[] gearNames, int amount, CategoryIcon categoryIcon)
        {
            return new CustomTradeItem
            {
                ExchangeType = ExchangeItemType.CategoryAmount,
                CategoryName = categoryName,
                CategoryGearNames = gearNames,
                CategoryIcon = categoryIcon,
                Amount = amount
            };
        }
    }
}