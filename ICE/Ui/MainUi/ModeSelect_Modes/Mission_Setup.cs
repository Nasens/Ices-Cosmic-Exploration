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

        public static Mission_Table? MissionTable;
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
                    modeType = "标准";
                else if (relicMode)
                {
                    modeType = "Relic 刷取";
                    modeIcon = FontAwesomeIcon.ArrowUpRightDots;
                }
                else if (xpLeveling)
                {
                    modeType = "练级刷取";
                    modeIcon = FontAwesomeIcon.Leaf;
                }
                else if (goldMode)
                {
                    modeType = "金牌完成刷取";
                    modeIcon = FontAwesomeIcon.Trophy;
                }
                else if (agendaMode)
                {
                    modeType = "Cosmic 日程";
                    modeIcon = FontAwesomeIcon.ClipboardList;
                }

                ImGuiEx.IconWithText(modeIcon, $"{modeType} 模式");

                ImGui.SameLine(0, 10 * scale);

                // Adjust the Y position to center the button vertically with the text
                float textHeight = ImGui.GetTextLineHeight();
                float buttonHeight = ImGui.GetFrameHeight();
                float yOffset = (textHeight - buttonHeight) / 2f;
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + yOffset);

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
                            || C.StopOnceRelicFinished;
                if (AnyStop)
                {
                    ImGui.SameLine(0, 10 * scale);
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + yOffset);
                    ImGuiEx.Icon(FontAwesomeIcon.ExclamationTriangle);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();

                        ImGui.Text("看起来你启用了以下其中一项");
                        if (C.StopOnceHitCosmicScore)
                            ImGui.BulletText($"在 Cosmic 评分达到 [{C.CosmicScoreCap:N0}] 时停止");
                        if (C.StopWhenLevel)
                            ImGui.BulletText($"在等级达到 [{C.TargetLevel:N0}] 时停止");
                        if (C.StopOnceHitCosmoCredits)
                            ImGui.BulletText($"在 Cosmo 点数达到 [{C.CosmoCreditsCap:N0}] 时停止");
                        if (C.StopOnceHitLunarCredits)
                            ImGui.BulletText($"在星球点数达到 [{C.LunarCreditsCap:N0}] 时停止");
                        if (C.StopOnceRelicFinished)
                            ImGui.BulletText($"Relic 完成后停止");

                        ImGui.Text("所以如果它停止了而你不清楚原因……这可能就是原因");

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
                        ImGui.Text("嘿！你需要更新 Artisan 才能使用此模式，请至少更新到：");
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
                        ImGui.Text($"嘿！{unsupportedHub.DisplayName} 暂不支持练级。");
                        var missing = new List<string>();
                        if (!CosmicMoonRegistry.HasLevelingContent(unsupportedHub))
                            missing.Add("QuickLevelList 任务");
                        if (!CosmicMoonContent.HasGatheringRoutes(unsupportedHub.TerritoryId))
                            missing.Add("采集路线");
                        if (missing.Count > 0)
                            ImGui.Text($"仍需：{string.Join(", ", missing)}。");
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
                        ImGui.Text($"嘿！你当前版本的 AutoHook 暂不支持在这个星球上使用");
                        ImGui.Text($"你（目前）需要使用测试版本才能在这里自动钓鱼");
                        ImGui.Text($"如果你仍然继续运行并且它选中了钓鱼任务，将会再弹出一个警告……");
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
                        if (ImGui.Button("停止", new Vector2(150 * scale, 0)))
                        {
                            SchedulerMain.DisablePlugin();
                        }
                    }
                }

                ImGui.SameLine(0, 10 * scale);
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + yOffset);

                if (ImGui.Button("任务设置"))
                {
                    ImGui.OpenPopup("Mission Settings: Popup");
                }
                if (ImGui.BeginPopup("Mission Settings: Popup"))
                {
                    // TODO: Mission Settings
                    bool grindAllProvisionals = C.GrindAllProvisionals;
                    if (ImGui.Checkbox("临时任务：允许所有职业", ref grindAllProvisionals))
                    {
                        C.GrindAllProvisionals = grindAllProvisionals;
                        C.Save();
                    }
                    ImGuiEx.HelpMarker("启用此项后，会在你开始时所用职业的常规任务之外，\n" +
                                       "额外显示所有你可以刷取的天气/限时/序列任务。\n" +
                                       "如果你只想专注于某一个特定职业，请将其设为关闭");

                    bool allowCriticalsAllClass = C.GrindOffClassRedAlert;
                    if (ImGui.Checkbox("紧急任务：允许所有职业", ref allowCriticalsAllClass))
                    {
                        C.GrindOffClassRedAlert = allowCriticalsAllClass;
                        C.Save();
                    }
                    ImGuiEx.HelpMarker($"这将允许你为紧急任务/红色警报刷取其他职业。" +
                        $"（比如你正在玩 CRP，但弹出了一个 BSM 的红色警报）");

                    bool removeGold = C.RemoveAfterGold;
                    if (ImGui.Checkbox("达成金牌后移除任务", ref removeGold))
                    {
                        C.RemoveAfterGold = removeGold;
                        C.Save();
                    }
                    using (ImRaii.Disabled(!removeGold))
                    {
                        bool keepARanks = C.KeepARanks;
                        if (ImGui.Checkbox("保留\"A 级\"及以下任务", ref keepARanks))
                        {
                            C.KeepARanks = keepARanks;
                            C.Save();
                        }
                    }

                    ImGui.Checkbox("完成当前任务后停止", ref Mission_Settings.StopAfterCurrent);
                    bool relicTurnin = C.TurninRelic;
                    if (ImGui.Checkbox($"Relic 完成时交付##RelicTurnin_GeneralSetting", ref relicTurnin))
                    {
                        C.TurninRelic = relicTurnin;
                        C.Save();
                    }
                    ImGui.SameLine();
                    ImGui.TextDisabled("?");
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("这是关于它如何工作的提示。如果我以后修改了它，这个提示也会随之更新。\n" +
                                         "1：这会检查你当前的职业 [不是菜单里选的职业，而是实际当前职业] 来进行 Relic 交付。\n" +
                                         "2：你必须没有装备该工具，才能让它全自动运行。\n" +
                                         "\t- 这是因为我现在懒得把这部分代码加进去。（以后也许会改主意 *耸肩*）\n" +
                                         "3：这会优先于\"Relic 交付时停止\"，也就是说如果两者都启用，它会选择交付而不是停止，然后继续运行\n" +
                                         "4：如果你在制作职业上，交付后它会把你带回到你之前制作的地点。\n" +
                                         "\t- 这是可选的，你可以随意禁用，我只是喜欢这样，好让自己能回到所选的独立区域");
                    }

                    ImGui.Separator();
                    bool relic_AllowRedAlert = C.Relic_IncludeCriticals;
                    if (ImGui.Checkbox("Relic 模式：允许红色警报", ref relic_AllowRedAlert))
                    {
                        C.Relic_IncludeCriticals = relic_AllowRedAlert;
                        C.Save();
                    }

                    bool OnlySelected = C.XPRelicOnlyEnabled;
                    if (ImGui.Checkbox("Relic 模式：仅启用项", ref OnlySelected))
                    {
                        C.XPRelicOnlyEnabled = OnlySelected;
                        C.Save();
                    }
                    if (ImGui.Button("打开职业切换设置"))
                    {
                        C.SelectedTab = WindowSelection.CharacterSettings;
                    }

                    if (ImGui.Button("保存当前任务预设"))
                    {
                        ImGui.OpenPopup("Preset Save Editor");
                    }

                    if (ImGui.BeginPopup("Preset Save Editor"))
                    {
                        ImGui.InputText($"播放列表名称", ref newListName);
                        using (ImRaii.Disabled(string.IsNullOrEmpty(newListName)))
                        {
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
                        if (ImGui.Button("查看所有预设"))
                        {
                            ImGui.OpenPopup("Preset: List Viewer");
                        }

                        if (ImGui.BeginPopup("Preset: List Viewer"))
                        {
                            ImGui.Text($"加载任务预设");

                            if (ImGui.BeginTable($"Preset: TableViewer", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders))
                            {
                                ImGui.TableSetupColumn("Name");
                                ImGui.TableSetupColumn("Amount Enabled");

                                ImGui.TableHeadersRow();

                                ImGui.TableNextRow();
                                ImGui.TableSetColumnIndex(0);
                                ImGui.AlignTextToFramePadding();
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

                    ImGui_Ice.DrawRankButton("红色警报", MissionFilter.RedAlert, MissionTable);
                    ImGui_Ice.DrawRankButton("序列", MissionFilter.Sequence, MissionTable);
                    ImGui_Ice.DrawRankButton("天气", MissionFilter.Weather, MissionTable);
                    ImGui_Ice.DrawRankButton("限时", MissionFilter.Timed, MissionTable);
                    ImGui_Ice.DrawRankButton("大师", MissionFilter.Master, MissionTable);
                    ImGui_Ice.DrawRankButton("A 级", MissionFilter.ARank, MissionTable);
                    ImGui_Ice.DrawRankButton("B 级", MissionFilter.BRank, MissionTable);
                    ImGui_Ice.DrawRankButton("C 级", MissionFilter.CRank, MissionTable);
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
                        var filterCount = filterActive ? $" (共 {ItemCount})" : "";
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
