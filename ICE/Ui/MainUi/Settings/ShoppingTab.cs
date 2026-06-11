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
                ImGui.Text("这是你可以自行创建的个性化购物清单，当你的点数达到一定数量时它就会执行。");
                ImGui.Text("以下各项的作用如下：");
                ImGui.BulletText("保留：会购买至多这么多的物品，以确保你的背包中有该数量。该数量在两次运行之间不会减少。\n" +
                                 "适用于像协奏药水这类你希望随时持有一定数量的物品");
                ImGui.BulletText("购买：会购买 X 个该物品，每次从商人处购买后，数字会递减直到归 0。\n" +
                                 "适合一次性购买，或只需特定数量的物品");
                ImGui.BulletText("持续购买：一旦其他两项（保留/购买）都满足后，只要还有足够的点数，它就会不停地购买此物品。\n" +
                                 "此项只能设置给 1 个物品，通常用于你只想用来花掉点数的东西");
                ImGui.EndTooltip();
            }
            ImGui.NewLine();

            int buyAtAmount = C.CosmoBuyAtAmount;
            int CosmoKeepAmount = C.CosmoKeepAmount;

            ImGui.SetNextItemWidth(150);
            if (ImGui.SliderInt("达到此数量时去购买物品", ref buyAtAmount, 0, 30000))
            {
                C.CosmoBuyAtAmount = buyAtAmount;
                C.SaveDebounced();
            }

            ImGui.SetNextItemWidth(150);
            if (ImGui.SliderInt("保留这么多 Cosmocredits", ref CosmoKeepAmount, 0, buyAtAmount))
            {
                C.CosmoKeepAmount = CosmoKeepAmount;
                C.SaveDebounced();
            }

            CheckConfigState();
            if (Task_BuyCosmoItems.CanPurchaseAnyItem())
            {
                ImGui.Text("你可以从清单中购买 cosmocredit 物品了！");
            }
            else
            {
                ImGui.Text("以你当前的点数/物品无法购买任何物品（没关系，这只是个测试）");
            }

            if (ImGui.Button("添加 材料/染料/物品"))
            {
                ImGui.OpenPopup("CosmocreditMateriaPopup");
            }

            ImGui.SameLine();

            if (ImGui.Button("添加 防具/家具/坐骑"))
            {
                ImGui.OpenPopup("Cosmocredit_MountArmorPopup");
            }

            ImGui.SameLine();
            
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

            DrawShoppingTable("防具/家具/坐骑", Shop_Cosmocredits.Shop_MountsCards, C.CosmoShoppingOrder_Gear, GearDragDrop);

            // Draw separate tables for each shop type

            ImGui.NewLine();

            DrawShoppingTable("材料/染料/物品", Shop_Cosmocredits.Shop_MateriaDye, C.CosmoShoppingOrder, MaterialDragDrop);
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
                    ImGui.TableSetupColumn("Icons", ImGuiTableColumnFlags.WidthFixed, 20);
                    ImGui.TableSetupColumn("Names", ImGuiTableColumnFlags.WidthStretch);

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
                    ImGui.TableSetupColumn("Icons", ImGuiTableColumnFlags.WidthFixed, 20);
                    ImGui.TableSetupColumn("Names", ImGuiTableColumnFlags.WidthStretch);

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
                ImGui.TextDisabled($"{tableName} 购物清单中没有物品");
                return;
            }

            ImGui.Text($"{tableName}（{orderList.Count} 个物品）");

            dragDrop.Begin();

            if (ImGui.BeginTable($"Shopping_{tableName}", 10, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders))
            {
                ImGui.TableSetupColumn("Order", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Name");
                ImGui.TableSetupColumn("Have", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Cost", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Kind", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Unlocked", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Keep", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Buy", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Keep Buying", ImGuiTableColumnFlags.WidthFixed);
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