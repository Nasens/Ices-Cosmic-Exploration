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
            // ImGuiEx.IconWithText(FontAwesomeIcon.WindowMaximize, "Overlay Window");
            ImGuiEx.IconWithText(FontAwesomeIcon.WindowMaximize, "悬浮窗");
            ImGui.Dummy(new (0, 5));

            bool showOverlay = C.ShowOverlay;
            // if (ImGui.Checkbox("Show Overlay", ref showOverlay))
            if (ImGui.Checkbox("显示悬浮窗", ref showOverlay))
            {
                C.ShowOverlay = showOverlay;
                C.Save();
            }
            ImGui.SameLine();
            bool useCogsIcon = C.Overlay_UseCogsIcon;
            // if (ImGui.Checkbox("Use cogs button instead of home", ref useCogsIcon))
            if (ImGui.Checkbox("用齿轮按钮代替主页", ref useCogsIcon))
            {
                C.Overlay_UseCogsIcon = useCogsIcon;
                C.Save();
            }

            bool ShowSeconds = C.ShowSeconds;
            // if (ImGui.Checkbox("Show Seconds", ref ShowSeconds))
            if (ImGui.Checkbox("显示秒数", ref ShowSeconds))
            {
                C.ShowSeconds = ShowSeconds;
                C.Save();
            }

            bool showExpOverlay = C.ShowExpBars;
            // if (ImGui.Checkbox("Show Experience Bars on Overlay", ref showExpOverlay))
            if (ImGui.Checkbox("悬浮窗显示经验条", ref showExpOverlay))
            {
                C.ShowExpBars = showExpOverlay;
                C.Save();
            }
            if (showExpOverlay)
            {
                ImGui.SameLine();
                bool hideWhenMaxed = C.ShowExpBars_HideWhenMaxed;
                // if (ImGui.Checkbox("Until maxed only", ref hideWhenMaxed))
                if (ImGui.Checkbox("仅未满级时显示", ref hideWhenMaxed))
                {
                    C.ShowExpBars_HideWhenMaxed = hideWhenMaxed;
                    C.Save();
                }
            }

            bool showClassScore = C.ShowCurrentScore;
            // if (ImGui.Checkbox("Show Current Class Score", ref showClassScore))
            if (ImGui.Checkbox("显示当前职业分数", ref showClassScore))
            {
                C.ShowCurrentScore = showClassScore;
                C.Save();
            }
            ImGui.SameLine();
            bool showTotalScore = C.ShowTotalScore;
            // if (ImGui.Checkbox("Show Total Score", ref showTotalScore))
            if (ImGui.Checkbox("显示总分数", ref showTotalScore))
            {
                C.ShowTotalScore = showTotalScore;
                C.Save();
            }

            bool AutoResize = C.Overlay_AutoResize;
            // if (ImGui.Checkbox("Auto Resize Overlay", ref AutoResize))
            if (ImGui.Checkbox("自动调整悬浮窗大小", ref AutoResize))
            {
                C.Overlay_AutoResize = AutoResize;
                C.Save();
            }


            bool highlightTokenWeather = C.Overlay_HighlightTokenWeather;
            // if (ImGui.Checkbox("Highlight EX+ token weathers", ref highlightTokenWeather))
            if (ImGui.Checkbox("高亮 EX+ 代币天气", ref highlightTokenWeather))
            {
                C.Overlay_HighlightTokenWeather = highlightTokenWeather;
                C.Save();
            }

            bool showSelectedWeatherMissions = C.Overlay_WeatherSelected;
            // if (ImGui.Checkbox($"Show enabled missions on weather hover", ref showSelectedWeatherMissions))
            if (ImGui.Checkbox($"悬停天气时显示已启用任务", ref showSelectedWeatherMissions))
            {
                C.Overlay_WeatherSelected = showSelectedWeatherMissions;
                C.Save();
            }

            bool filterByCurrentJob = C.Overlay_FilterByCurrentJob;
            // if (ImGui.Checkbox("Filter by current job only", ref filterByCurrentJob))
            if (ImGui.Checkbox("仅筛选当前职业", ref filterByCurrentJob))
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
            // if (ImGui.Checkbox("Disable HUD Clipping", ref disableHudClipping))
            if (ImGui.Checkbox("禁用 HUD 裁剪", ref disableHudClipping))
            {
                C.DisableHudClipping = disableHudClipping;
                C.Save();
            }
            if (ImGui.IsItemHovered())
            {
                // ImGui.SetTooltip("When enabled, overlays will render over the native UI elements");
                ImGui.SetTooltip("启用后，悬浮窗将渲染在游戏原生 UI 元素之上");
            }

        }
        private static void AutoUse()
        {
            // ImGuiEx.IconWithText(FontAwesomeIcon.PersonRays, "Auto-Use");
            ImGuiEx.IconWithText(FontAwesomeIcon.PersonRays, "自动使用");
            ImGui.Dummy(new Vector2(0, 5));

            bool DisableLunarAura = C.RemoveStellarStatus;
            // if (ImGui.Checkbox("Auto-Remove Stellar Status", ref DisableLunarAura))
            if (ImGui.Checkbox("自动移除星界状态", ref DisableLunarAura))
            {
                C.RemoveStellarStatus = DisableLunarAura;
                C.Save();
            }
            ImGui.SameLine();
            ImGuiEx.IconWithTooltip(FontAwesomeIcon.InfoCircle,
                                   // "Automatically removes the Star Contributor visual effect (the glow you get for being a top contributor).\n" +
                                   // "The buff restores itself when you re-enter the zone."
                                   "自动移除星界贡献者视觉效果（顶尖贡献者的光效）。\n" +
                                   "重新进入区域后该增益会恢复。");

            bool autoStartOnMoonEnter = C.StartUponEnterMoon;
            // if (ImGui.Checkbox("Auto start upon entering a Cosmic Exploration area", ref autoStartOnMoonEnter))
            if (ImGui.Checkbox("进入宇宙探索区域时自动启动", ref autoStartOnMoonEnter))
            {
                C.StartUponEnterMoon = autoStartOnMoonEnter;
                C.Save();
            }
            ImGui.SameLine();
            ImGuiEx.IconWithTooltip(FontAwesomeIcon.QuestionCircle,
                                   // "This will check to see if you're on a gathering/crafting class upon first entering the moon.\n" +
                                   // "If you are, it will automatically start as if you had pressed the start button yourself\n" +
                                   // "Really useful if you have a tool to auto-log you in/if you just want to enter the moon and go\n" +
                                   // "This will ONLY run upon first entry."
                                   "首次进入星球时，将检查当前是否为采集/制作职业。\n" +
                                   "若是，将自动启动，如同手动按下开始按钮。\n" +
                                   "适用于自动登录工具，或进入星球即开跑的场景。\n" +
                                   "仅在首次进入时运行一次。");
            ImGui.Dummy(Vector2.Zero);
        }
        private static void GoldMissionRemover()
        {
            // ImGuiEx.IconWithText(FontAwesomeIcon.Medal, "Post Mission Settings");
            ImGuiEx.IconWithText(FontAwesomeIcon.Medal, "任务后设置");

            bool removeGold = C.RemoveAfterGold;
            // if (ImGui.Checkbox("Remove Mission Upon Gold Completion", ref removeGold))
            if (ImGui.Checkbox("金牌完成后移除任务", ref removeGold))
            {
                C.RemoveAfterGold = removeGold;
                C.Save();
            }

            using (ImRaii.Disabled(!removeGold))
            {
                bool keepARanks = C.KeepARanks;
                // if (ImGui.Checkbox("Keep \"A Rank\" missions and below", ref keepARanks))
                if (ImGui.Checkbox("保留 A 级及以下任务", ref keepARanks))
                {
                    C.KeepARanks = keepARanks;
                    C.Save();
                }
            }
        }
        private static void TimeRecords()
        {
            // ImGuiEx.IconWithText(FontAwesomeIcon.Clock, "Record Settings");
            ImGuiEx.IconWithText(FontAwesomeIcon.Clock, "记录设置");
            ImGui.Dummy(new Vector2(0, 5));

            int TimeHistory = C.TimeHistoryLimit;
            ImGui.SetNextItemWidth(100);
            // if (ImGui.InputInt("Average Time History to keep", ref TimeHistory))
            if (ImGui.InputInt("保留的平均用时记录数", ref TimeHistory))
            {
                C.TimeHistoryLimit = TimeHistory;
                C.Save();
            }
            ImGui.SameLine();
            ImGui.TextDisabled("?");
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(
                    // "Anything below 0 to keep all logs\n" +
                    // "Above 0 to keep a set limit"
                    "小于 0 则保留全部记录\n" +
                    "大于 0 则保留指定数量上限"
                );
            }
        }
        private static void PostMissionCommands()
        {
            // ImGuiEx.IconWithText(FontAwesomeIcon.Play, "Post Mission Commands");
            ImGuiEx.IconWithText(FontAwesomeIcon.Play, "任务后命令");
            ImGui.Dummy(new Vector2(0, 5));

            ImGui.TextWrapped(
                // "Input below a list of commands that you would like to run after a run has been completed. \n" +
                // "This is kind of my way of letting you somewhat script/set up a sequence of other things that you would like to do that might not be included in the plugin itself. \n" +
                // "If you want something more complex, just make an SND script at that point. And have this run that script post lol."
                "在下方输入运行结束后要执行的命令列表。\n" +
                "这是一种简易脚本方式，可串联插件未内置的其他操作。\n" +
                "若需更复杂逻辑，请编写 SND 脚本，再在此运行该脚本。"
            );

            // if (ImGui.Button("Add New Command"))
            if (ImGui.Button("添加新命令"))
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
                // ImGui.TableSetupColumn("Command");
                // ImGui.TableSetupColumn("Delay");
                // ImGui.TableSetupColumn("Remove");
                ImGui.TableSetupColumn("命令");
                ImGui.TableSetupColumn("延迟");
                ImGui.TableSetupColumn("移除");

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
            // ImGuiEx.IconWithText(FontAwesomeIcon.Heart, "Dev Favorites");
            ImGuiEx.IconWithText(FontAwesomeIcon.Heart, "开发者收藏");
            var crazyEnabled = C.CrazyTaxiArrow;
            // if (ImGui.Checkbox("Show Crazy Taxi Arrow when navmeshing", ref crazyEnabled))
            if (ImGui.Checkbox("寻路时显示 Crazy Taxi 箭头", ref crazyEnabled))
            {
                C.CrazyTaxiArrow = crazyEnabled;
                C.Save();
            }

            var placiboEffect = C.PlaceboCheckbox;
            // if (ImGui.Checkbox("Increase Gathering & Crafting Speed", ref placiboEffect))
            if (ImGui.Checkbox("提升采集与制作速度", ref placiboEffect))
            {
                C.PlaceboCheckbox = placiboEffect;
                C.Save();
            }
            ImGui.SameLine();
            ImGuiEx.IconWithTooltip(FontAwesomeIcon.QuestionCircle,
                // "This does abosolutely nothing\n" +
                // "But I know there's going to be people who enable this and don't read, so it's a tehe.\n" +
                // "Thanks for using my plugin though, it means a lot <3"
                "这完全没有任何效果\n" +
                "但我知道会有人不看说明就勾选，所以留个彩蛋。\n" +
                "感谢使用本插件，意义重大 <3");
        }
        private static void Separator()
        {
            ImGui.Dummy(new Vector2(0, 5));
            ImGui.Separator();
            ImGui.Dummy(new Vector2(0, 5));
        }
    }
}
