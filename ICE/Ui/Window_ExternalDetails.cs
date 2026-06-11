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
            "海盗最喜欢的字母是哪个？\n" +
            "你可能以为是 R，但它的初恋其实是 C（读作 sea，海）\n" +
            "（用海盗的腔调读出来会更有感觉）",

            "你知道吗，我最近在读一本关于反重力的书，\n" +
            "说实话，我都没法把它放下（反重力嘛）",

            "为什么网球职业选手总是互相拥抱？\n" +
            "因为他们的比赛从 \"Love All\"（0:0，也意为'爱所有人'）开始",

            "为什么鬼魂不能生孩子？\n" +
            "因为他们有的是 hallow-eenies（万圣节 Halloween 的谐音梗）",

            "怎么救一个溺水的海盗？\n" +
            "给他做 Cprrrrrr（CPR 加上海盗的'啊rrr'）",

            "骷髅最爱的零食是什么？\n" +
            "肋排！多出来的肋骨！",

            "说真的，只想谢谢你使用我的插件，非常感谢你 <3",

            "咚咚咚（敲门）\n" +
            "[这里该你问'谁啊？']\n" +
            "生菜（Lettuce）\n" +
            "[什么生菜？]\n" +
            "Lettuce in（Lettuce = let us，让我们进去吧）",

            "只有一只眼睛的恐龙叫什么？" +
            "叫 \"Doyouthinkheseemesaurs\"（Do-you-think-he-sees-us 的谐音）",

            "所以…你是说这碗饭是一只虾炒的？（虾炒饭的梗）",

            "感谢每一位帮助让这一切成为可能的人。\n" +
            "特别要向 Strife 致谢，谢谢你做了我不想碰的钓鱼部分\n" +
            "（抱歉让你从 big fish 开始搞 #NotSorry#MuchLove）\n" +
            "Wah 谢谢你做的 UI，一如既往地美得不行\n" +
            "还有 Puni.sh，感谢你们解答我每一个蠢问题"
        };
        public static int jokeId = 0;

        public Window_ExternalDetails() : base($"Ice's Cosmic Exploration | Mission Details")
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

                ImGui.Text($"任务:");
                ImGui.SameLine(0, 5);
                ImGui.TextDisabled($"[{id}]");
                ImGui.SameLine(0, 5);
                ImGui.Text($"{mission.Name}");

                #endregion

                if (ImGui.BeginTable("Detailed Mission Info", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
                {
                    ImGui.TableSetupColumn("Name");
                    ImGui.TableSetupColumn("Info");

                    // Row 1
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text("Cosmocredits");

                    ImGui.TableNextColumn();
                    ImGui.Text($"{mission.CosmoCredit}");

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text($"Planetary Credits");

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
                        ImGui.Text($"Dronebits");

                        ImGui.TableNextColumn();
                        ImGui.AlignTextToFramePadding();
                        ImGui.Text($"{mission.DronebitReward}");
                    }

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text($"职业分数:");

                    ImGui.TableNextColumn();
                    ImGui.Text($"{mission.ClassScore}");

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.AlignTextToFramePadding();
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
                    ImGui.Text($"已完成:");

                    ImGui.TableNextColumn();
                    ImGui_Ice.CompletionStatusIcon(mission);

                    if (mission.BronzeScore != 0)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text($"铜牌要求");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{mission.BronzeScore}");
                    }
                    if (mission.SilverScore != 0)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text($"银牌要求");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{mission.SilverScore}");
                    }
                    if (mission.GoldScore != 0)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text("金牌要求");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{mission.GoldScore}");
                    }

                    if (mission.MarkerId != 0)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
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
                        ImGui.Text("Critical 区域");

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
                    ImGui.TableSetupColumn("Relic 经验类型");
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

                        ImGui.Text($"Lv. {type}");
                        ImGui.TableNextColumn();
                        ImGui.Text($"{xp.Value}");
                    }

                    ImGui.EndTable();
                }

                if (mission.ExpModifier_3 != 0)
                {
                    if (ImGui.BeginTable("Exp Rewards", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
                    {
                        ImGui.TableSetupColumn("职业经验");
                        ImGui.TableSetupColumn("等级百分比");

                        ImGui.TableHeadersRow();

                        if (mission.ExpModifier_1 != 0)
                        {
                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            ImGui.Text("Lv. 10-49");

                            ImGui.TableNextColumn();
                            ImGui.Text($"{mission.ExpModifier_1}%");
                        }

                        if (mission.ExpModifier_2 != 0)
                        {
                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            ImGui.Text("Lv. 50-89");

                            ImGui.TableNextColumn();
                            ImGui.Text($"{mission.ExpModifier_2}%");
                        }

                        if (mission.ExpModifier_3 != 0)
                        {
                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            ImGui.Text("Lv. 90-99");

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

                ImGui.Text("任务属性");
                if (mission.Attributes == MissionAttributes.None)
                {
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
                    ImGui.Text("需要先完成以下任务并取得金牌，才能进行此任务");
                    foreach (var lockedMission in unlock)
                    {
                        ImGui_Ice.CompletionStatusIcon(CosmicHelper.SheetMissionDict[lockedMission]);
                        ImGui.SameLine();
                        ImGui.Text($"[{lockedMission}] - {CosmicHelper.SheetMissionDict[lockedMission].Name}");
                    }

                }

                ImGui_Ice.WindowSpacer();
                ImGui.Text($"任务时间！");

                if (C.MissionConfig.TryGetValue(SelectedMission, out var config))
                {
                    bool allowDelete = (ImGui.IsKeyDown(ImGuiKey.LeftShift) || ImGui.IsKeyDown(ImGuiKey.RightShift)) && (ImGui.IsKeyDown(ImGuiKey.LeftCtrl) || ImGui.IsKeyDown(ImGuiKey.RightCtrl));

                    using (ImRaii.Disabled(!allowDelete))
                    {
                        if (ImGui.Button("重置统计"))
                        {
                            P.MissionTimer.ResetTimers(SelectedMission);
                        }
                    }
                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text("按住 Shift + Control");
                        ImGui.EndTooltip();
                    }

                    if (config.TurninRecords.Count > 0)
                    {
                        ImGui.Text($"最佳时间: {TimeSpan.FromSeconds(config.BestTime):mm\\:ss\\.ff}");
                        ImGui.Text($"平均时间: {TimeSpan.FromSeconds(config.AverageTime):mm\\:ss\\.ff}");
                    }
                    else
                    {
                        ImGui.Text("最佳时间: --:--:--");
                        ImGui.Text("平均时间: --:--:--");
                    }

                    ImGui.Text($"完成次数: {config.TotalCompletions}");
                    ImGui.Text($"尝试次数: {config.TotalAttempts}");

                    if (CosmicHelper.SheetMissionDict.TryGetValue(SelectedMission, out var missionInfo))
                    {
                        var baseScore = missionInfo.ClassScore;
                        var comsoCredit = missionInfo.CosmoCredit;
                        var planetCredit = missionInfo.LunarCredit;

                        ImGui.Separator();
                        ImGui.Text("预计每小时分数:");
                        ImGui.SameLine();
                        ImGui.TextDisabled("?");
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();
                            ImGui.Text("这是在假设:");
                            ImGui.Text("1: 你每次都能完美抽到你想要的任务");
                            ImGui.Text("2: 你每次都能达到阈值");
                            ImGui.Text("这是基于你的平均时间计算的。\n" +
                                       "所以多跑几次以更好地把握节奏");
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
                MissionAttributes.Craft => "制作",
                MissionAttributes.Gather => "采集",
                MissionAttributes.Fish => "钓鱼",
                MissionAttributes.Limited => "限量供应",
                MissionAttributes.Collectables => "收藏品",
                MissionAttributes.ReducedItems => "可精选物品",
                MissionAttributes.ExpertCraft => "专家制作",
                MissionAttributes.Score_TimeRemaining => "限时计分",
                MissionAttributes.Score_Chain => "连锁采集计分",
                MissionAttributes.Score_Boon => "采集者恩惠计分",
                MissionAttributes.Score_LargestSize => "最大鱼计分",
                MissionAttributes.Score_Variety => "需要多种鱼类",
                MissionAttributes.Score_MinimumScore => "需要达到任务分数",
                MissionAttributes.Critical => "Critical 任务",
                MissionAttributes.ProvisionalTimed => "需要特定时间",
                MissionAttributes.ProvisionalWeather => "需要特定天气",
                MissionAttributes.ProvisionalSequential => "需要序列任务",
                _ => attribute.ToString()
            };
        }
    }
}
