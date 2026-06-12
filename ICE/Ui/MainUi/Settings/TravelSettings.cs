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
            // ImGuiEx.IconWithText(FontAwesomeIcon.Route, "Pathfinding");
            ImGuiEx.IconWithText(FontAwesomeIcon.Route, "寻路");
            ImGui.Dummy(new Vector2(0, 5));

            bool stellarSprint = C.MoonSprint;
            // if (ImGui.Checkbox("Auto-Use Stellar Sprint", ref stellarSprint))
            if (ImGui.Checkbox("自动使用星界冲刺", ref stellarSprint))
            {
                C.MoonSprint = stellarSprint;
                C.Save();
            }

            bool closestNode = C.ClosestNodeSelection;
            // if (ImGui.Checkbox("Prioritize closest gathering node", ref closestNode))
            if (ImGui.Checkbox("优先最近采集点", ref closestNode))
            {
                C.ClosestNodeSelection = closestNode;
                C.Save();
            }
            if (ImGui.IsItemHovered())
            {
                // ImGui.SetTooltip("Always navigate to the closest targetable node instead of following the fixed route order.\nUseful for timed EX+ missions where speed matters.");
                ImGui.SetTooltip("始终导航至最近的可选节点，而非按固定路线顺序移动。\n适用于限时 EX+ 任务等需要快速移动的场景。");
            }

            bool randomize = C.RandomizeWaypoints;
            // if (ImGui.Checkbox("Randomize waypoint positions", ref randomize))
            if (ImGui.Checkbox("随机化路径点位置", ref randomize))
            {
                C.RandomizeWaypoints = randomize;
                C.Save();
            }
            if (ImGui.IsItemHovered())
            {
                // ImGui.SetTooltip("Adds a small random offset to navigation destinations so the character doesn't always follow the exact same path");
                ImGui.SetTooltip("为导航目标添加小幅随机偏移，避免角色每次都走完全相同的路径");
            }
            if (randomize)
            {
                ImGui.SameLine();
                float radius = C.RandomizeWaypointsRadius;
                ImGui.SetNextItemWidth(100);
                // if (ImGui.SliderFloat("Randomize radius (yalms)", ref radius, 0.5f, 1.0f, "%.1f"))
                if (ImGui.SliderFloat("随机化半径（码）", ref radius, 0.5f, 1.0f, "%.1f"))
                {
                    C.RandomizeWaypointsRadius = radius;
                    C.SaveDebounced();
                }
                bool showDebug = C.RandomizeWaypointsDebug;
                // if (ImGui.Checkbox("Show random location debug target", ref showDebug))
                if (ImGui.Checkbox("显示随机位置调试目标", ref showDebug))
                {
                    C.RandomizeWaypointsDebug = showDebug;
                    C.Save();
                }
            }

            int GatherFanRandom = C.GatherFanSectionSize;
            ImGui.SetNextItemWidth(200);
            // if (ImGui.SliderInt("Gathering Fan Selection", ref GatherFanRandom, 0, 360))
            if (ImGui.SliderInt("采集扇形选择", ref GatherFanRandom, 0, 360))
            {
                C.GatherFanSectionSize = GatherFanRandom;
                C.SaveDebounced();
            }
            ImGui.SameLine();
            // ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle,
            //     "This will adjust how much of the center point of the fan it will randomize from.\n" +
            //     "360 = the whole fan will be available for selection\n" +
            //     "Anything besides that will chose within that fan (if it's available)", false);
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle,
                "调整从扇形中心点随机选择的范围。\n" +
                "360 = 整个扇形范围均可选择\n" +
                "其他数值则在该角度范围内选择（若可用）", false);

            bool useHubReturn = C.UseHubReturn;
            // if (ImGui.Checkbox("Use Hub Return", ref useHubReturn))
            if (ImGui.Checkbox("使用中枢返回", ref useHubReturn))
            {
                C.UseHubReturn = useHubReturn;
                C.Save();
            }
            ImGui.SameLine();
            bool useAethernet = C.UseAethernet;
            // if (ImGui.Checkbox("Use Aethernet", ref useAethernet))
            if (ImGui.Checkbox("使用以太之光", ref useAethernet))
            {
                C.UseAethernet = useAethernet;
                C.Save();
            }

            bool useRedAlertNpc = C.UseRedAlertNpc;
            // if (ImGui.Checkbox("Use Red Alert NPC for travel", ref useRedAlertNpc))
            if (ImGui.Checkbox("使用红警 NPC 传送", ref useRedAlertNpc))
            {
                C.UseRedAlertNpc = useRedAlertNpc;
                C.Save();
            }
            ImGui.SameLine();
            // ImGui.TextDisabled("Beta, might not work");
            ImGui.TextDisabled("Beta，可能无法使用");

            bool avoidStellarReturn = C.AvoidStellarReturn;
            // if (ImGui.Checkbox("Avoid Stellar Return for pathing", ref avoidStellarReturn))
            if (ImGui.Checkbox("寻路时避免使用星界返回", ref avoidStellarReturn))
            {
                C.AvoidStellarReturn = avoidStellarReturn;
                C.Save();
            }
            if (ImGui.IsItemHovered())
            {
                // ImGui.SetTooltip("When enabled, the pathfinder will not use Stellar Return to travel to gathering nodes.\nThis applies to both Hub Return and Hub + Aethernet travel methods.");
                ImGui.SetTooltip("启用后，寻路器不会使用星界返回前往采集节点。\n适用于中枢返回以及中枢 + 以太之光两种移动方式。");
            }
            if (C.AvoidStellarReturn)
            {
                ImGui.SameLine();
                bool exceptHub = C.AvoidStellarReturnExceptHub;
                // if (ImGui.Checkbox("Except for hub activities", ref exceptHub))
                if (ImGui.Checkbox("中枢活动除外", ref exceptHub))
                {
                    C.AvoidStellarReturnExceptHub = exceptHub;
                    C.Save();
                }
                if (ImGui.IsItemHovered())
                {
                    // ImGui.SetTooltip("When enabled, Stellar Return will still be used to return to the hub\nfor activities like credit purchases, gambling, drone bits, and repairs.");
                    ImGui.SetTooltip("启用后，星界返回仍会用于返回中枢，\n以进行点数购物、赌博、无人机代币和修理等活动。");
                }
            }

            var minHubReturnDistance = C.HubReturn_Distance;
            ImGui.SetNextItemWidth(200);
            // if (ImGui.DragFloat("Distance before hub return is used (yalms)", ref minHubReturnDistance))
            if (ImGui.DragFloat("使用中枢返回前的距离（码）", ref minHubReturnDistance))
            {
                C.HubReturn_Distance = minHubReturnDistance;
                C.SaveDebounced();
            }

            bool DisableRedAlertPathing = C.DisablePathfindingToRedAlert;
            // if (ImGui.Checkbox("Disable Pathfinding to Red Alerts", ref DisableRedAlertPathing))
            if (ImGui.Checkbox("禁用寻路到紧急通告", ref DisableRedAlertPathing))
            {
                C.DisablePathfindingToRedAlert = DisableRedAlertPathing;
                C.Save();
            }

            bool DisableHubActivies_RE = C.DisableHub_Critical;
            // if (ImGui.Checkbox("Don't do hub activities when a red alert is active", ref DisableHubActivies_RE))
            if (ImGui.Checkbox("紧急通告期间不执行中枢活动", ref DisableHubActivies_RE))
            {
                C.DisableHub_Critical = DisableHubActivies_RE;
                C.Save();
            }

            bool delayAether = C.Delay_Aethernet;
            // if (ImGui.Checkbox("Add delay to athernet / npc travel", ref delayAether))
            if (ImGui.Checkbox("以太之光/NPC 传送增加延迟", ref delayAether))
            {
                C.Delay_Aethernet = delayAether;
                C.Save();
            }
            // ImGuiEx.HelpMarker("Adds a random delay before interacting with the aethershard / red alert npc travel.\n" +
            //     "The delays will be before, and a little bit inbetween interacting with menus");
            ImGuiEx.HelpMarker("在与以太晶簇/红警 NPC 传送交互前添加随机延迟。\n" +
                "延迟会出现在交互前，以及菜单交互之间的短暂停顿");
        }
        private static void StuckSettings()
        {
            // ImGuiEx.IconWithText(FontAwesomeIcon.ExclamationTriangle, "Stuck Detection");
            ImGuiEx.IconWithText(FontAwesomeIcon.ExclamationTriangle, "卡住检测");
            ImGui.Dummy(new Vector2(0, 5));

            bool unstuckEnabled = C.JumpIfStuck_V2 || C.RetargetIfStuck;
            // if (ImGui.Checkbox("If stuck during nav movement:", ref unstuckEnabled))
            if (ImGui.Checkbox("导航移动时若卡住：", ref unstuckEnabled))
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
            // ImGuiEx.HelpMarker(
            //     "When stuck during navmesh movement for the configured delay:\n" +
            //     "- Jump: attempts to jump over the obstacle\n" +
            //     "- Retarget: stops and re-pathfinds to the destination (re-randomizes if enabled)");
            ImGuiEx.HelpMarker(
                "在 navmesh 移动卡住达到设定延迟后：\n" +
                "- 跳跃：尝试跳过障碍物\n" +
                "- 重新寻路：停止并重新寻路至目的地（若已启用则会重新随机化）");
            if (!unstuckEnabled) ImGui.BeginDisabled();
            // if (ImGui.RadioButton("Jump", C.JumpIfStuck_V2 && !C.RetargetIfStuck))
            if (ImGui.RadioButton("跳跃", C.JumpIfStuck_V2 && !C.RetargetIfStuck))
            {
                C.JumpIfStuck_V2 = true;
                C.RetargetIfStuck = false;
                C.Save();
            }
            ImGui.SameLine();
            // if (ImGui.RadioButton("Retarget", C.RetargetIfStuck))
            if (ImGui.RadioButton("重新寻路", C.RetargetIfStuck))
            {
                C.RetargetIfStuck = true;
                C.JumpIfStuck_V2 = false;
                C.Save();
            }
            ImGui.SameLine();
            // ImGui.Text("after");
            ImGui.Text("后");
            ImGui.SameLine();
            int stuckDelay = C.StuckDelayMs;
            ImGui.SetNextItemWidth(100);
            // if (ImGui.SliderInt("ms stuck###StuckDelay", ref stuckDelay, 500, 3000))
            if (ImGui.SliderInt("毫秒卡住###StuckDelay", ref stuckDelay, 500, 3000))
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
            // ImGuiEx.IconWithText(FontAwesomeIcon.MapPin, "Crafting Return Spot");
            ImGuiEx.IconWithText(FontAwesomeIcon.MapPin, "制作返回点");
            ImGui.Dummy(new Vector2(0, 5));

            bool usePersonalLocations = C.PersonalReturnSpot;
            // if (ImGui.Checkbox("Use personal return spots", ref usePersonalLocations))
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
                    // if (ImGui.Button("Set to current location"))
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
                    // if (ImGui.Button("Add Location"))
                    if (ImGui.Button("添加位置"))
                    {
                        C.CrafterLocations[territory] = Player.Position;
                        C.Save();
                    }
                    ImGui.SameLine();
                    // ImGui.Text("No location set");
                    ImGui.Text("未设置位置");
                }
            }
        }
        private static void FishingLocations()
        {
            // ImGuiEx.IconWithText(FontAwesomeIcon.Fish, "Personalized Fishing Spots");
            ImGuiEx.IconWithText(FontAwesomeIcon.Fish, "个性化钓点");
            ImGui.SameLine();
            // ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle, "A way for you to save your own positions if you choose to not use a randomized spot that's included in the plugin\n" +
            //     "You don't have to use this, it will just use a random spot if:\n" +
            //     "1: A position is saved:\n" +
            //     "2: A random spot even is saved", false);
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle, "若您不想使用插件内置的随机钓点，可用此功能保存自己的位置。\n" +
                "非必须启用；满足以下条件时会使用随机钓点：\n" +
                "1：已保存位置\n" +
                "2：已保存随机钓点事件", false);
            ImGui.Dummy(new Vector2(0, 5));

            var currentTerritory = Player.Territory.RowId;

            if (GatheringUtil.MoonFishingLocations.TryGetValue(currentTerritory, out var fishingHoles))
            {
                // ImGui.Text($"Planet: {Player.Territory.Value.PlaceName.Value.Name}");
                ImGui.Text($"星球：{Player.Territory.Value.PlaceName.Value.Name}");
                // ImGui.Checkbox("Show fishing spot raycast", ref _fishingDebug.ShowFishRay);
                ImGui.Checkbox("显示钓点射线检测", ref _fishingDebug.ShowFishRay);
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

                    // string currentPos = entry.WorldPosition == null ? "Add New" : $"Remove";
                    string currentPos = entry.WorldPosition == null ? "新增" : $"移除";

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
                // ImGui.Text($"Current planet has no stored fishing holes in the sheets. (Might need to be added?)");
                ImGui.Text($"当前星球在数据表中无已存储钓点。（可能需要添加？）");
            }
        }
    }
}
