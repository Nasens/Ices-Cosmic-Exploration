using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ICE.Ui.MainUi.ModeSelect_Modes;
using ICE.Ui.MainUi.ModeSelect_Modes.CosmicTable;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.GatheringHelper;
using ICE.Utilities.ImGuiTools;
using OtterGui;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TerraFX.Interop.Windows;
using static ICE.ConfigFiles.Config.MissionSettings;
using static MissionTimer;

namespace ICE.Ui
{
    internal class Window_ExternalDetails : Window
    {
        public static uint SelectedMission = 0;

        public static List<string> JokeList = new()
        {
            // "What is a pirates favorite letter?\nYou might thing it's R, but tis first love was the C\n(It helps if you verbally say it like a pirate)",
            "海盗最喜欢的字母是什么？\n你可能以为是 R，但他初恋是 C\n（用海盗腔念出来效果更好）",

            // "You know, I was reading this book about anti-gravity recently,\nand honestly I'm having a hard time putting it down",
            "最近在读一本关于反重力的书，\n说实话，我根本放不下来",

            // "Why are tennis pros always hugging each other?\nBecause they start their match at \"Love All\"",
            "为什么网球选手总是互相拥抱？\n因为他们比赛开始时是「Love All」（零比零）",

            // "Why can't ghost have babies?\nBecause they have hallow-eenies",
            "为什么幽灵不能有宝宝？\n因为他们有「万圣空」",

            // "How do you save a drowning pirate?\nYou give him Cprrrrrr",
            "怎么救落水的海盗？\n给他做 Cprrrrrr（心肺复苏）",

            // "What is a skeleton's favorite snack?\nRibs! Spare Ribs!",
            "骷髅最喜欢的零食是什么？\n肋骨！Spare Ribs（备用肋骨）！",

            // "Honestly, just wanted to say thank you for using my plugin, you're appreciated <3",
            "说真的，只是想感谢你使用我的插件，你很棒 <3",

            // "Knock knock\n[This is where you say who's there]\nLettuce\n[Lettuce who]\nLettuce in",
            "咚咚咚\n[这里你说「谁啊」]\n生菜\n[生菜谁]\n生菜进来",

            // "What do you a dinosaur that only has one eye?\nA \"Doyouthinkheseemesaurs\"",
            "只有一只眼睛的恐龙叫什么？\n「Do-you-think-he-saw-us-aurus（你觉得他看见我们了吗 龙）」",

            // "So... you're telling me a shrimp fried this rice?",
            "所以……你是说一只虾炒了这碗饭？",

            // "Thank you everyone who's helped make this possible.\nStrife special shoutout...\nWah thank you for the UI...\nPuni.sh in general...",
            "感谢所有让这一切成为可能的人。\n特别感谢 Strife 帮我做了我不想碰的捕鱼部分\n（抱歉让你从大鱼开始 #不抱歉#满满的爱）\n感谢 Wah 的 UI，一如既往地漂亮\n感谢 Puni.sh 的各位回答我的各种问题\n"
        };
        public static int jokeId = 0;

        // public Window_ExternalDetails() : base($"Ice's Cosmic Exploration | Mission Details")
        public Window_ExternalDetails() : base($"Ice 宇宙探索 | 任务详情")
        {
            Flags = ImGuiWindowFlags.None;
            SizeConstraints = new()
            {
                MinimumSize = new Vector2(500, 500)
            };
            P.windowSystem.AddWindow(this);
        }

        public void Dispose()
        {
            P.windowSystem.RemoveWindow(this);
        }

        public override void OnOpen()
        {
            Collapsed = false;
            BringToFront();
            CollapsedCondition = ImGuiCond.Appearing;
        }

        private bool _openStatsTab = false;
        public void OpenToStatsTab(uint missionId)
        {
            SelectedMission = missionId;
            P.externalDetails.IsOpen = true;
            _openStatsTab = true;
        }

        public override void Draw()
        {
            if (CosmicHelper.SheetMissionDict.TryGetValue(SelectedMission, out var sheetInfo))
            {
                // ImGui.Text($"Mission:");
                ImGui.Text($"任务:");
                ImGui.SameLine(0, 5);
                ImGui.TextDisabled($"[{SelectedMission}]");
                ImGui.SameLine(0, 5);
                ImGui.Text($"{sheetInfo.Name}");

                if (ImGui.BeginTabBar("Mission Details Master Tabs"))
                {
                    // if (ImGui.BeginTabItem("Details"))
                    if (ImGui.BeginTabItem("详情"))
                    {
                        MissionDetails(sheetInfo);
                        ImGui.EndTabItem();
                    }

                    if (CosmicHelper.CrafterJobList.ContainsAny(sheetInfo.Jobs))
                    {
                        // if (ImGui.BeginTabItem("Craft Details"))
                        if (ImGui.BeginTabItem("制作详情"))
                        {
                            CraftDetails(sheetInfo);
                            ImGui.EndTabItem();
                        }
                    }

                    var statsFlag = _openStatsTab ? ImGuiTabItemFlags.SetSelected : ImGuiTabItemFlags.None;
                    _openStatsTab = false;

                    // if (ImGui.BeginTabItem("Completion Stats", statsFlag))
                    if (ImGui.BeginTabItem("完成统计", statsFlag))
                    {
                        StatInfo(sheetInfo);
                        ImGui.EndTabItem();
                    }
                    ImGui.EndTabBar();
                }
            }
        }
        private static IDalamudTextureWrap TrophyIcon(TurninState state)
        {
            string resource = state switch
            {
                TurninState.Bronze => "ICE.Resources.TrophyIcons.bronze_trophy.png",
                TurninState.Silver => "ICE.Resources.TrophyIcons.silver_trophy.png",
                TurninState.Gold => "ICE.Resources.TrophyIcons.gold_trophy.png",
                _ => "ICE.Resources.TrophyIcons.bronze_trophy.png",
            };

            var texture = Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), resource).GetWrapOrEmpty();
            return texture;
        }
        private static void MissionDetails(CosmicHelper.CosmicInfo mission)
        {
            float scale = ImGuiHelpers.GlobalScale;
            Vector2 size = new Vector2(24 * scale, 24 * scale);

            void ItemInfo(uint itemId, uint amount)
            {
                if (amount != 0)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    if (ExcelHelper.ItemSheet.TryGetRow(itemId, out var itemSheet))
                    {
                        var iconId = (int)itemSheet.Icon;
                        if (Svc.Texture.TryGetFromGameIcon(iconId, out var iconTexture))
                        {
                            ImGui_Ice.ImageButtonWithText(iconTexture.GetWrapOrEmpty(), $"{itemSheet.Name}", $"{itemSheet.Name}", size);
                        }
                    }

                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text($"{amount:N0}");
                }
            }

            void ScoreInfo(TurninState state, string label, uint score)
            {
                var texture = TrophyIcon(state);
                if (score != 0)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui_Ice.ImageButtonWithText(texture, label, label, size);

                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text($"{score:N0}");
                }
            }

            void RelicInfo()
            {
                var exps = mission.RelicXpInfo
                    .Where(x => x.Value != 0)
                    .OrderBy(x => x.Key)
                    .ToList();

                if (exps.Count != 0)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text($"Mission Exp[s]");

                    ImGui.TableNextColumn();
                    for (int i = 0; i < exps.Count; i++)
                    {
                        var (tier, value) = (exps[i].Key, exps[i].Value);

                        Vector4 pillColor = tier switch
                        {
                            1 => new Vector4(0.9f, 0.8f, 0.1f, 0.8f), // I   - Yellow
                            2 => new Vector4(0.9f, 0.5f, 0.1f, 0.8f), // II  - Orange
                            3 => new Vector4(0.8f, 0.2f, 0.2f, 0.8f), // III - Red
                            4 => new Vector4(0.6f, 0.2f, 0.8f, 0.8f), // IV  - Purple
                            5 => new Vector4(0.2f, 0.4f, 0.9f, 0.8f), // V   - Blue
                            6 => new Vector4(0.4f, 0.8f, 1.0f, 0.8f), // VI  - Light Blue
                            7 => new Vector4(0.2f, 0.8f, 0.3f, 0.8f), // VII - Green
                            _ => new Vector4(0.5f, 0.5f, 0.5f, 0.8f),
                        };

                        string roman = tier switch
                        {
                            1 => "I",
                            2 => "II",
                            3 => "III",
                            4 => "IV",
                            5 => "V",
                            6 => "VI",
                            7 => "VII",
                            _ => "?"
                        };


                        using (ImRaii.PushColor(ImGuiCol.Button, pillColor)
                                     .Push(ImGuiCol.ButtonHovered, pillColor with { W = 1.0f })
                                     .Push(ImGuiCol.ButtonActive, pillColor))
                        {
                            ImGui.SmallButton($"{roman}:{value}##exp{tier}");
                        }

                        if (i < exps.Count - 1)
                            ImGui.SameLine();
                    }
                }
            }

            if (ImGui.BeginTable("Detailed Mission Info", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
            {
                // ImGui.TableSetupColumn("Name");
                ImGui.TableSetupColumn("名称");
                // ImGui.TableSetupColumn("Info");
                ImGui.TableSetupColumn("信息");

                // Cosmocredits
                ItemInfo(45690, mission.CosmoCredit);

                // Planetary Credit
                if (CosmicMoonRegistry.TryGetPlanetCreditItemId(mission.TerritoryId, out var planetCreditId))
                {
                    ItemInfo(planetCreditId, mission.LunarCredit);
                }

                if (CosmicMoonRegistry.TryGetDronebit(mission.TerritoryId, out var dronebitId))
                {
                    ItemInfo(dronebitId.creditId, mission.DronebitReward);
                }

                if (CosmicMoonRegistry.TokenIds.TryGetValue(mission.TerritoryId, out var tokenId))
                {
                    ItemInfo(tokenId.tokenId, mission.TokenItemAmount);
                }

                RelicInfo();

                // ScoreInfo(TurninState.Gold, "Class Score", mission.ClassScore)
                ScoreInfo(TurninState.Gold, "职业分数", mission.ClassScore);

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.AlignTextToFramePadding();
                // ImGui.Text($"Job(s)")
                ImGui.Text($"职业");

                ImGui.TableNextColumn();
                ImGui_Ice.DrawJobIconButton("Jobs", mission.Jobs);

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.AlignTextToFramePadding();
                // ImGui.Text($"Completed:")
                ImGui.Text($"已完成：");

                ImGui.TableNextColumn();
                ImGui_Ice.CompletionStatusIcon(mission);

                // ScoreInfo(TurninState.Bronze, "Bronze Requirement", mission.BronzeScore)
                ScoreInfo(TurninState.Bronze, "铜牌要求", mission.BronzeScore);
                // ScoreInfo(TurninState.Silver, "Silver Requirement", mission.SilverScore)
                ScoreInfo(TurninState.Silver, "银牌要求", mission.SilverScore);
                // ScoreInfo(TurninState.Gold, "Gold Requirement", mission.GoldScore)
                ScoreInfo(TurninState.Gold, "金牌要求", mission.GoldScore);

                if (mission.MarkerId != 0)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    // ImGui.Text("Gathering Zone");
                    ImGui.Text("采集区域");

                    ImGui.TableNextColumn();

                    ImGui.PushFont(UiBuilder.IconFont);
                    ImGui.Text(FontAwesomeIcon.Flag.ToIconString());
                    ImGui.PopFont();
                    if (ImGui.IsItemClicked())
                    {
                        Utils.SetGatheringRing(mission.TerritoryId, (int)mission.MapPosition.X, (int)mission.MapPosition.Y, mission.Radius, mission.Name);
                    }
                }

                if (GatheringUtil.CriticalSpots.TryGetValue(mission.Critical_MapKey, out var criticalInfo))
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    // ImGui.Text("Critical Area");
                    ImGui.Text("紧急区域");

                    ImGui.TableNextColumn();
                    ImGuiEx.Icon(FontAwesomeIcon.Flag);
                    if (ImGui.IsItemClicked())
                    {
                        Utils.SetFlagForNPC(mission.TerritoryId, criticalInfo.X, criticalInfo.Y);
                    }
                }

                if (mission.TemporaryAction.ActionId != 0)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    // ImGui.Text("Mission Skill")
                    ImGui.Text("任务技能");

                    ImGui.TableNextColumn();
                    ImGui_Ice.ImageButtonWithText(mission.TemporaryAction.Icon.GetWrapOrEmpty(), $"{mission.TemporaryAction.Name}", "tempAction", size);
                    if (ImGui.IsItemHovered() && mission.TemporaryAction.UseAmount != 0)
                    {
                        // ImGui.SetTooltip($"Max Use: {mission.TemporaryAction.UseAmount}")
                        ImGui.SetTooltip($"最大使用次数：{mission.TemporaryAction.UseAmount}");
                    }
                }

                if (mission.Supplies.Count > 0)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    // ImGui.Text("Supplied Items")
                    ImGui.Text("补给物品");

                    ImGui.TableNextColumn();
                    for (int i = 0; i < mission.Supplies.Count(); i++)
                    {
                        var supply = mission.Supplies[i];
                        ImGui_Ice.ImageButtonWithText(supply.Icon.GetWrapOrEmpty(), $"{supply.Count:N0}", "Supplyitem", size);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();
                            // ImGui.Text($"ItemId: {supply.ItemId}")
                            ImGui.Text($"物品 ID：{supply.ItemId}");
                            // ImGui.Text($"Name: {supply.Name}")
                            ImGui.Text($"名称：{supply.Name}");
                            ImGui.EndTooltip();
                        }
                        if (i+1 < mission.Supplies.Count())
                        {
                            ImGui.SameLine();
                            ImGui.Text(" | ");
                            ImGui.SameLine();
                        }
                    }
                }

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                // ImGui.Text($"Notes [Hover over]")
                ImGui.Text($"备注 [悬停查看]");

                ImGui.TableNextColumn();
                var HasSPM = mission.BestSPM.SPM > 0;
                var HasSequence = mission.SequenceMissions_Next.Count() > 0 || mission.SequenceMissions_Previous.Count() > 0;
                var HasUnlockable = mission.MissionUnlock.Count() > 0;

                if (HasSPM)
                {
                    ImGuiEx.IconButton(FontAwesomeIcon.Trophy);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        // ImGui.Text($"Average SPM: {mission.BestSPM.SPM:N2}")
                        ImGui.Text($"平均每分钟得分：{mission.BestSPM.SPM:N2}");
                        ImGui.Text($"{mission.BestSPM.NoteInfo}");
                        ImGui.EndTooltip();
                    }
                }
                if (HasSequence)
                {
                    if (HasSPM)
                        ImGui.SameLine();

                    ImGuiEx.IconButton(FontAwesomeIcon.ListOl);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        if (mission.SequenceMissions_Next.Count() > 0)
                        {
                            // ImGui.Text("Next Sequence:")
                            ImGui.Text("后续序列：");
                            foreach (var missionSeq in mission.SequenceMissions_Next)
                            {
                                var seqInfo = CosmicHelper.SheetMissionDict[missionSeq];
                                ImGui.Text($"[{missionSeq}] {seqInfo.Name}");
                            }
                        }
                        if (mission.SequenceMissions_Previous.Count() > 0)
                        {
                            // ImGui.Text("Previous Sequence:")
                            ImGui.Text("前置序列：");
                            foreach (var missionSeq in mission.SequenceMissions_Previous)
                            {
                                var seqInfo = CosmicHelper.SheetMissionDict[missionSeq];
                                ImGui.Text($"[{missionSeq}] {seqInfo.Name}");
                            }
                        }
                        ImGui.EndTooltip();
                    }
                }
                if (HasUnlockable)
                {
                    if (HasSPM || HasSequence)
                    {
                        ImGui.SameLine();
                    }
                    if (Svc.Texture.GetFromGame("ui/uld/WKSMission_hr1.tex") is { } tex)
                    {
                        var frameHeight = ImGui.GetFrameHeight();
                        if (tex.TryGetWrap(out var wrap, out var exc))
                        {
                            ImGui.ImageButton(wrap.Handle, size, new Vector2(0.2347f, 0.3500f), new Vector2(0.2959f, 0.6500f));
                        }
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        // ImGui.Text("The following missions are required to have gold before you can do this one")
                        ImGui.Text("以下任务需要达成金牌后才能执行本任务");
                        foreach (var missionUnlock in mission.MissionUnlock)
                        {
                            ImGui_Ice.CompletionStatusIcon(CosmicHelper.SheetMissionDict[missionUnlock]);
                            ImGui.SameLine();
                            ImGui.Text($"[{mission}] - {CosmicHelper.SheetMissionDict[missionUnlock].Name}");
                        }
                        ImGui.EndTooltip();
                    }
                }

                ImGui.EndTable();
            }

            if (mission.ExpModifier_3 != 0)
            {
                if (ImGui.BeginTable("Exp Rewards", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
                {
                    // ImGui.TableSetupColumn("Class Exp")
                    ImGui.TableSetupColumn("职业经验");
                    // ImGui.TableSetupColumn("% of Level")
                    ImGui.TableSetupColumn("等级百分比");

                    ImGui.TableHeadersRow();

                    if (mission.ExpModifier_1 != 0)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        // ImGui.Text("Lv. 10-49")
                        ImGui.Text("等级 10-49");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{mission.ExpModifier_1}%");
                    }

                    if (mission.ExpModifier_2 != 0)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        // ImGui.Text("Lv. 50-89")
                        ImGui.Text("等级 50-89");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{mission.ExpModifier_2}%");
                    }

                    if (mission.ExpModifier_3 != 0)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        // ImGui.Text("Lv. 90-99")
                        ImGui.Text("等级 90-99");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{mission.ExpModifier_3}%");
                    }

                    ImGui.EndTable();
                }
            }

            // ImGui.Text("Mission Atributes")
            ImGui.Text("任务属性");
            if (mission.Attributes == MissionAttributes.None)
            {
                // ImGui.Text("None")
                ImGui.Text("无");
                return;
            }
            else
            {
                foreach (MissionAttributes flag in Enum.GetValues<MissionAttributes>())
                {
                    if (flag != MissionAttributes.None && mission.Attributes.HasFlag(flag))
                    {
                        ImGui.Text($"{EnumNameConverter(flag)}");
                    }
                }
            }
        }
        private static void CraftDetails(CosmicHelper.CosmicInfo mission)
        {
            if (mission.Crafts_Main.Count > 0)
            {
                CosmicHelper.CrafterManagement(mission, SelectedMission);
            }
        }
        private static void StatInfo(CosmicHelper.CosmicInfo missionInfo)
        {
            if (C.MissionConfig.TryGetValue(SelectedMission, out var config))
            {
                bool allowDelete = (ImGui.IsKeyDown(ImGuiKey.LeftShift) || ImGui.IsKeyDown(ImGuiKey.RightShift)) && (ImGui.IsKeyDown(ImGuiKey.LeftCtrl) || ImGui.IsKeyDown(ImGuiKey.RightCtrl));

                using (ImRaii.Disabled(!allowDelete))
                {
                    // if (ImGui.Button("Reset Stats"))
                    if (ImGui.Button("重置统计"))
                    {
                        P.MissionTimer.ResetTimers(SelectedMission);
                    }
                }
                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                {
                    ImGui.BeginTooltip();
                    // ImGui.Text("Hold Shift + Control")
                    ImGui.Text("按住 Shift + Control");
                    ImGui.EndTooltip();
                }

                if (config.TurninRecords.Count > 0)
                {
                    // ImGui.Text($"Best Time: {TimeSpan.FromSeconds(config.BestTimeOverall()):mm\\:ss\\.ff}");
                    ImGui.Text($"最佳用时：{TimeSpan.FromSeconds(config.BestTimeOverall()):mm\\:ss\\.ff}");
                    // ImGui.Text($"Average Time: {TimeSpan.FromSeconds(config.AverageTime()):mm\\:ss\\.ff}");
                    ImGui.Text($"平均用时：{TimeSpan.FromSeconds(config.AverageTime()):mm\\:ss\\.ff}");
                }
                else
                {
                    // ImGui.Text("Best Time: --:--:--")
                    ImGui.Text("最佳用时：--:--:--");
                    // ImGui.Text("Average Time: --:--:--")
                    ImGui.Text("平均用时：--:--:--");
                }

                // ImGui.Text($"Times Completed: {config.TotalCompletions}")
                ImGui.Text($"完成次数：{config.TotalCompletions}");
                // ImGui.Text($"Times Attempted: {config.TotalAttempts}")
                ImGui.Text($"尝试次数：{config.TotalAttempts}");

                var baseScore = missionInfo.ClassScore;
                var comsoCredit = missionInfo.CosmoCredit;
                var planetCredit = missionInfo.LunarCredit;

                ImGui.Separator();
                // ImGui.Text("Estimated Score Per Hour:")
                ImGui.Text("预估每小时得分：");
                ImGui.SameLine();
                ImGui.TextDisabled("?");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    // ImGui.Text("This is ASSUMING:")
                    ImGui.Text("以下为假设前提：");
                    // ImGui.Text("1: You have immaculate rng of getting the mission you want every time")
                    ImGui.Text("1：每次都好运刷到你想要的任务");
                    // ImGui.Text("2: You're hitting the threshold every time")
                    ImGui.Text("2：每次都达到分数门槛");
                    // ImGui.Text("This is based on your average time.\n" +
                    //        "So get a good couple of runs to get a good feel for the timing")
                    ImGui.Text("该数值基于你的平均用时。\n" +
                               "多跑几次以获得更准确的估算");
                    ImGui.EndTooltip();
                }
                if (ImGui.BeginTable("Score Info: External Details", 5, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders))
                {
                    foreach (var entry in missionInfo.ScoreInfo().Where(x => x.Value.Score != 0))
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text($"{entry.Key} [{entry.Value.Completions:N0}]");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{entry.Value.Score:N2}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{entry.Value.Cosmocredit:N2}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{entry.Value.PlanetCredits:N2}");

                        ImGui.TableNextColumn();
                        string tokens = entry.Value.Tokens > 0 ? $"{entry.Value.Tokens:N2}" : "-";
                        ImGui.Text(tokens);
                    }

                    ImGui.EndTable();
                }
                if (config.TotalCompletions != 0)
                {
                    if (ImGui.BeginChild("Mission Timers", ImGui.GetContentRegionAvail()))
                    {
                        // Group records by state, preserving enum order
                        var recordsByState = config.TurninRecords
                            .GroupBy(r => r.State)
                            .OrderBy(g => (int)g.Key)
                            .ToList();

                        if (ImGui.BeginTabBar("Completion Stats"))
                        {
                            // "All" tab always shown if there are any records
                            // if (ImGui.BeginTabItem("All"))
                            if (ImGui.BeginTabItem("全部"))
                            {
                                DrawTurninTable(config.TurninRecords);
                                ImGui.EndTabItem();
                            }

                            // One tab per state that has at least one record
                            foreach (var group in recordsByState)
                            {
                                var label = group.Key.ToString();
                                if (ImGui.BeginTabItem(label))
                                {
                                    DrawTurninTable(group.ToList());
                                    ImGui.EndTabItem();
                                }
                            }

                            ImGui.EndTabBar();
                        }
                    }
                    ImGui.EndChild();
                }
            }
        }
        public static string EnumNameConverter(MissionAttributes attribute)
        {
            return attribute switch
            {
                // MissionAttributes.Craft => "Crafting",
                MissionAttributes.Craft => "制作",
                // MissionAttributes.Gather => "Gathering",
                MissionAttributes.Gather => "采集",
                // MissionAttributes.Fish => "Fishing",
                MissionAttributes.Fish => "捕鱼",
                // MissionAttributes.Limited => "Limited Supplies",
                MissionAttributes.Limited => "限量供应",
                // MissionAttributes.Collectables => "Collectable",
                MissionAttributes.Collectables => "收藏品",
                // MissionAttributes.ReducedItems => "Reducable Items",
                MissionAttributes.ReducedItems => "可缩减物品",
                // MissionAttributes.ExpertCraft => "Expert Crafts",
                MissionAttributes.ExpertCraft => "专家制作",
                // MissionAttributes.Score_TimeRemaining => "Timed Scoring",
                MissionAttributes.Score_TimeRemaining => "限时计分",
                // MissionAttributes.Score_Chain => "Chained Gather Scoring",
                MissionAttributes.Score_Chain => "连锁采集计分",
                // MissionAttributes.Score_Boon => "Gatherer's Boons Scoring",
                MissionAttributes.Score_Boon => "采集者恩惠计分",
                // MissionAttributes.Score_LargestSize => "Largest Fish Scored",
                MissionAttributes.Score_LargestSize => "最大鱼获计分",
                // MissionAttributes.Score_Variety => "Variety of Fish Required",
                MissionAttributes.Score_Variety => "需多种鱼类",
                // MissionAttributes.Score_MinimumScore => "Mission Score Required",
                MissionAttributes.Score_MinimumScore => "需达到任务分数",
                // MissionAttributes.Critical => "Critical Mission",
                MissionAttributes.Critical => "紧急任务",
                // MissionAttributes.ProvisionalTimed => "Time Required",
                MissionAttributes.ProvisionalTimed => "需特定时间",
                // MissionAttributes.ProvisionalWeather => "Weather Required",
                MissionAttributes.ProvisionalWeather => "需特定天气",
                // MissionAttributes.ProvisionalSequential => "Sequential Missions Required",
                MissionAttributes.ProvisionalSequential => "需完成前置序列任务",
                _ => attribute.ToString()
            };
        }
        private  static void DrawTurninTable(List<TurninData> records)
        {
            if (!ImGui.BeginTable("TurninTable", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingFixedFit)) 
                return;

            ImGui.TableSetupScrollFreeze(0, 1);
            // ImGui.TableSetupColumn("Time");
            ImGui.TableSetupColumn("用时");
            // ImGui.TableSetupColumn("State", ImGuiTableColumnFlags.WidthStretch, 100f);
            ImGui.TableSetupColumn("状态", ImGuiTableColumnFlags.WidthStretch, 100f);
            ImGui.TableHeadersRow();

            foreach (var record in records)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted($"{TimeSpan.FromSeconds(record.Time):mm\\:ss\\.ff}");
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(record.State.ToString());
            }

            ImGui.EndTable();
        }
    }
}
