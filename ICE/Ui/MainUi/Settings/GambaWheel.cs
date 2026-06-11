using Dalamud.Interface.Utility;
using ECommons.GameHelpers;
using ICE.Utilities.Cosmic_Helper;
using Lumina.Excel.Sheets;
using static ICE.ConfigFiles.Config;

namespace ICE.Ui.MainUi.Settings
{
    internal class GambaWheel
    {
        private static bool gambaEnabled = C.GambaEnabled;
        private static int gambaDelay = C.GambaDelay;
        private static int gambaCreditsMinimum = C.GambaCreditsMinimum;
        private static bool gambaPreferSmallerWheel = C.GambaPreferSmallerWheel;

        public static unsafe void Draw_Old()
        {
            if (ImGui.Checkbox("启用自动赌博", ref gambaEnabled))
            {
                C.GambaEnabled = gambaEnabled;
                C.Save();
            }
            ImGuiEx.HelpMarker("如果你想让它自动选择轮盘并进行赌博，请启用此项。如果你在运行赌博轮盘时不想让它自动运行，请禁用此项。");
            ImGui.SetNextItemWidth(150);
            if (ImGui.SliderInt("保留的最低点数", ref gambaCreditsMinimum, 0, 10000))
            {
                C.GambaCreditsMinimum = gambaCreditsMinimum;
                C.SaveDebounced();
            }
            bool gambaBetween = C.GambaBetweenRuns;
            if (ImGui.Checkbox("在两次运行之间赌博", ref gambaBetween))
            {
                C.GambaBetweenRuns = gambaBetween;
                C.Save();
            }
            ImGui.SameLine();
            GambaSlider();
            ImGui.SetNextItemWidth(150);
            if (ImGui.SliderInt("赌博延迟", ref gambaDelay, 50, 2000))
            {
                C.GambaDelay = gambaDelay;
                C.SaveDebounced();
            }

            if (ImGui.Checkbox("偏好较小的轮盘", ref gambaPreferSmallerWheel))
            {
                C.GambaPreferSmallerWheel = gambaPreferSmallerWheel;
                C.Save();
            }
            ImGuiEx.HelpMarker("这会让赌博偏好物品较少的轮盘。");

            if (PlayerHelper.IsInCosmicZone())
            {
                var territory = Player.Territory.RowId;
                if (CosmicMoonRegistry.TryGetPlanetCreditItemId(territory, out var itemId))
                {
                    PlayerHelper.GetItemCount(itemId, out var credits);
                    ImGui.Text($"当前位置：{territory} | 货币数量：{credits}");
                }
            }

            ImGui.Separator();
            ImGui.TextUnformatted("配置赌博中每个物品的权重。权重越高 = 越想要。");
            ImGui.Spacing();
            foreach (GambaType type in Enum.GetValues(typeof(GambaType)))
            {
                var itemsType = C.GambaItemWeights.Where(x => x.Type == type).OrderBy(x => x.ItemId).ToList();
                if (itemsType.Count == 0) continue;
                if (ImGui.TreeNodeEx($"{type} ({itemsType.Count})##gamba_type_{type}", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.Indent();
                    foreach (var gamba in itemsType)
                    {
                        var itemName = ExcelItemHelper.GetName(gamba.ItemId);
                        int weight = gamba.Weight;
                        ImGui.SetNextItemWidth(120f);
                        if (ImGui.InputInt($"[{gamba.ItemId}] {itemName}##gamba_weight", ref weight))
                        {
                            gamba.Weight = weight;
                            C.Save();
                        }
                    }
                    ImGui.Unindent();
                    ImGui.TreePop();
                }
            }
            if (ImGui.Button("重置权重"))
            {
                Task_Gamba.EnsureGambaWeightsInitialized(true);
            }
        }

        public static unsafe void Draw()
        {
            bool gambaEnabled = C.GambaEnabled;
            if (ImGui.Checkbox("启用自动赌博轮盘", ref gambaEnabled))
            {
                C.GambaEnabled = gambaEnabled;
                C.Save();
            }
            ImGuiEx.HelpMarker("如果你想让它自动选择轮盘并进行赌博，请启用此项。如果你在运行赌博轮盘时不想让它自动运行，请禁用此项。");
            ImGui.SetNextItemWidth(150);
            if (ImGui.SliderInt("保留的最低点数", ref gambaCreditsMinimum, 0, 10000))
            {
                C.GambaCreditsMinimum = gambaCreditsMinimum;
                C.SaveDebounced();
            }
            bool gambaBetween = C.GambaBetweenRuns;
            if (ImGui.Checkbox("在两次运行之间赌博", ref gambaBetween))
            {
                C.GambaBetweenRuns = gambaBetween;
                C.Save();
            }
            GambaSlider();
            ImGui.SetNextItemWidth(150);
            if (ImGui.SliderInt("赌博延迟", ref gambaDelay, 50, 2000))
            {
                C.GambaDelay = gambaDelay;
                C.SaveDebounced();
            }

            if (ImGui.Checkbox("偏好较小的轮盘", ref gambaPreferSmallerWheel))
            {
                C.GambaPreferSmallerWheel = gambaPreferSmallerWheel;
                C.Save();
            }
            ImGuiEx.HelpMarker("这会让赌博偏好物品较少的轮盘。");

            if (PlayerHelper.IsInCosmicZone())
            {
                var territory = Player.Territory.RowId;
                if (CosmicMoonRegistry.TryGetPlanetCreditItemId(territory, out var itemId))
                {
                    PlayerHelper.GetItemCount(itemId, out var credits);
                    ImGui.Text($"当前位置：{territory} | 货币数量：{credits}");
                }
            }

            ImGui.Separator();
            ImGui.TextUnformatted("配置赌博中每个物品的权重。权重越高 = 越想要。");

            if (ImGui.Button("重置权重"))
            {
                Task_Gamba.EnsureGambaWeightsInitialized(true);
            }

            if (ImGui.BeginTabBar("Gamba Item Tabs"))
            {
                foreach (GambaType type in Enum.GetValues(typeof(GambaType)))
                {
                    var itemsType = C.GambaItemWeights.Where(x => x.Type == type).OrderBy(x => x.ItemId).ToList();
                    if (itemsType.Count == 0) continue;

                    if (ImGui.BeginTabItem($"{type.ToString()} [{itemsType.Count}]"))
                    {
                        if (ImGui.BeginTable($"{type.ToString()}_GambaItems", 4, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
                        {
                            ImGui.TableSetupColumn("Icon");
                            ImGui.TableSetupColumn("Unlocked");
                            ImGui.TableSetupColumn("Name");
                            ImGui.TableSetupColumn("Weight");

                            ImGui.TableHeadersRow();

                            foreach (var item in itemsType)
                            {
                                if (Svc.Data.GetExcelSheet<Item>().TryGetRow(item.ItemId, out var itemInfo))
                                {
                                    var iconId = itemInfo.Icon;
                                    var name = itemInfo.Name;
                                    var weight = item.Weight;

                                    ImGui.TableNextRow();
                                    ImGui.TableSetColumnIndex(0);
                                    if (Svc.Texture.TryGetFromGameIcon((int)iconId, out var iconImage) && iconImage != null)
                                    {
                                        var scale = ImGuiHelpers.GlobalScale;
                                        Vector2 imageSize = new Vector2(25 * scale, 25 * scale);

                                        ImGui.Image(iconImage.GetWrapOrEmpty().Handle, imageSize);
                                        if (ImGui.IsItemHovered())
                                        {
                                            ImGui.BeginTooltip();
                                            ImGui.Image(iconImage.GetWrapOrEmpty().Handle, new Vector2(50, 50));
                                            ImGui.EndTooltip();
                                        }
                                    }

                                    ImGui.TableNextColumn();
                                    ImGui.TextUnformatted(UnlockState.IsItemUnlockable(itemInfo) ? UnlockState.IsItemUnlocked(itemInfo) ? "是" : "否" : "-");
                                    
                                    ImGui.TableNextColumn();
                                    ImGui.Text($"{name}");

                                    ImGui.TableNextColumn();
                                    ImGui.SetNextItemWidth(200);
                                    if (ImGui.InputInt($"##weight_{name}_{item.ItemId}", ref weight))
                                    {
                                        item.Weight = weight;
                                        C.SaveDebounced();
                                    }
                                }
                            }

                            ImGui.EndTable();
                        }

                        ImGui.EndTabItem();
                    }
                }

                ImGui.EndTabBar();
            }
        }

        private static int[] allowedValues = { 1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000, 9000, 10000 };
        private static void GambaSlider()
        {
            int currentIndex = Array.IndexOf(allowedValues, C.GambaAtAmount);
            if (currentIndex == -1)
            {
                currentIndex = 0;
                C.GambaAtAmount = allowedValues[0];
                C.SaveDebounced();
            }

            ImGui.SetNextItemWidth(150);
            if (ImGui.SliderInt("开始赌博的点数", ref currentIndex, 0, allowedValues.Length - 1,
                allowedValues[currentIndex].ToString()))
            {
                C.GambaAtAmount = allowedValues[currentIndex];
                C.SaveDebounced();
            }
        }
    }
}
