using Dalamud.Interface;
using ECommons.GameHelpers;
using ICE.Ui.DebugWindowTabs;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.GatheringHelper;
using ICE.Utilities.ImGuiTools;
using static ICE.ConfigFiles.Config;

namespace ICE.Ui.MainUi.Settings.Settings_Table
{
    internal class TravelSettings
    {
        private static FishingDebug _fishingDebug = null;

        public static unsafe void Draw()
        {
            if (_fishingDebug == null)
            {
                _fishingDebug = new FishingDebug();
            }

            PathfindingSettings();

            Separator();
            StuckSettings();

            Separator();
            CraftingLocations();

            Separator();
            FishingLocations();
        }

        private static void Separator()
        {
            ImGui.Dummy(new Vector2(0, 5));
            ImGui.Separator();
            ImGui.Dummy(new Vector2(0, 5));
        }

        private static void PathfindingSettings()
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.Route, "寻路");
            ImGui.Dummy(new Vector2(0, 5));

            bool stellarSprint = C.MoonSprint;
            if (ImGui.Checkbox("自动使用 Stellar Sprint", ref stellarSprint))
            {
                C.MoonSprint = stellarSprint;
                C.Save();
            }

            bool closestNode = C.ClosestNodeSelection;
            if (ImGui.Checkbox("优先选择最近的采集点", ref closestNode))
            {
                C.ClosestNodeSelection = closestNode;
                C.Save();
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("始终导航到最近的可选目标采集点，而不是按固定的路线顺序行进。\n对于以速度为重的限时 EX+ 任务很有用。");
            }

            bool randomize = C.RandomizeWaypoints;
            if (ImGui.Checkbox("随机化路径点位置", ref randomize))
            {
                C.RandomizeWaypoints = randomize;
                C.Save();
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("为导航目的地添加一个小的随机偏移，让角色不会总是沿着完全相同的路径行进");
            }
            if (randomize)
            {
                ImGui.SameLine();
                float radius = C.RandomizeWaypointsRadius;
                ImGui.SetNextItemWidth(100);
                if (ImGui.SliderFloat("随机化半径（yalms）", ref radius, 0.5f, 1.0f, "%.1f"))
                {
                    C.RandomizeWaypointsRadius = radius;
                    C.SaveDebounced();
                }
                bool showDebug = C.RandomizeWaypointsDebug;
                if (ImGui.Checkbox("显示随机位置调试目标", ref showDebug))
                {
                    C.RandomizeWaypointsDebug = showDebug;
                    C.Save();
                }
            }

            int GatherFanRandom = C.GatherFanSectionSize;
            ImGui.SetNextItemWidth(200);
            if (ImGui.SliderInt("采集扇形选择", ref GatherFanRandom, 0, 360))
            {
                C.GatherFanSectionSize = GatherFanRandom;
                C.SaveDebounced();
            }
            ImGui.SameLine();
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle,
                "这会调整从扇形中心点起可供随机选择的范围大小。\n" +
                "360 = 整个扇形都可供选择\n" +
                "其他数值则会在该扇形范围内选择（如果可用的话）", false);

            bool useHubReturn = C.UseHubReturn;
            if (ImGui.Checkbox("使用中心返回", ref useHubReturn))
            {
                C.UseHubReturn = useHubReturn;
                C.Save();
            }
            ImGui.SameLine();
            bool useAethernet = C.UseAethernet;
            if (ImGui.Checkbox("使用以太之网", ref useAethernet))
            {
                C.UseAethernet = useAethernet;
                C.Save();
            }

            bool useRedAlertNpc = C.UseRedAlertNpc;
            if (ImGui.Checkbox("使用红色警报 NPC 进行移动", ref useRedAlertNpc))
            {
                C.UseRedAlertNpc = useRedAlertNpc;
                C.Save();
            }
            ImGui.SameLine();
            ImGui.TextDisabled("测试版，可能无法使用");

            bool avoidStellarReturn = C.AvoidStellarReturn;
            if (ImGui.Checkbox("寻路时避免使用 Stellar Return", ref avoidStellarReturn))
            {
                C.AvoidStellarReturn = avoidStellarReturn;
                C.Save();
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("启用后，寻路器将不会使用 Stellar Return 前往采集点。\n这适用于中心返回以及中心 + 以太之网两种移动方式。");
            }
            if (C.AvoidStellarReturn)
            {
                ImGui.SameLine();
                bool exceptHub = C.AvoidStellarReturnExceptHub;
                if (ImGui.Checkbox("中心活动除外", ref exceptHub))
                {
                    C.AvoidStellarReturnExceptHub = exceptHub;
                    C.Save();
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("启用后，对于点数购买、赌博、无人机箱、修理等活动，\n仍会使用 Stellar Return 返回中心。");
                }
            }

            var minHubReturnDistance = C.HubReturn_Distance;
            ImGui.SetNextItemWidth(200);
            if (ImGui.DragFloat("使用中心返回前的距离（yalms）", ref minHubReturnDistance))
            {
                C.HubReturn_Distance = minHubReturnDistance;
                C.SaveDebounced();
            }

            bool DisableRedAlertPathing = C.DisablePathfindingToRedAlert;
            if (ImGui.Checkbox("禁用前往红色警报的寻路", ref DisableRedAlertPathing))
            {
                C.DisablePathfindingToRedAlert = DisableRedAlertPathing;
                C.Save();
            }

            bool DisableHubActivies_RE = C.DisableHub_Critical;
            if (ImGui.Checkbox("当红色警报激活时不执行中心活动", ref DisableHubActivies_RE))
            {
                C.DisableHub_Critical = DisableHubActivies_RE;
                C.Save();
            }

            bool delayAether = C.Delay_Aethernet;
            if (ImGui.Checkbox("为以太之网 / NPC 移动添加延迟", ref delayAether))
            {
                C.Delay_Aethernet = delayAether;
                C.Save();
            }
            ImGuiEx.HelpMarker("在与以太之芯 / 红色警报 NPC 移动交互前添加一段随机延迟。\n" +
                "延迟会在交互之前，以及在操作菜单之间稍作停顿");
        }
        private static void StuckSettings()
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.ExclamationTriangle, "卡住检测");
            ImGui.Dummy(new Vector2(0, 5));

            bool unstuckEnabled = C.JumpIfStuck_V2 || C.RetargetIfStuck;
            if (ImGui.Checkbox("在导航移动中卡住时：", ref unstuckEnabled))
            {
                if (unstuckEnabled)
                    C.JumpIfStuck_V2 = true;
                else
                {
                    C.JumpIfStuck_V2 = false;
                    C.RetargetIfStuck = false;
                }
                C.Save();
            }
            ImGui.SameLine();
            ImGuiEx.HelpMarker(
                "当在 navmesh 移动中卡住达到设定的时长后：\n" +
                "- 跳跃：尝试跳过障碍物\n" +
                "- 重新寻路：停下并重新寻路至目的地（如果启用则重新随机化）");
            if (!unstuckEnabled) ImGui.BeginDisabled();
            if (ImGui.RadioButton("跳跃", C.JumpIfStuck_V2 && !C.RetargetIfStuck))
            {
                C.JumpIfStuck_V2 = true;
                C.RetargetIfStuck = false;
                C.Save();
            }
            ImGui.SameLine();
            if (ImGui.RadioButton("重新寻路", C.RetargetIfStuck))
            {
                C.RetargetIfStuck = true;
                C.JumpIfStuck_V2 = false;
                C.Save();
            }
            ImGui.SameLine();
            ImGui.Text("卡住");
            ImGui.SameLine();
            int stuckDelay = C.StuckDelayMs;
            ImGui.SetNextItemWidth(100);
            if (ImGui.SliderInt("毫秒后###StuckDelay", ref stuckDelay, 500, 3000))
            {
                if (C.StuckDelayMs != stuckDelay)
                {
                    C.StuckDelayMs = stuckDelay;
                    C.SaveDebounced();
                }
            }
            if (!unstuckEnabled) ImGui.EndDisabled();
        }
        private static void CraftingLocations()
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.MapPin, "制作返回点");
            ImGui.Dummy(new Vector2(0, 5));

            bool usePersonalLocations = C.PersonalReturnSpot;
            if (ImGui.Checkbox("使用个人返回点", ref usePersonalLocations))
            {
                C.PersonalReturnSpot = usePersonalLocations;
                C.Save();
            }
            if (usePersonalLocations)
            {
                var territory = Player.Territory.RowId;
                var location = Player.Position;
                ImGui.SameLine();
                if (C.CrafterLocations.TryGetValue(territory, out var moonLoc))
                {
                    if (ImGui.Button("设为当前位置"))
                    {
                        C.CrafterLocations[territory] = location;
                        C.Save();
                    }
                    ImGui.SameLine();
                    ImGui.Text($"({moonLoc.X:N1}, {moonLoc.Y:N1}, {moonLoc.Z:N1})");
                }
                else
                {
                    if (ImGui.Button("添加位置"))
                    {
                        C.CrafterLocations[territory] = Player.Position;
                        C.Save();
                    }
                    ImGui.SameLine();
                    ImGui.Text("未设置位置");
                }
            }
        }
        private static void FishingLocations()
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.Fish, "个性化钓鱼点");
            ImGui.SameLine();
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle, "如果你选择不使用插件内置的随机钓鱼点，这里可以让你保存自己的位置\n" +
                "你不一定要用此功能，在以下情况下它会直接使用随机点：\n" +
                "1: 已保存了一个位置：\n" +
                "2: 甚至已保存了一个随机点", false);
            ImGui.Dummy(new Vector2(0, 5));

            var currentTerritory = Player.Territory.RowId;

            if (GatheringUtil.MoonFishingLocations.TryGetValue(currentTerritory, out var fishingHoles))
            {
                ImGui.Text($"星球：{Player.Territory.Value.PlaceName.Value.Name}");
                ImGui.Checkbox("显示钓鱼点射线", ref _fishingDebug.ShowFishRay);
                if (Player.Object is { } player && _fishingDebug.ShowFishRay)
                {
                    _fishingDebug.Draw();
                }

                ImGui.Separator();

                foreach (var hole in fishingHoles.Keys)
                {
                    // Find existing entry for this zone + map coord, or creating a new one if one doesn't exist
                    var entry = C.Personal_FishLocation.FirstOrDefault(f => f.ZoneId == currentTerritory && f.MapCoords == hole);

                    if (entry == null)
                    {
                        entry = new FishingLocations
                        {
                            ZoneId = currentTerritory,
                            X = hole.X,
                            Y = hole.Y,
                            WorldPosition = null
                        };
                        C.Personal_FishLocation.Add(entry);
                        C.SaveDebounced();
                    }

                    ImGui.PushID($"{hole}_Flag");

                    if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Flag, $"  X: {hole.X:N2} Y: {hole.Y:N2}"))
                    {
                        var mission = CosmicHelper.SheetMissionDict.Where(x => x.Value.MapPosition == hole).FirstOrDefault();
                        Utils.SetGatheringRing(mission.Value.TerritoryId, (int)hole.X, (int)hole.Y, mission.Value.Radius, $"{hole.X:N2} {hole.Y:N2}");
                    }
                    ImGui.SameLine();

                    string currentPos = entry.WorldPosition == null ? "添加新点" : $"移除";

                    if (ImGui.Button($"{currentPos}"))
                    {
                        entry.WorldPosition = entry.WorldPosition == null ? Player.Position : null;
                        C.Save();
                    }

                    if (entry.WorldPosition != null)
                    {
                        ImGui.SameLine();
                        ImGui.Text($"{entry.WorldPosition.Value:N2}");
                    }

                    ImGui.PopID();
                }
            }
            else
            {
                ImGui.Text($"当前星球在数据表中没有存储的钓鱼点。（也许需要添加？）");
            }
        }
    }
}
