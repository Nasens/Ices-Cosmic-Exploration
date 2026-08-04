using FFXIVClientStructs.FFXIV.Client.Game.Event;
using ICE.Utilities.Cosmic_Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.Ui.Debug_Tabs.Debug_Hud
{
    internal class Hud_Shop
    {
        public static void Draw()
        {
            // if (ImGui.Button("Update Shop"))
            if (ImGui.Button("更新商店"))
            {
                UpdateShop();
            }
        }

        public class ItemInfo
        {
            public string ItemName { get; set; }
            public string Quantity { get; set; }
            public string Set { get; set; }
            public uint IconId { get; set; }
            public uint ItemCount { get; set; }
            public uint ItemId { get; set; }
            public uint MaxStack { get; set; }
        }

        private static Dictionary<uint, ItemInfo> ShopItems = new();

        private static unsafe void UpdateShop()
        {
            var proxy = ShopEventHandler.AgentProxy.Instance();
            if (proxy == null || proxy->Handler == null)
            {
                // IceLogging.Info("Proxy or Handler was null");
                IceLogging.Info("代理或处理器为空");
                return;
            }

            var handler = proxy->Handler;

            for (int i = 0; i < handler->ItemsCount; i++)
            {
                ref var item = ref handler->Items[i];

                // IceLogging.Info($"[{i}] {item.ItemName} | Buy: {item.PriceBuy}g | Own: {item.NumOwned} | Stack: {item.StackSize}");
                IceLogging.Info($"[{i}] {item.ItemName} | 购买价：{item.PriceBuy}g | 持有：{item.NumOwned} | 堆叠：{item.StackSize}");
            }
        }
    }
}
