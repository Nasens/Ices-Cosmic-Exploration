using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ICE.Ui.MainUi.ModeSelect_Modes;
using ICE.Ui.MainUi.ModeSelect_Modes.CosmicTable;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.GatheringHelper;
using ICE.Utilities.ImGuiTools;
using System;
using System.Collections.Generic;
using System.Text;
using static MissionTimer;

namespace ICE.Ui
{
    internal class Window_ExternalDetails : Window
    {
        public static uint SelectedMission = 0;

        public static List<string> JokeList = new()
        {
            // "What is a pirates favorite letter?\n" +
            // "You might thing it's R, but tis first love was the C\n" +
            // "(It helps if you verbally say it like a pirate)",
            "What is a pirates favorite letter?\n" +
            "海盗最喜欢的字母是什么？\n" +
            "You might thing it's R, but tis first love was the C\n" +
            "你可能以为是 R，但他初恋是 C\n" +
            "(It helps if you verbally say it like a pirate)",
            "（用海盗腔念出来效果更好）",

            // "You know, I was reading this book about anti-gravity recently,\n" +
            // "and honestly I'm having a hard time putting it down",
            "You know, I was reading this book about anti-gravity recently,\n" +
            "最近在读一本关于反重力的书，\n" +
            "and honestly I'm having a hard time putting it down",
            "说实话，我根本放不下来",

            // "Why are tennis pros always hugging each other?\n" +
            // "Because they start their match at \"Love All\"",
            "Why are tennis pros always hugging each other?\n" +
            "为什么网球选手总是互相拥抱？\n" +
            "Because they start their match at \"Love All\"",
            "因为他们比赛开始时是「Love All」（零比零）",

            // "Why can't ghost have babies?\n" +
            // "Because they have hallow-eenies",
            "Why can't ghost have babies?\n" +
            "为什么幽灵不能有宝宝？\n" +
            "Because they have hallow-eenies",
            "因为他们有「万圣空」",

            // "How do you save a drowning pirate?\n" +
            // "You give him Cprrrrrr",
            "How do you save a drowning pirate?\n" +
            "怎么救落水的海盗？\n" +
            "You give him Cprrrrrr",
            "给他做 Cprrrrrr（心肺复苏）",

            // "What is a skeleton's favorite snack?\n" +
            // "Ribs! Spare Ribs!",

            "What is a skeleton's favorite snack?\n" +
            "骷髅最喜欢的零食是什么？\n" +
            "Ribs! Spare Ribs!",
            "肋骨！Spare Ribs（备用肋骨）！",

            // "Honestly, just wanted to say thank you for using my plugin, you're appreciated <3",
            "Honestly, just wanted to say thank you for using my plugin, you're appreciated <3",
            "说真的，只是想感谢你使用我的插件，你很棒 <3",

            // "Knock knock\n" +
            // "[This is where you say who's there]\n" +
            // "Lettuce\n" +
            // "[Lettuce who]\n" +
            // "Lettuce in",
            "Knock knock\n" +
            "咚咚咚\n" +
            "[This is where you say who's there]\n" +
            "[这里你说「谁啊」]\n" +
            "Lettuce\n" +
            "生菜\n" +
            "[Lettuce who]\n" +
            "[生菜谁]\n" +
            "Lettuce in",
            "生菜进来",

            // "What do you a dinosaur that only has one eye?" +
            // "A \"Doyouthinkheseemesaurs\"",
            "What do you a dinosaur that only has one eye?" +
            "只有一只眼睛的恐龙叫什么？" +
            "A \"Doyouthinkheseemesaurs\"",
            "「Do-you-think-he-saw-us-aurus（你觉得他看见我们了吗 龙）」",

            // "So... you're telling me a shrimp fried this rice?",
            "So... you're telling me a shrimp fried this rice?",
            "所以……你是说一只虾炒了这碗饭？",

            // "Thank you everyone who's helped make this possible.\n" +
            // "Strife special shoutout to you for doing what I didn't want to with fishing\n" +
            // "(Sorry for making you start big fish #NotSorry#MuchLove)\n" +
            // "Wah thank you for the UI, this is fucking beautiful as always\n" +
            // "Puni.sh in general for each one of your help my dumb questions"
            "Thank you everyone who's helped make this possible.\n" +
            "感谢所有让这一切成为可能的人。\n" +
            "Strife special shoutout to you for doing what I didn't want to with fishing\n" +
            "特别感谢 Strife 帮我做了我不想碰的捕鱼部分\n" +
            "(Sorry for making you start big fish #NotSorry#MuchLove)\n" +
            "（抱歉让你从大鱼开始 #不抱歉#满满的爱）\n" +
            "Wah thank you for the UI, this is fucking beautiful as always\n" +
            "感谢 Wah 的 UI，一如既往地漂亮\n" +
            "Puni.sh in general for each one of your help my dumb questions\n" +
            "感谢 Puni.sh 的各位回答我的各种问题\n" +
            "翻译-By Composer 2.5 Fast"
        };
        public static int jokeId = 0;

        // public Window_ExternalDetails() : base($"Ice's Cosmic Exploration | Mission Details")
        public Window_ExternalDetails() : base($"Ice's Cosmic Exploration | 任务详情")
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

        public override void Draw()
        {
            DrawMissionDetails();
        }
        public static void DrawMissionDetails()
        {
            if (CosmicHelper.SheetMissionDict.TryGetValue(SelectedMission, out var mission))
            {
                var id = SelectedMission;
                ImGui.PushID($"{mission}_{id}");

                #region Mission Name

                // ImGui.Text($"Mission:");
                ImGui.Text($"任务:");
                ImGui.SameLine(0, 5);
                ImGui.TextDisabled($"[{id}]");
                ImGui.SameLine(0, 5);
                ImGui.Text($"{mission.Name}");

                #endregion

                if (ImGui.BeginTable("Detailed Mission Info", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
                {
                    // ImGui.TableSetupColumn("Name");
                    ImGui.TableSetupColumn("名称");
                    // ImGui.TableSetupColumn("Info");
                    ImGui.TableSetupColumn("信息");

                    // Row 1
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    // ImGui.Text("Cosmocredits");
                    ImGui.Text("宇宙点数");

                    ImGui.TableNextColumn();
                    ImGui.Text($"{mission.CosmoCredit}");

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    // ImGui.Text($"Planetary Credits");
                    ImGui.Text($"行星点数");

                    ImGui.TableNextColumn();
                    ImGui.Text($"{mission.LunarCredit}");

                    if (mission.DronebitReward != 0)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        if (Svc.Texture.TryGetFromGameIcon(65138, out var dronebitIcon))
                        {
                            ImGui.Image(dronebitIcon.GetWrapOrEmpty().Handle, new Vector2(24, 24));
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                ImGui.Image(dronebitIcon.GetWrapOrEmpty().Handle, new Vector2(40, 40));
                                ImGui.EndTooltip();
                            }
                            ImGui.SameLine();
                        }
                        ImGui.AlignTextToFramePadding();
                        // ImGui.Text($"Dronebits");
                        ImGui.Text($"无人机代币");

                        ImGui.TableNextColumn();
                        ImGui.AlignTextToFramePadding();
                        ImGui.Text($"{mission.DronebitReward}");
                    }

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    // ImGui.Text($"Class Score:");
                    ImGui.Text($"职业分数:");

                    ImGui.TableNextColumn();
                    ImGui.Text($"{mission.ClassScore}");

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.AlignTextToFramePadding();
                    // ImGui.Text($"Job(s)");
                    ImGui.Text($"职业");

                    ImGui.TableNextColumn();
                    foreach (var job in mission.Jobs)
                    {
                        ISharedImmediateTexture? icon = CosmicHelper.ClassInfoDict[job].JobIcon;
                        Vector2 size = new Vector2(20, 20);
                        ImGui.Image(icon.GetWrapOrEmpty().Handle, size);
                        ImGui.SameLine();
                    }

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.AlignTextToFramePadding();
                    // ImGui.Text($"Completed:");
                    ImGui.Text($"完成状态:");

                    ImGui.TableNextColumn();
                    ImGui_Ice.CompletionStatusIcon(mission);

                    if (mission.BronzeScore != 0)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        // ImGui.Text($"Bronze Requirement");
                        ImGui.Text($"铜牌要求");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{mission.BronzeScore}");
                    }
                    if (mission.SilverScore != 0)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        // ImGui.Text($"Silver Requirement");
                        ImGui.Text($"银牌要求");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{mission.SilverScore}");
                    }
                    if (mission.GoldScore != 0)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        // ImGui.Text("Gold Requirement");
                        ImGui.Text("金牌要求");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{mission.GoldScore}");
                    }

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

                    ImGui.EndTable();
                }

                if (ImGui.BeginTable("Relic Exp Info Table", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit))
                {
                    // ImGui.TableSetupColumn("Relix Exp Kind");
                    ImGui.TableSetupColumn("Relic 经验类型");
                    // ImGui.TableSetupColumn("Amount");
                    ImGui.TableSetupColumn("数量");

                    ImGui.TableHeadersRow();

                    foreach (var xp in mission.RelicXpInfo.OrderByDescending(x => x.Key))
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        string type = "";
                        switch (xp.Key)
                        {
                            case 1:
                                type = "I";
                                break;
                            case 2:
                                type = "II";
                                break;
                            case 3:
                                type = "III";
                                break;
                            case 4:
                                type = "IV";
                                break;
                            case 5:
                                type = "V";
                                break;
                            case 6:
                                type = "VI";
                                break;
                            case 7:
                                type = "VII";
                                break;
                            default:
                                type = "???";
                                break;
                        }

                        // ImGui.Text($"Lv. {type}");
                        ImGui.Text($"等级 {type}");
                        ImGui.TableNextColumn();
                        ImGui.Text($"{xp.Value}");
                    }

                    ImGui.EndTable();
                }

                if (mission.ExpModifier_3 != 0)
                {
                    if (ImGui.BeginTable("经验奖励", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
                    {
                        // ImGui.TableSetupColumn("Class Exp");
                        ImGui.TableSetupColumn("职业经验");
                        // ImGui.TableSetupColumn("% of Level");
                        ImGui.TableSetupColumn("等级百分比");

                        ImGui.TableHeadersRow();

                        if (mission.ExpModifier_1 != 0)
                        {
                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            // ImGui.Text("Lv. 10-49");
                            ImGui.Text("等级 10-49");

                            ImGui.TableNextColumn();
                            ImGui.Text($"{mission.ExpModifier_1}%");
                        }

                        if (mission.ExpModifier_2 != 0)
                        {
                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            // ImGui.Text("Lv. 50-89");
                            ImGui.Text("等级 50-89");

                            ImGui.TableNextColumn();
                            ImGui.Text($"{mission.ExpModifier_2}%");
                        }

                        if (mission.ExpModifier_3 != 0)
                        {
                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            // ImGui.Text("Lv. 90-99");
                            ImGui.Text("等级 90-99");

                            ImGui.TableNextColumn();
                            ImGui.Text($"{mission.ExpModifier_3}%");
                        }

                        ImGui.EndTable();
                    }
                }

                if (mission.Crafts_Main.Count > 0)
                {
                    ImGui_Ice.WindowSpacer();

                    Mission_Table.CrafterManagement(mission, id);
                }

                ImGui_Ice.WindowSpacer();

                // ImGui.Text("Mission Atributes");
                ImGui.Text("任务属性");
                if (mission.Attributes == MissionAttributes.None)
                {
                    // ImGui.Text("None");
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

                if (CosmicHelper.MissionUnlock.TryGetValue(SelectedMission, out var unlock))
                {
                    // ImGui.Text("The following missions are required to have gold before you can do this one");
                    ImGui.Text("以下任务需先获得金牌才能解锁此任务");
                    foreach (var lockedMission in unlock)
                    {
                        ImGui_Ice.CompletionStatusIcon(CosmicHelper.SheetMissionDict[lockedMission]);
                        ImGui.SameLine();
                        ImGui.Text($"[{lockedMission}] - {CosmicHelper.SheetMissionDict[lockedMission].Name}");
                    }

                }

                ImGui_Ice.WindowSpacer();
                // ImGui.Text($"Mission Times!");
                ImGui.Text($"任务用时！");

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
                        // ImGui.Text("Hold Shift + Control");
                        ImGui.Text("按住 Shift + Control");
                        ImGui.EndTooltip();
                    }

                    if (config.TurninRecords.Count > 0)
                    {
                        // ImGui.Text($"Best Time: {TimeSpan.FromSeconds(config.BestTime):mm\\:ss\\.ff}");
                        ImGui.Text($"最佳用时: {TimeSpan.FromSeconds(config.BestTime):mm\\:ss\\.ff}");
                        // ImGui.Text($"Average Time: {TimeSpan.FromSeconds(config.AverageTime):mm\\:ss\\.ff}");
                        ImGui.Text($"平均用时: {TimeSpan.FromSeconds(config.AverageTime):mm\\:ss\\.ff}");
                    }
                    else
                    {
                        // ImGui.Text("Best Time: --:--:--");
                        ImGui.Text("最佳用时: --:--:--");
                        // ImGui.Text("Average Time: --:--:--");
                        ImGui.Text("平均用时: --:--:--");
                    }

                    // ImGui.Text($"Times Completed: {config.TotalCompletions}");
                    ImGui.Text($"完成次数: {config.TotalCompletions}");
                    // ImGui.Text($"Times Attempted: {config.TotalAttempts}");
                    ImGui.Text($"尝试次数: {config.TotalAttempts}");

                    if (CosmicHelper.SheetMissionDict.TryGetValue(SelectedMission, out var missionInfo))
                    {
                        var baseScore = missionInfo.ClassScore;
                        var comsoCredit = missionInfo.CosmoCredit;
                        var planetCredit = missionInfo.LunarCredit;

                        ImGui.Separator();
                        // ImGui.Text("Estimated Score Per Hour:");
                        ImGui.Text("预估每小时得分:");
                        ImGui.SameLine();
                        ImGui.TextDisabled("?");
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();
                            // ImGui.Text("This is ASSUMING:");
                            ImGui.Text("以下假设前提：");
                            // ImGui.Text("1: You have immaculate rng of getting the mission you want every time");
                            ImGui.Text("1：每次都能完美随机到你想要的任务");
                            // ImGui.Text("2: You're hitting the threshold every time");
                            ImGui.Text("2：每次都能达到分数阈值");
                            // ImGui.Text("This is based on your average time.\n" +
                            //            "So get a good couple of runs to get a good feel for the timing");
                            ImGui.Text("基于你的平均用时计算。\n" +
                                       "建议多跑几次以获得更准确的数据");
                            ImGui.EndTooltip();
                        }
                        if (ImGui.BeginTable("Score Info: External Details", 5, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders))
                        {
                            foreach (var entry in missionInfo.ScoreInfo().Where(x => x.Value.Score != 0))
                            {
                                ImGui.TableNextRow();
                                ImGui.TableSetColumnIndex(0);
                                // ImGui.Text($"{entry.Key} [{entry.Value.Completions:N0}]");
                                ImGui.Text($"{CosmicHelper.TurninStateDisplayName(entry.Key)} [{entry.Value.Completions:N0}]");

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
                    }
                }

                ImGui.PopID();
            }
            else
            {
                string joke = JokeList[jokeId];
                ImGui.TextWrapped(joke);
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
    }
}
