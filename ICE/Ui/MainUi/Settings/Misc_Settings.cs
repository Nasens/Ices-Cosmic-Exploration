using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ICE.Ui.MainUi.Settings.Settings_Table;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.ImGuiTools;
using System.Collections.Generic;
using static ICE.ConfigFiles.Config;

namespace ICE.Ui.MainUi.Settings
{
    internal class Misc_Settings
    {
        public static void Draw()
        {
            OverlaySettings();
            Separator();
            AutoUse();
            Separator();
            GoldMissionRemover();
            Separator();
            SafetySettings.Draw();
            Separator();
            Separator();
            TimeRecords();
            Separator();
            PostMissionCommands();
            Separator();
            FunSettings();
#if DEBUG
            Separator();
            DebugTab.Draw();
#endif
        }

        public static void OverlaySettings()
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.WindowMaximize, "悬浮窗");
            ImGui.Dummy(new (0, 5));

            bool showOverlay = C.ShowOverlay;
            if (ImGui.Checkbox("显示悬浮窗", ref showOverlay))
            {
                C.ShowOverlay = showOverlay;
                C.Save();
            }
            ImGui.SameLine();
            bool useCogsIcon = C.Overlay_UseCogsIcon;
            if (ImGui.Checkbox("使用齿轮按钮代替主页按钮", ref useCogsIcon))
            {
                C.Overlay_UseCogsIcon = useCogsIcon;
                C.Save();
            }

            bool ShowSeconds = C.ShowSeconds;
            if (ImGui.Checkbox("显示秒数", ref ShowSeconds))
            {
                C.ShowSeconds = ShowSeconds;
                C.Save();
            }

            bool showExpOverlay = C.ShowExpBars;
            if (ImGui.Checkbox("在悬浮窗上显示经验条", ref showExpOverlay))
            {
                C.ShowExpBars = showExpOverlay;
                C.Save();
            }
            if (showExpOverlay)
            {
                ImGui.SameLine();
                bool hideWhenMaxed = C.ShowExpBars_HideWhenMaxed;
                if (ImGui.Checkbox("仅在满级前显示", ref hideWhenMaxed))
                {
                    C.ShowExpBars_HideWhenMaxed = hideWhenMaxed;
                    C.Save();
                }
            }

            bool showClassScore = C.ShowCurrentScore;
            if (ImGui.Checkbox("显示当前职业分数", ref showClassScore))
            {
                C.ShowCurrentScore = showClassScore;
                C.Save();
            }
            ImGui.SameLine();
            bool showTotalScore = C.ShowTotalScore;
            if (ImGui.Checkbox("显示总分数", ref showTotalScore))
            {
                C.ShowTotalScore = showTotalScore;
                C.Save();
            }

            bool AutoResize = C.Overlay_AutoResize;
            if (ImGui.Checkbox("自动调整悬浮窗大小", ref AutoResize))
            {
                C.Overlay_AutoResize = AutoResize;
                C.Save();
            }


            bool highlightTokenWeather = C.Overlay_HighlightTokenWeather;
            if (ImGui.Checkbox("高亮 EX+ 代币天气", ref highlightTokenWeather))
            {
                C.Overlay_HighlightTokenWeather = highlightTokenWeather;
                C.Save();
            }

            bool showSelectedWeatherMissions = C.Overlay_WeatherSelected;
            if (ImGui.Checkbox($"悬停天气时显示已启用的任务", ref showSelectedWeatherMissions))
            {
                C.Overlay_WeatherSelected = showSelectedWeatherMissions;
                C.Save();
            }

            bool filterByCurrentJob = C.Overlay_FilterByCurrentJob;
            if (ImGui.Checkbox("仅按当前职业筛选", ref filterByCurrentJob))
            {
                C.Overlay_FilterByCurrentJob = filterByCurrentJob;
                C.Save();
            }
            if (!filterByCurrentJob)
            {
                float scale = ImGuiHelpers.GlobalScale;
                float iconSize = 26 * scale;
                float iconSpacing = 4;
                var classDict = new Dictionary<uint, string>
                {
                    [8] = "CRP", [9] = "BSM", [10] = "ARM", [11] = "GSM",
                    [12] = "LTW", [13] = "WVR", [14] = "ALC", [15] = "CUL",
                    [16] = "MIN", [17] = "BTN", [18] = "FSH",
                };
                foreach (var (jobId, name) in classDict)
                {
                    bool isSelected = C.Overlay_FilterJobs.Contains(jobId);
                    var icon = isSelected
                        ? CosmicHelper.ClassInfoDict.TryGetValue(jobId, out var tex) ? tex.JobIcon.GetWrapOrEmpty() : null
                        : ImGui_Ice.GetGreyscaleJob(jobId);
                    if (icon != null && ImGui_Ice.DrawStyledImageButton(icon, new Vector2(iconSize, iconSize), isSelected))
                    {
                        if (isSelected)
                            C.Overlay_FilterJobs.Remove(jobId);
                        else
                            C.Overlay_FilterJobs.Add(jobId);
                        C.Save();
                    }
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip(name);
                    ImGui.SameLine(0, iconSpacing);
                }
                ImGui.NewLine();
            }

            bool disableHudClipping = C.DisableHudClipping;
            if (ImGui.Checkbox("禁用 HUD 裁剪", ref disableHudClipping))
            {
                C.DisableHudClipping = disableHudClipping;
                C.Save();
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("启用后，悬浮窗将渲染在原生 UI 元素之上");
            }

        }
        private static void AutoUse()
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.PersonRays, "自动使用");
            ImGui.Dummy(new Vector2(0, 5));

            bool DisableLunarAura = C.RemoveStellarStatus;
            if (ImGui.Checkbox("自动移除 Stellar 状态", ref DisableLunarAura))
            {
                C.RemoveStellarStatus = DisableLunarAura;
                C.Save();
            }
            ImGui.SameLine();
            ImGuiEx.IconWithTooltip(FontAwesomeIcon.InfoCircle,
                                   "自动移除星辰贡献者的视觉特效（成为顶尖贡献者时获得的光效）。\n" +
                                   "重新进入区域时该增益会自行恢复。");

            bool autoStartOnMoonEnter = C.StartUponEnterMoon;
            if (ImGui.Checkbox("进入 Cosmic Exploration 区域时自动开始", ref autoStartOnMoonEnter))
            {
                C.StartUponEnterMoon = autoStartOnMoonEnter;
                C.Save();
            }
            ImGui.SameLine();
            ImGuiEx.IconWithTooltip(FontAwesomeIcon.QuestionCircle,
                                   "这会在你首次进入月球时检查你是否处于采集/制作职业。\n" +
                                   "如果是，它会像你自己按下开始按钮一样自动开始\n" +
                                   "如果你有自动登录的工具，或者你只想进入月球后直接出发，这非常有用\n" +
                                   "此功能仅在首次进入时运行。");
            ImGui.Dummy(Vector2.Zero);
        }
        private static void GoldMissionRemover()
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.Medal, "任务后设置");

            bool removeGold = C.RemoveAfterGold;
            if (ImGui.Checkbox("达成金牌后移除任务", ref removeGold))
            {
                C.RemoveAfterGold = removeGold;
                C.Save();
            }

            using (ImRaii.Disabled(!removeGold))
            {
                bool keepARanks = C.KeepARanks;
                if (ImGui.Checkbox("保留 \"A 级\" 及以下的任务", ref keepARanks))
                {
                    C.KeepARanks = keepARanks;
                    C.Save();
                }
            }
        }
        private static void TimeRecords()
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.Clock, "记录设置");
            ImGui.Dummy(new Vector2(0, 5));

            int TimeHistory = C.TimeHistoryLimit;
            ImGui.SetNextItemWidth(100);
            if (ImGui.InputInt("保留的平均用时历史记录数", ref TimeHistory))
            {
                C.TimeHistoryLimit = TimeHistory;
                C.Save();
            }
            ImGui.SameLine();
            ImGui.TextDisabled("?");
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("小于 0 则保留所有记录\n" +
                                 "大于 0 则保留设定的上限数量");
            }
        }
        private static void PostMissionCommands()
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.Play, "任务后指令");
            ImGui.Dummy(new Vector2(0, 5));

            ImGui.TextWrapped("在下方输入你希望在一次运行完成后执行的指令列表。\n" +
                              "这算是我让你在一定程度上脚本化/设置一系列插件本身可能不包含的其他操作的方式。\n" +
                              "如果你想要更复杂的功能，那就去写一个 SND 脚本吧。然后让它在结束后运行那个脚本，哈哈。");

            if (ImGui.Button("添加新指令"))
            {
                C.PostMissionCommands.Add(new MissionCommand
                {
                    command = "",
                    Delay = 0,
                });
                C.Save();
            }

            MissionCommand? toRemove = null;
            int entryCounter = 0;

            if (ImGui.BeginTable("Mission Commands", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
            {
                ImGui.TableSetupColumn("Command");
                ImGui.TableSetupColumn("Delay");
                ImGui.TableSetupColumn("Remove");

                ImGui.TableHeadersRow();

                foreach (var entry in C.PostMissionCommands)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.SetNextItemWidth(200);

                    ImGui.PushID($"{entryCounter}_MissionCommand");
                    string command = entry.command;
                    if (ImGui.InputText("##Command", ref command))
                    {
                        entry.command = command;
                        C.SaveDebounced();
                    }

                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(100);
                    int delay = entry.Delay;
                    if (ImGui.InputInt("###Delay", ref delay))
                    {
                        entry.Delay = delay;
                        C.SaveDebounced();
                    }

                    ImGui.TableNextColumn();
                    if (ImGuiEx.IconButton(FontAwesomeIcon.Trash, $"remove{C.PostMissionCommands.IndexOf(entry)}"))
                    {
                        toRemove = entry;
                    }
                    ImGui.PopID();
                    entryCounter += 1;
                }

                if (toRemove != null)
                {
                    C.PostMissionCommands.Remove(toRemove);
                    C.Save();
                }

                ImGui.EndTable();
            }
        }
        private static void FunSettings()
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.Heart, "开发者最爱");
            var crazyEnabled = C.CrazyTaxiArrow;
            if (ImGui.Checkbox("寻路时显示疯狂出租车箭头", ref crazyEnabled))
            {
                C.CrazyTaxiArrow = crazyEnabled;
                C.Save();
            }

            var placiboEffect = C.PlaceboCheckbox;
            if (ImGui.Checkbox("提升采集与制作速度", ref placiboEffect))
            {
                C.PlaceboCheckbox = placiboEffect;
                C.Save();
            }
            ImGui.SameLine();
            ImGuiEx.IconWithTooltip(FontAwesomeIcon.QuestionCircle, "这个完全没有任何作用\n" +
                "但我知道肯定会有人启用它却不看说明，所以这是个小恶作剧。\n" +
                "不过还是谢谢你使用我的插件，这对我意义重大 <3");
        }
        private static void Separator()
        {
            ImGui.Dummy(new Vector2(0, 5));
            ImGui.Separator();
            ImGui.Dummy(new Vector2(0, 5));
        }
    }
}
