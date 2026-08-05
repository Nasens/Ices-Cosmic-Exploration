using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ECommons.GameHelpers;
using ICE.Ui.MainUi.ModeSelect_Modes.CosmicTable;
using ICE.Ui.MainUi.Settings;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.ImGuiTools;
using System.Collections.Generic;

namespace ICE.Ui.MainUi.ModeSelect_Modes
{
    internal class Mission_Setup
    {
        private static readonly Dictionary<string, uint> BattleJobs = new()
        {
            // Tanks
            { "Paladin", 19 },
            { "Warrior", 21 },
            { "Dark Knight", 32 },
            { "Gunbreaker", 37 },
    
            // Healers
            { "White Mage", 24 },
            { "Scholar", 28 },
            { "Astrologian", 33 },
            { "Sage", 40 },
    
            // Melee DPS
            { "Monk", 20 },
            { "Dragoon", 22 },
            { "Ninja", 30 },
            { "Samurai", 34 },
            { "Reaper", 39 },
            { "Viper", 41 },
    
            // Physical Ranged DPS
            { "Bard", 23 },
            { "Machinist", 31 },
            { "Dancer", 38 },
    
            // Magical Ranged DPS
            { "Black Mage", 25 },
            { "Summoner", 27 },
            { "Red Mage", 35 },
            { "Pictomancer", 42 }
        };

        public static CosmicTables.Mission_Table? MissionTable;
        private static List<CosmicHelper.MissionInfo> TableItems = [];
        private static int ItemCount = 0;
        private static string newListName = string.Empty;

        public static void Draw()
        {
            using var style = ImRaii.PushStyle(ImGuiStyleVar.ChildRounding, 10).Push(ImGuiStyleVar.ChildBorderSize, 1);

            // Header at the top
            float scale = ImGuiHelpers.GlobalScale;

            using (var headerChild = ImRaii.Child("##modeSelect_StandardHeader", new Vector2(0, 45 * scale), true, ImGuiWindowFlags.NoScrollbar))
            {
                if (!headerChild.Success) return;

                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 10 * scale);
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 5 * scale);

                string modeType = string.Empty;
                FontAwesomeIcon modeIcon = FontAwesomeIcon.List;

                bool standard = C.SelectedMode == ModeSelect.Standard;
                bool relicMode = C.SelectedMode == ModeSelect.RelicMode;
                bool xpLeveling = C.SelectedMode == ModeSelect.LevelMode;
                bool goldMode = C.SelectedMode == ModeSelect.MissionGoldMode;
                bool agendaMode = C.SelectedMode == ModeSelect.AgendaMode;


                if (standard)
                    // modeType = "Standard";
                    modeType = "标准";
                else if (relicMode)
                {
                    // modeType = "Relic Grind";
                    modeType = "宇宙工具刷取";
                    modeIcon = FontAwesomeIcon.ArrowUpRightDots;
                }
                else if (xpLeveling)
                {
                    // modeType = "Leveling Grind";
                    modeType = "练级";
                    modeIcon = FontAwesomeIcon.Leaf;
                }
                else if (goldMode)
                {
                    // modeType = "Gold Completion Grind";
                    modeType = "金牌完成";
                    modeIcon = FontAwesomeIcon.Trophy;
                }
                else if (agendaMode)
                {
                    // modeType = "Cosmic Agenda";
                    modeType = "宇宙议程";
                    modeIcon = FontAwesomeIcon.ClipboardList;
                }

                // ImGuiEx.IconWithText(modeIcon, $"{modeType} Mode");
                ImGuiEx.IconWithText(modeIcon, $"{modeType}模式");

                ImGui.SameLine(0, 10 * scale);

                // Adjust the Y position to center the button vertically with the text
                float textHeight = ImGui.GetTextLineHeight();
                float buttonHeight = ImGui.GetFrameHeight();
                float yOffset = (textHeight - buttonHeight) / 2f;
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + yOffset);

                // if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Play, "Mode Selection"))
                if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Play, "模式选择"))
                {
                    ImGui.OpenPopup("Mode Select | Select Mode Window");
                }
                if (ImGui.BeginPopup("Mode Select | Select Mode Window"))
                {
                    MainWindow.ModeSelection();

                    ImGui.EndPopup();
                }

                uint currentJobId = (uint)Player.Job;
                bool usingSupportedJob = CosmicHelper.CrafterJobList.Contains(currentJobId) || CosmicHelper.GatheringJobList.Contains(currentJobId);

                bool AnyStop = C.StopOnceHitCosmicScore
                             | C.StopWhenLevel
                            || C.StopOnceHitCosmoCredits
                            || C.StopOnceHitLunarCredits
                            || C.StopOnceRelicFinished
                            || C.StopOnceStandardMissionsGolded;
                if (AnyStop)
                {
                    ImGui.SameLine(0, 10 * scale);
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + yOffset);
                    ImGuiEx.Icon(FontAwesomeIcon.ExclamationTriangle);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();

                        // ImGui.Text("It appears that you have on of the following enabled");
                        ImGui.Text("以下停止条件已启用");
                        if (C.StopOnceHitCosmicScore)
                            // ImGui.BulletText($"Stop at Cosmic Score [{C.CosmicScoreCap:N0}]");
                            ImGui.BulletText($"达到宇宙分数时停止 [{C.CosmicScoreCap:N0}]");
                        if (C.StopWhenLevel)
                            // ImGui.BulletText($"Stop When Level [{C.TargetLevel:N0}]");
                            ImGui.BulletText($"达到等级时停止 [{C.TargetLevel:N0}]");
                        if (C.StopOnceHitCosmoCredits)
                            // ImGui.BulletText($"Stop once cosmo credit hit [{C.CosmoCreditsCap:N0}]");
                            ImGui.BulletText($"达到宇宙点数时停止 [{C.CosmoCreditsCap:N0}]");
                        if (C.StopOnceHitLunarCredits)
                            // ImGui.BulletText($"Stop once planetary credit hit [{C.LunarCreditsCap:N0}]");
                            ImGui.BulletText($"达到行星点数时停止 [{C.LunarCreditsCap:N0}]");
                        if (C.StopOnceRelicFinished)
                            // ImGui.BulletText($"Stop once relic completed");
                            ImGui.BulletText($"宇宙工具完成时停止");
                        if (C.StopOnceStandardMissionsGolded)
                            // ImGui.BulletText("Stop when all standard missions are golded");
                            ImGui.BulletText("所有标准任务金牌后停止");

                        // ImGui.Text("So if you stop and you're unsure why... this might be why");
                        ImGui.Text("若插件意外停止，可能是以下原因");

                        ImGui.EndTooltip();
                    }
                }

                ImGui.SameLine(0, 10 * scale);
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + yOffset);

                bool unsupportedArtisan = false; // xpLeveling && CosmicHelper.CrafterJobList.Contains((uint)Player.Job);
                bool unsupportedMoon = xpLeveling 
                    && CosmicMoonRegistry.TryGetMoon(Player.Territory.RowId, out var currentMoon)
                    && !CosmicMoonRegistry.HasLevelingContent(currentMoon);

                // Leveling on a hub requires QuickLevelList entries; gathering still needs route YAML per territory
                using (ImRaii.Disabled(SchedulerMain.State != IceState.Idle || !usingSupportedJob || unsupportedMoon))
                {
                    // if (ImGui.Button("Start", new Vector2(150 * scale, 0)))
                    if (ImGui.Button("开始", new Vector2(150 * scale, 0)))
                    {
                        SchedulerMain.EnablePlugin();
                    }
                }

                if (unsupportedArtisan)
                {
                    ImGui.SameLine(0, 10 * scale);
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + yOffset);
                    ImGuiEx.Icon(EColor.Red, FontAwesomeIcon.ExclamationTriangle);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        // ImGui.Text("Hey! You need to update artisan to use this mode, please update to at minimum:");
                        ImGui.Text("请更新 Artisan 至 4.0.4.29 及以上以使用此模式：");
                        ImGui.Text("4.0.4.29");
                        ImGui.EndTooltip();
                    }
                }
                else if (unsupportedMoon && CosmicMoonRegistry.TryGetMoon(Player.Territory.RowId, out var unsupportedHub))
                {
                    ImGui.SameLine(0, 10 * scale);
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + yOffset);
                    ImGuiEx.Icon(EColor.Red, FontAwesomeIcon.ExclamationTriangle);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        // ImGui.Text($"Hey! {unsupportedHub.DisplayName} is not supported for leveling yet.");
                        ImGui.Text($"{unsupportedHub.DisplayName} 暂不支持练级模式。");
                        var missing = new List<string>();
                        if (!CosmicMoonRegistry.HasLevelingContent(unsupportedHub))
                            // missing.Add("QuickLevelList missions");
                            missing.Add("QuickLevelList 任务");
                        if (!CosmicMoonContent.HasGatheringRoutes(unsupportedHub.TerritoryId))
                            // missing.Add("gathering routes");
                            missing.Add("采集路线");
                        if (missing.Count > 0)
                            // ImGui.Text($"Still needed: {string.Join(", ", missing)}.");
                            ImGui.Text($"仍缺少：{string.Join("、", missing)}。");
                        ImGui.EndTooltip();
                    }
                }
                if (!P.AutoHook.UpdatedPlugin() && CosmicMoonRegistry.Auxesia.TerritoryId == Player.Territory.RowId)
                {
                    ImGui.SameLine(0, 10 * scale);
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + yOffset);
                    ImGuiEx.Icon(EColor.Red, FontAwesomeIcon.ExclamationTriangle);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        // ImGui.Text($"Hey! Your version of autohook is not currently supported on this planet");
                        ImGui.Text("当前星球不支持您使用的 AutoHook 版本");
                        // ImGui.Text($"You need to (currently) be on the testing version to be able fish automated here");
                        ImGui.Text("目前需要使用测试版才能在此自动捕鱼");
                        // ImGui.Text($"There will be another warning to pop up if you try and run this still and it selects a fishing mission...");
                        ImGui.Text("若仍运行并选中捕鱼任务，将再次弹出警告…");
                        ImGui.EndTooltip();
                    }
                }

                ImGui.SameLine(0, 10 * scale);
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + yOffset);

                using (ImRaii.Disabled(SchedulerMain.State == IceState.Idle))
                {
                    using (ImRaii.PushColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 1.0f)))
                    using (ImRaii.PushColor(ImGuiCol.ButtonHovered, new Vector4(0.9f, 0.3f, 0.3f, 1.0f)))
                    using (ImRaii.PushColor(ImGuiCol.ButtonActive, new Vector4(0.7f, 0.1f, 0.1f, 1.0f)))
                    {
                        // if (ImGui.Button("Stop", new Vector2(150 * scale, 0)))
                        if (ImGui.Button("停止", new Vector2(150 * scale, 0)))
                        {
                            SchedulerMain.DisablePlugin();
                        }
                    }
                }

                ImGui.SameLine(0, 10 * scale);
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + yOffset);

                // if (ImGui.Button("Mission Settings"))
                if (ImGui.Button("任务设置"))
                {
                    ImGui.OpenPopup("Mission Settings: Popup");
                }
                if (ImGui.BeginPopup("Mission Settings: Popup"))
                {
                    // TODO: Mission Settings
                    bool grindAllProvisionals = C.GrindAllProvisionals;
                    // if (ImGui.Checkbox("Provisional: Allow All Classes", ref grindAllProvisionals))
                    if (ImGui.Checkbox("临时任务：允许所有职业", ref grindAllProvisionals))
                    {
                        C.GrindAllProvisionals = grindAllProvisionals;
                        C.Save();
                    }
                    // ImGuiEx.HelpMarker("Enabling this will show you all weather/timed/sequence missions that you can grind,\n" +
                    //                    "ON TOP OF doing the normal missions for whichever class you start on.\n" +
                    //                    "If you just want to focus one specific class, set this to false");
                    ImGuiEx.HelpMarker("启用后，除当前职业常规任务外，还会显示所有可刷的天气/限时/序列任务。\n" +
                                       "若只想专注单一职业，请关闭此项。");

                    bool allowCriticalsAllClass = C.GrindOffClassRedAlert;
                    // if (ImGui.Checkbox("Critical: Allow All Classes", ref allowCriticalsAllClass))
                    if (ImGui.Checkbox("紧急任务：允许所有职业", ref allowCriticalsAllClass))
                    {
                        C.GrindOffClassRedAlert = allowCriticalsAllClass;
                        C.Save();
                    }
                    // ImGuiEx.HelpMarker($"This will allow you to grind other classes for criticals/red alerts. " +
                    //     $"(So if you're on crp, but a bsm red alert pops up)");
                    ImGuiEx.HelpMarker("允许跨职业刷紧急通告任务。\n" +
                        "（例如当前为 CRP，但 BSM 的紧急通告弹出时也会接取）");

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

                    // ImGui.Checkbox("Stop after current mission", ref Mission_Settings.StopAfterCurrent);
                    ImGui.Checkbox("当前任务完成后停止", ref Mission_Settings.StopAfterCurrent);
                    bool relicTurnin = C.TurninRelic;
                    // if (ImGui.Checkbox($"Turnin if relic is complete##RelicTurnin_GeneralSetting", ref relicTurnin))
                    if (ImGui.Checkbox($"宇宙工具完成时自动交付##RelicTurnin_GeneralSetting", ref relicTurnin))
                    {
                        C.TurninRelic = relicTurnin;
                        C.Save();
                    }
                    ImGui.SameLine();
                    ImGui.TextDisabled("?");
                    if (ImGui.IsItemHovered())
                    {
                        // ImGui.SetTooltip("THIS IS YOUR HEADS UP ON HOW THIS WORKS. If I change this in the future, this tooltip will also change.\n" +
                        //                  "1: This will check for your current CLASS [not menu class, actual current class] for relic turnin.\n" +
                        //                  "2: You must not have the tool eqipped for this to run full auto. \n" +
                        //                  "\t- This is due to the fact that I cba coding this in at this time. (might change my mind in the future *shrugs*)\n" +
                        //                  "3: This will take prio over \"Stop @ Relic Turnin\", in the sense that if you have both enabled, it will turnin vs stop. And continue about it's day\n" +
                        //                  "4: If you're on a crafting class, it will return you back to the stop you were crafting post turnin. \n" +
                        //                  "\t- This is optional, you can disable it at your own free will, I just like this so I can just go back to an isolated area of my choosing");
                        ImGui.SetTooltip("Relic 自动交付说明（若日后逻辑变更，此说明也会更新）：\n" +
                                         "1：按当前实际职业（非菜单选择）检测 Relic 交付。\n" +
                                         "2：未装备工具时才能全自动运行。\n" +
                                         "\t- 暂未实现装备工具时的自动逻辑。\n" +
                                         "3：若与「宇宙工具完成时停止」同时启用，将优先交付而非停止。\n" +
                                         "4：制作职业交付后会返回交付前的制作位置。\n" +
                                         "\t- 可选行为，可按需关闭。");
                    }

                    ImGui.Separator();
                    bool relic_AllowRedAlert = C.Relic_IncludeCriticals;
                    // if (ImGui.Checkbox("Relic Mode: Allow Red Alerts", ref relic_AllowRedAlert))
                    if (ImGui.Checkbox("宇宙工具模式：允许紧急探索任务", ref relic_AllowRedAlert))
                    {
                        C.Relic_IncludeCriticals = relic_AllowRedAlert;
                        C.Save();
                    }

                    bool OnlySelected = C.XPRelicOnlyEnabled;
                    // if (ImGui.Checkbox("Relic Mode: Only Enabled", ref OnlySelected))
                    if (ImGui.Checkbox("宇宙工具模式：仅已启用任务", ref OnlySelected))
                    {
                        C.XPRelicOnlyEnabled = OnlySelected;
                        C.Save();
                    }
                    // if (ImGui.Button("Open Job Swap Settings"))
                    if (ImGui.Button("打开职业切换设置"))
                    {
                        C.SelectedTab = WindowSelection.CharacterSettings;
                    }

                    // if (ImGui.Button("Save Current Mission Preset"))
                    if (ImGui.Button("保存当前任务预设"))
                    {
                        ImGui.OpenPopup("Preset Save Editor");
                    }

                    if (ImGui.BeginPopup("Preset Save Editor"))
                    {
                        // ImGui.InputText($"Playlist Name", ref newListName);
                        ImGui.InputText($"预设名称", ref newListName);
                        using (ImRaii.Disabled(string.IsNullOrEmpty(newListName)))
                        {
                            // if (ImGui.Button("Save New List"))
                            if (ImGui.Button("保存新列表"))
                            {
                                List<uint> new_Playlist = new();
                                foreach (var mission in C.MissionConfig.Where(x => x.Value.Enabled))
                                {
                                    new_Playlist.Add(mission.Key);
                                }
                                if (C.Mission_Playlist.ContainsKey(newListName))
                                {
                                    C.Mission_Playlist[newListName] = new_Playlist;
                                }
                                else
                                {
                                    C.Mission_Playlist.Add(newListName, new_Playlist);
                                }
                                C.Save();
                                ImGui.CloseCurrentPopup();
                            }
                        }

                        ImGui.EndPopup();
                    }

                    if (C.Mission_Playlist.Count > 0)
                    {
                        // if (ImGui.Button("View All Presets"))
                        if (ImGui.Button("查看全部预设"))
                        {
                            ImGui.OpenPopup("Preset: List Viewer");
                        }

                        if (ImGui.BeginPopup("Preset: List Viewer"))
                        {
                            // ImGui.Text($"Load Mission Preset");
                            ImGui.Text($"加载任务预设");

                            if (ImGui.BeginTable($"Preset: TableViewer", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders))
                            {
                                // ImGui.TableSetupColumn("Name");
                                ImGui.TableSetupColumn("名称");
                                // ImGui.TableSetupColumn("Amount Enabled");
                                ImGui.TableSetupColumn("已启用数量");

                                ImGui.TableHeadersRow();

                                ImGui.TableNextRow();
                                ImGui.TableSetColumnIndex(0);
                                ImGui.AlignTextToFramePadding();
                                // ImGui.Text($"Clear All");
                                ImGui.Text($"全部清除");
                                ImGui.SameLine();
                                if (ImGuiEx.IconButton(FontAwesomeIcon.ArrowUpRightFromSquare, $"FreshPreset_Button"))
                                {
                                    foreach (var mission in C.MissionConfig)
                                    {
                                        mission.Value.Enabled = false;
                                    }
                                    C.Save();
                                    ImGui.CloseCurrentPopup();
                                }

                                foreach (var item in C.Mission_Playlist)
                                {
                                    ImGui.TableNextRow();
                                    ImGui.TableSetColumnIndex(0);
                                    ImGui.AlignTextToFramePadding();
                                    ImGui.Text($"{item.Key}");
                                    ImGui.SameLine();
                                    if (ImGuiEx.IconButton(FontAwesomeIcon.ArrowUpRightFromSquare, $"{item.Key}_Button"))
                                    {
                                        foreach (var mission in C.MissionConfig)
                                        {
                                            if (item.Value.Contains(mission.Key))
                                                mission.Value.Enabled = true;
                                            else
                                                mission.Value.Enabled = false;
                                        }
                                        C.Save();
                                        ImGui.CloseCurrentPopup();
                                    }
                                    if (ImGui.IsItemHovered())
                                    {
                                        // ImGui.SetTooltip("Import Missions");
                                        ImGui.SetTooltip("导入任务");
                                    }

                                    ImGui.TableNextColumn();
                                    ImGui.AlignTextToFramePadding();
                                    ImGui.Text($"{item.Value.Count}");

                                    ImGui.TableNextColumn();
                                    if (ImGuiEx.IconButton(FontAwesomeIcon.Trash, $"{item.Key}_Remove"))
                                    {
                                        C.Mission_Playlist.Remove(item);
                                        C.Save();
                                    }
                                    if (ImGui.IsItemHovered())
                                    {
                                        // ImGui.SetTooltip("Remove from list");
                                        ImGui.SetTooltip("从列表移除");
                                    }
                                }

                                ImGui.EndTable();
                            }

                            ImGui.EndPopup();
                        }
                    }


                ImGui.EndPopup();
                }
            }

            using (var bodyChild = ImRaii.Child("##modeSelect_Body", new Vector2(0, -1), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                if (!bodyChild.Success) return;

                float scrollbarSize = ImGui.GetStyle().ScrollbarSize;
                float buttonRowHeight = (ImGui.GetTextLineHeight() + 8 * scale + 4 * scale) + scrollbarSize;

                using (var missionButtons = ImRaii.Child("##tab_scroll", new Vector2(0, buttonRowHeight), false, ImGuiWindowFlags.HorizontalScrollbar))
                {
                    if (!missionButtons.Success)
                        return;

                    // ImGui_Ice.DrawRankButton("Red Alert", MissionFilter.RedAlert, MissionTable);
                    ImGui_Ice.DrawRankButton("紧急通告", MissionFilter.RedAlert, MissionTable);
                    // ImGui_Ice.DrawRankButton("Sequence", MissionFilter.Sequence, MissionTable);
                    ImGui_Ice.DrawRankButton("序列", MissionFilter.Sequence, MissionTable);
                    // ImGui_Ice.DrawRankButton("Weather", MissionFilter.Weather, MissionTable);
                    ImGui_Ice.DrawRankButton("天气", MissionFilter.Weather, MissionTable);
                    // ImGui_Ice.DrawRankButton("Timed", MissionFilter.Timed, MissionTable);
                    ImGui_Ice.DrawRankButton("限时", MissionFilter.Timed, MissionTable);
                    // ImGui_Ice.DrawRankButton("Master", MissionFilter.Master, MissionTable);
                    ImGui_Ice.DrawRankButton("大师", MissionFilter.Master, MissionTable);
                    // ImGui_Ice.DrawRankButton("A Rank", MissionFilter.ARank, MissionTable);
                    ImGui_Ice.DrawRankButton("A 级", MissionFilter.ARank, MissionTable);
                    // ImGui_Ice.DrawRankButton("B Rank", MissionFilter.BRank, MissionTable);
                    ImGui_Ice.DrawRankButton("B 级", MissionFilter.BRank, MissionTable);
                    // ImGui_Ice.DrawRankButton("C Rank", MissionFilter.CRank, MissionTable);
                    ImGui_Ice.DrawRankButton("C 级", MissionFilter.CRank, MissionTable);
                    // ImGui_Ice.DrawRankButton("D Rank", MissionFilter.DRank, MissionTable);
                    ImGui_Ice.DrawRankButton("D 级", MissionFilter.DRank, MissionTable);

                    ImGui_Ice.EndCategoryButtonRow();
                }

                var bottomSpace = ImGui.GetTextLineHeight() + 6f;
                bottomSpace += 12f; // prevent the tabs from creating a scrollbar

                Vector2 size = new(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y - bottomSpace);
                if (ImGui.BeginChild("###MissionTableV3", size, false))
                {
                    try
                    {
                        if (MissionTable == null && CosmicHelper.SheetMissionDict.Count > 0)
                        {
                            foreach (var mission in CosmicHelper.SheetMissionDict)
                            {
                                CosmicHelper.MissionInfo missionDetails = new() { Id = mission.Key };
                                TableItems.Add(missionDetails);
                            }
                            ItemCount = TableItems.Count();
                            MissionTable = new(TableItems);
                        }
                        var filterActive = MissionTable.FilteredItems.Count != 0 && MissionTable.FilteredItems.Count != ItemCount;
                        var filterCount = filterActive ? $" (of {ItemCount})" : "";
                        var height = ImGui.GetFrameHeight();
                        MissionTable.Draw(height + 4f);
                    }
                    catch (Exception ex)
                    {
                        IceLogging.Error(ex.Message, "Drawing Mission Table");
                    }
                }
                ImGui.EndChild();
            }
        }
    }
}
