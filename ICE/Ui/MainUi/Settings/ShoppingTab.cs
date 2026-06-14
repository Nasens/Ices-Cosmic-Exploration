using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using Lumina.Excel.Sheets;
using System.Collections.Generic;
using static ICE.ConfigFiles.Config;

namespace ICE.Ui.MainUi.Settings
{
    internal class ShoppingTab
    {
        private static string ItemSearch = string.Empty;
        private static ImGuiEx.RealtimeDragDrop<uint> MaterialDragDrop = new("MaterialShop", (id) => id.ToString());
        private static ImGuiEx.RealtimeDragDrop<uint> GearDragDrop = new("GearShop", (id) => id.ToString());

        public static unsafe void Draw()
        {
            float minContentWidth = 920 * ImGuiHelpers.GlobalScale;
            var availWidth = ImGui.GetContentRegionAvail().X;
            if (availWidth < minContentWidth)
                ImGui.SetNextWindowContentSize(new Vector2(minContentWidth, 0));

            using var scrollChild = ImRaii.Child("##shoppingTabScroll", new Vector2(0, 0), false, ImGuiWindowFlags.HorizontalScrollbar);
            if (!scrollChild.Success) return;

            bool BuyItems = C.BuyItems;

            // if (ImGui.Checkbox("Buy Items", ref BuyItems))
            if (ImGui.Checkbox("购买物品", ref BuyItems))
            {
                C.BuyItems = BuyItems;
                C.StopOnceHitCosmoCredits = false;
                C.Save();
            }
            ImGui.SameLine();
            ImGuiEx.Icon(FontAwesomeIcon.QuestionCircle);
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                // ImGui.Text("This is your personalized shopping list that you can create that it will run when you hit a certain amount of credits.");
                ImGui.Text("这是你的个性化购物清单，达到指定点数后将自动购买。");
                // ImGui.Text("Here's what each of the following does:");
                ImGui.Text("各项含义如下：");
                // ImGui.BulletText("Keep: Will buy up to that many items to make sure that you have in your inventory. This count doesn't go down between runs.\n" +
                //                  "Useful for things like cordials where you want to always have a certain amount on hand");
                ImGui.BulletText("保留：购买至该数量以确保背包持有。运行间不会减少。\n" +
                                 "适用于 cordial 等需常备的物品");
                // ImGui.BulletText("Buy: Will buy X amount of those items, as it buys it from the vendor, the number will decrease until it hits 0.\n" +
                //                  "Good for one off buys, or something that you only need a particular amount of");
                ImGui.BulletText("购买：购买指定数量，从商人处购入后递减至 0。\n" +
                                 "适用于一次性或仅需固定数量的物品");
                // ImGui.BulletText("Keep Buying: Once the other 2 have been met (Keep/Buy), it will constantly buy this item if it has the credits to do so.\n" +
                //                  "This can only be set to 1 item, and gererally used for things you want to just spend your credits on");
                ImGui.BulletText("持续购买：满足保留/购买后，若有点数则持续购入。\n" +
                                 "仅可设 1 项，一般用于消耗剩余点数");
                ImGui.EndTooltip();
            }
            ImGui.NewLine();

            int buyAtAmount = C.CosmoBuyAtAmount;
            int CosmoKeepAmount = C.CosmoKeepAmount;

            ImGui.SetNextItemWidth(150);
            // if (ImGui.SliderInt("Go buy items when you reach", ref buyAtAmount, 0, 30000))
            if (ImGui.SliderInt("达到以下点数时开始购买", ref buyAtAmount, 0, 30000))
            {
                C.CosmoBuyAtAmount = buyAtAmount;
                C.SaveDebounced();
            }

            ImGui.SetNextItemWidth(150);
            // if (ImGui.SliderInt("Keep this much Cosmocredits", ref CosmoKeepAmount, 0, buyAtAmount))
            if (ImGui.SliderInt("保留宇宙点数数量", ref CosmoKeepAmount, 0, buyAtAmount))
            {
                C.CosmoKeepAmount = CosmoKeepAmount;
                C.SaveDebounced();
            }

            CheckConfigState();
            if (Task_BuyCosmoItems.CanPurchaseAnyItem())
            {
                // ImGui.Text("You can buy cosmocredit items from the list!");
                ImGui.Text("当前可购买清单中的宇宙点数物品！");
            }
            else
            {
                // ImGui.Text("You can't buy any items with your current credit value/items (tis fine, this just a test)");
                ImGui.Text("以当前点数/物品无法购买任何项目（无妨，仅为测试）");
            }

            // if (ImGui.Button("Add Material/Dyes/Items"))
            if (ImGui.Button("添加材料/染色/物品"))
            {
                ImGui.OpenPopup("CosmocreditMateriaPopup");
            }

            ImGui.SameLine();

            // if (ImGui.Button("Add Armor/Housing/Mounts"))
            if (ImGui.Button("添加防具/家具/坐骑"))
            {
                ImGui.OpenPopup("Cosmocredit_MountArmorPopup");
            }

            ImGui.SameLine();
            
            // if (ImGui.Button("Clear shopping list"))
            if (ImGui.Button("清空购物清单"))
            {
                C.CosmoShopping.Clear();
                C.CosmoShoppingOrder.Clear();
                C.CosmoShoppingOrder_Gear.Clear();

                C.Save();
            }

            DrawAddItemPopups();

            ImGui.Separator();
            ImGui.NewLine();

            // DrawShoppingTable("Armor/Housing/Mounts", Shop_Cosmocredits.Shop_MountsCards, C.CosmoShoppingOrder_Gear, GearDragDrop);
            DrawShoppingTable("防具/家具/坐骑", Shop_Cosmocredits.Shop_MountsCards, C.CosmoShoppingOrder_Gear, GearDragDrop);

            // Draw separate tables for each shop type

            ImGui.NewLine();

            // DrawShoppingTable("Materials/Dyes/Items", Shop_Cosmocredits.Shop_MateriaDye, C.CosmoShoppingOrder, MaterialDragDrop);
            DrawShoppingTable("材料/染色/物品", Shop_Cosmocredits.Shop_MateriaDye, C.CosmoShoppingOrder, MaterialDragDrop);
        }

        private static void DrawAddItemPopups()
        {
            ImGui.SetNextWindowSize(new Vector2(400, 0), ImGuiCond.Appearing);

            if (ImGui.BeginPopup("CosmocreditMateriaPopup"))
            {
                ImGui.SetNextItemWidth(380);
                ImGui.InputText("##Item Search", ref ItemSearch, 256);

                ImGui.Spacing();

                if (ImGui.BeginTable("Cosmo Materia Shop", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.ScrollY | ImGuiTableFlags.RowBg, new Vector2(0, 250)))
                {
                    // ImGui.TableSetupColumn("Icons", ImGuiTableColumnFlags.WidthFixed, 20);
                    // ImGui.TableSetupColumn("Names", ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableSetupColumn("图标", ImGuiTableColumnFlags.WidthFixed, 20);
                    ImGui.TableSetupColumn("名称", ImGuiTableColumnFlags.WidthStretch);

                    foreach (var item in Shop_Cosmocredits.Shop_MateriaDye)
                    {
                        DrawShopItemRow(item.Key, C.CosmoShoppingOrder);
                    }
                    ImGui.EndTable();
                }

                ImGui.EndPopup();
            }

            if (ImGui.BeginPopup("Cosmocredit_MountArmorPopup"))
            {
                ImGui.SetNextItemWidth(380);
                ImGui.InputText("##Item Search2", ref ItemSearch, 256);

                ImGui.Spacing();

                if (ImGui.BeginTable("Cosmo Gear Shop", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.ScrollY | ImGuiTableFlags.RowBg, new Vector2(0, 250)))
                {
                    // ImGui.TableSetupColumn("Icons", ImGuiTableColumnFlags.WidthFixed, 20);
                    // ImGui.TableSetupColumn("Names", ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableSetupColumn("图标", ImGuiTableColumnFlags.WidthFixed, 20);
                    ImGui.TableSetupColumn("名称", ImGuiTableColumnFlags.WidthStretch);

                    foreach (var item in Shop_Cosmocredits.Shop_MountsCards)
                    {
                        DrawShopItemRow(item.Key, C.CosmoShoppingOrder_Gear);
                    }
                    ImGui.EndTable();
                }

                ImGui.EndPopup();
            }
        }

        private static void DrawShopItemRow(uint id, List<uint> orderList)
        {
            if (Svc.Data.GetExcelSheet<Item>().TryGetRow(id, out var itemInfo))
            {
                var name = itemInfo.Name.ToString();

                if (!ItemSearch.IsNullOrWhitespace() && !name.ToLower().Contains(ItemSearch.ToLower()))
                {
                    return;
                }

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.PushID(id);

                if (itemInfo.Icon is { } itemIcon && Svc.Texture.TryGetFromGameIcon((int)itemIcon, out var texture))
                {
                    ImGui.Image(texture.GetWrapOrEmpty().Handle, new Vector2(20, 20));
                }

                ImGui.TableNextColumn();
                ImGui.Text($"{itemInfo.Name}");

                if (ImGui.IsItemHovered() && ImGui.IsItemClicked(ImGuiMouseButton.Left))
                {
                    AddItemToList(id, orderList);
                    C.Save();
                }

                ImGui.PopID();
            }
        }

        private static void DrawShoppingTable(string tableName, Dictionary<uint, Shop_Cosmocredits.ItemInfo> shopData, List<uint> orderList, ImGuiEx.RealtimeDragDrop<uint> dragDrop)
        {
            if (orderList.Count == 0)
            {
                // ImGui.TextDisabled($"No items in {tableName} shopping list");
                ImGui.TextDisabled($"{tableName} 购物清单为空");
                return;
            }

            // ImGui.Text($"{tableName} ({orderList.Count} items)");
            ImGui.Text($"{tableName}（{orderList.Count} 项）");

            dragDrop.Begin();

            if (ImGui.BeginTable($"Shopping_{tableName}", 10, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders))
            {
                // ImGui.TableSetupColumn("Order", ImGuiTableColumnFlags.WidthFixed);
                // ImGui.TableSetupColumn("Name");
                // ImGui.TableSetupColumn("Have", ImGuiTableColumnFlags.WidthFixed);
                // ImGui.TableSetupColumn("Cost", ImGuiTableColumnFlags.WidthFixed);
                // ImGui.TableSetupColumn("Kind", ImGuiTableColumnFlags.WidthFixed);
                // ImGui.TableSetupColumn("Unlocked", ImGuiTableColumnFlags.WidthFixed);
                // ImGui.TableSetupColumn("Keep", ImGuiTableColumnFlags.WidthFixed);
                // ImGui.TableSetupColumn("Buy", ImGuiTableColumnFlags.WidthFixed);
                // ImGui.TableSetupColumn("Keep Buying", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("顺序", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("名称");
                ImGui.TableSetupColumn("持有", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("价格", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("类型", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("已解锁", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("保留", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("购买", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("持续购买", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);

                ImGui.TableHeadersRow();

                for (int i = 0; i < orderList.Count; i++)
                {
                    uint itemId = orderList[i];
                    DrawShoppingItemRow(itemId, shopData, i, orderList, dragDrop);
                }

                ImGui.EndTable();
            }

            dragDrop.End();
        }

        private static void DrawShoppingItemRow(uint itemId, Dictionary<uint, Shop_Cosmocredits.ItemInfo> shopData, int index, List<uint> orderList, ImGuiEx.RealtimeDragDrop<uint> dragDrop)
        {
            var setting = C.CosmoShopping[itemId];
            var itemInfo = Svc.Data.GetExcelSheet<Item>().GetRow(itemId);

            ImGui.TableNextRow();
            dragDrop.NextRow();
            dragDrop.SetRowColor(itemId);

            ImGui.PushID(itemId);

            // Drag/Drop Handle - MUCH SIMPLER NOW!
            ImGui.TableSetColumnIndex(0);
            dragDrop.DrawButtonDummy(itemId, orderList, index);

            // Name
            ImGui.TableNextColumn();
            if (itemInfo.Icon is { } itemIcon && Svc.Texture.TryGetFromGameIcon((int)itemIcon, out var texture))
            {
                ImGui.Image(texture.GetWrapOrEmpty().Handle, new Vector2(24, 24));
                ImGui.SameLine();
            }
            ImGui.Text($"{itemInfo.Name}");

            // Have
            ImGui.TableNextColumn();
            PlayerHelper.GetItemCount(itemId, out var count);
            ImGui.Text($"{count}");

            // Cost
            ImGui.TableNextColumn();
            if (shopData.TryGetValue(itemId, out var shopInfo))
            {
                ImGui.Text($"{shopInfo.Cost:N0}");
            }

            // Kind
            ImGui.TableNextColumn();
            string kind = itemInfo.ItemUICategory.Value.Name.ToString();
            ImGui.Text(kind);

            // Unlocked (for consumable items like mounts, orchestrion rolls, cards, etc.)
            ImGui.TableNextColumn();
            // ImGui.TextUnformatted(UnlockState.IsItemUnlockable(itemInfo) ? UnlockState.IsItemUnlocked(itemInfo) ? "Yes" : "No" : "-");
            ImGui.TextUnformatted(UnlockState.IsItemUnlockable(itemInfo) ? UnlockState.IsItemUnlocked(itemInfo) ? "是" : "否" : "-");

            // Keep Amount
            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(80);
            var keepAmount = setting.KeepAmount;
            if (ImGui.InputInt($"##keep_{itemId}", ref keepAmount))
            {
                setting.KeepAmount = Math.Max(0, keepAmount);
                C.SaveDebounced();
            }

            // Buy Amount
            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(80);
            var buyAmount = setting.BuyAmount;
            if (ImGui.InputInt($"##buy_{itemId}", ref buyAmount))
            {
                setting.BuyAmount = Math.Max(0, buyAmount);
                C.SaveDebounced();
            }

            // Keep Buying
            ImGui.TableNextColumn();
            var keepBuying = setting.KeepBuying;
            if (ImGui.Checkbox($"##keepbuying_{itemId}", ref keepBuying))
            {
                foreach (var enabled in C.CosmoShopping)
                {
                    enabled.Value.KeepBuying = false;
                }

                setting.KeepBuying = keepBuying;
                C.Save();
            }

            // Remove Button
            ImGui.TableNextColumn();
            if (ImGuiEx.IconButton(FontAwesomeIcon.Trash, $"##remove_{itemId}"))
            {
                RemoveItem(itemId, orderList);
                C.Save();
            }

            ImGui.PopID();
        }

        private static void AddItemToList(uint itemId, List<uint> orderList)
        {
            if (C.CosmoShopping.ContainsKey(itemId))
                return;

            C.CosmoShopping[itemId] = new CosmoShoppingList();
            orderList.Add(itemId);
        }

        private static void RemoveItem(uint itemId, List<uint> orderList)
        {
            C.CosmoShopping.Remove(itemId);
            orderList.Remove(itemId);
        }

        public static void CheckConfigState()
        {
            if (C.CosmoShopping == null)
            {
                C.CosmoShopping = new();
                C.Save();
            }
            if (C.CosmoShoppingOrder == null)
            {
                C.CosmoShoppingOrder = new();
                C.Save();
            }
            if (C.CosmoShoppingOrder_Gear == null)
            {
                C.CosmoShoppingOrder_Gear = new();
                C.Save();
            }
        }
    }
}
