using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.WKS;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ICE.Utilities.Cosmic_Helper;
using System.Collections.Generic;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;

namespace ICE.Ui.Debug_Tabs.Debug_Hud
{
    internal class Hud_Mission
    {
        private static int BestMission = 0;
        private static string MissionName = "";

        public static List<int> XpKinds = new() { 1, 2, 3, 4, 5, 6, 7 };

        public unsafe static void Draw()
        {
            if (GenericHelpers.TryGetAddonMaster<WKSMission>("WKSMission", out var x) && x.IsAddonReady)
            {
                // ImGui.Text("List of Visible Missions");
                ImGui.Text("可见任务列表");
                // ImGui.Text($"Selected Mission Name: {x.SelectedMissionName}");
                ImGui.Text($"所选任务名称: {x.SelectedMissionName}");
                // ImGui.Text($"Selected Mission ID: {x.SelectedMissionId}");
                ImGui.Text($"所选任务 ID: {x.SelectedMissionId}");
                ImGui.Text($"{AgentWKSMissionEx.selectedTab()}");

                // if (ImGui.Button("Help"))
                if (ImGui.Button("帮助"))
                {
                    x.Help();
                }
                ImGui.SameLine();

                // if (ImGui.Button("Mission Selection"))
                if (ImGui.Button("任务选择"))
                {
                    x.MissionSelection();
                }
                ImGui.SameLine();

                // if (ImGui.Button("Mission Log"))
                if (ImGui.Button("任务日志"))
                {
                    x.MissionLog();
                }
                ImGui.SameLine();

                // if (ImGui.Button("Basic Missions"))
                if (ImGui.Button("基础任务"))
                {
                    x.BasicMissions();
                }
                ImGui.SameLine();

                // if (ImGui.Button("Provisional Missions"))
                if (ImGui.Button("临时任务"))
                {
                    x.ProvisionalMissions();
                }
                ImGui.SameLine();

                // if (ImGui.Button("Critical Missions"))
                if (ImGui.Button("关键任务"))
                {
                    x.CriticalMissions();
                }

                // if (ImGui.Button("Test Mission List"))
                if (ImGui.Button("测试任务列表"))
                {
                    Mission_Settings.SelectedJob = (uint)Player.Job;
                    Mission_Settings.Mode = C.SelectedMode;
                    Task_CheckMissions.RefreshMissionLibrary();
                }

                bool EnableDummyXp = C.UseDummyXp;
                // if (ImGui.Checkbox("Enable Dummy XP", ref EnableDummyXp))
                if (ImGui.Checkbox("启用虚拟经验", ref EnableDummyXp))
                {
                    C.UseDummyXp = EnableDummyXp;
                    C.Save();
                }

                for (int i = 8; i < 19; i++)
                {
                    if (i != 8)
                        ImGui.SameLine();

                    if (ImGui.Button($"[{i}]"))
                    {
                        var agent = AgentWKSMission.Instance();
                        if (agent == null) return;

                        // IceLogging.Debug($"Before: SelectedTab={agent->SelectedTab}, SelectedJobIndex={agent->Data->SelectedJobIndex}");
                        IceLogging.Debug($"处理前：SelectedTab={agent->SelectedTab}, SelectedJobIndex={agent->Data->SelectedJobIndex}");
                        AgentWKSMissionEx.SetSelectedJobTab(agent, (byte)i);
                        // IceLogging.Debug($"After: SelectedTab={agent->SelectedTab}, SelectedJobIndex={agent->Data->SelectedJobIndex}");
                        IceLogging.Debug($"处理后：SelectedTab={agent->SelectedTab}, SelectedJobIndex={agent->Data->SelectedJobIndex}");
                    }
                }

                bool IgnoreManual = C.XPRelicIgnoreManual;
                bool onlyEnabled = C.XPRelicOnlyEnabled;
                // if (ImGui.Checkbox("Ignore Manual Mode", ref IgnoreManual))
                if (ImGui.Checkbox("忽略手动模式", ref IgnoreManual))
                {
                    C.XPRelicIgnoreManual = IgnoreManual;
                    C.Save();
                }
                // if (ImGui.Checkbox("Only Enabled Missions", ref onlyEnabled))
                if (ImGui.Checkbox("仅已启用任务", ref onlyEnabled))
                {
                    C.XPRelicOnlyEnabled = onlyEnabled;
                    C.Save();
                }

                // if (ImGui.Button("Update Dummy XP"))
                if (ImGui.Button("更新虚拟经验"))
                {
                    foreach (var kind in XpKinds)
                    {
                        if (!C.DummyXP.ContainsKey(kind))
                        {
                            C.DummyXP[kind] = new()
                            {
                                CurrentXP = 0,
                                NeededXP = 1,
                            };
                            C.Save();
                        }
                    }
                }

                foreach (var key in C.DummyXP.Keys.ToList())
                {
                    var xp = C.DummyXP[key];

                    ImGui.Text($"XP: {key}");
                    ImGui.PushID(key);

                    int currentXP = xp.CurrentXP;
                    int neededXP = xp.NeededXP;

                    ImGui.SetNextItemWidth(100);
                    // if (ImGui.InputInt("Current XP", ref currentXP))
                    if (ImGui.InputInt("当前经验", ref currentXP))
                    {
                        xp.CurrentXP = currentXP;
                        C.Save();
                    }

                    ImGui.SetNextItemWidth(100);
                    // if (ImGui.InputInt("Needed XP", ref neededXP))
                    if (ImGui.InputInt("所需经验", ref neededXP))
                    {
                        xp.NeededXP = neededXP;
                        C.Save();
                    }

                    ImGui.PopID();

                    ImGui.Separator();
                }

                foreach (var m in x.StellerMissions)
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text($"[{m.MissionId}] {m.Name}");
                    ImGui.SameLine();
                    // if (ImGui.Button($"Select###Select + {m.Name}"))
                    if (ImGui.Button($"选择###Select + {m.Name}"))
                    {
                        m.Select();
                    }
                    ImGui.SameLine();
                    // if (ImGui.Button($"Initiate##Initiate + {m.Name}"))
                    if (ImGui.Button($"接取##Initiate + {m.Name}"))
                    {
                        m.Initiate();
                    }
                }

                // ImGui.Text($"Best Relic Mission: {BestMission} | {MissionName}");
                ImGui.Text($"最佳遗物任务: {BestMission} | {MissionName}");
                // if (ImGui.Button("Update Best Mission"))
                if (ImGui.Button("更新最佳任务"))
                {
                    BestMission = (int)RelicMissionFinder();
                    if (BestMission < 1)
                        MissionName = "None";
                    else
                    {
                        MissionName = CosmicHelper.SheetMissionDict[(uint)BestMission].Name;
                    }
                }


            }
            else
            {
                // ImGui.Text("Waiting for \"WKSMission\" to be visible");
                ImGui.Text("等待 \"WKSMission\" 界面可见");
            }
        }

        private static unsafe uint RelicMissionFinder()
        {
            if (GenericHelpers.TryGetAddonMaster<WKSMission>("WKSMission", out var missionInfo) && missionInfo.IsAddonReady)
            {
                var job = Mission_Settings.SelectedJob;
                var relicInfo = CosmicHelper.Cosmic_ClassInfo();
                var classInfo = relicInfo[job];

                var urgency = new Dictionary<int, float>();
                if (C.UseDummyXp)
                {
                    foreach (var exp in C.DummyXP)
                    {
                        urgency[exp.Key] = 1f - exp.Value.CurrentXP / exp.Value.NeededXP;
                    }
                }
                else
                {
                    foreach (var exp in classInfo.CurrentExp)
                    {
                        if (classInfo.Stage_Current != classInfo.Stage_Next)
                            urgency[exp.Key] = exp.Value.Needed > 0 ? 1f - (float)exp.Value.Current / exp.Value.Needed : 0f;
                        else
                            urgency[exp.Key] = 1f - (float)exp.Value.Current / exp.Value.Max;
                    }
                }
                // IceLogging.Verbose($"Urgency Exp Values");
                IceLogging.Verbose($"紧急度经验值");
                foreach (var exp in urgency)
                {
                    // IceLogging.Verbose($"{exp.Key} : Value: {exp.Value:N2}");
                    IceLogging.Verbose($"{exp.Key} : 值：{exp.Value:N2}");
                }

                List<uint> missionList = new();
                foreach (var mission in C.MissionConfig)
                {
                    if (CosmicHelper.SheetMissionDict.TryGetValue(mission.Key, out var sheetInfo))
                    {
                        if (sheetInfo.TerritoryId != Player.Territory.RowId)
                            continue;

                        if (!sheetInfo.Jobs.Contains((uint)Player.Job))
                            continue;

                        if (sheetInfo.IsProvisional)
                            continue;

                        missionList.Add(mission.Key);
                    }
                }

                // IceLogging.Verbose($"Total Mission Count: {missionList.Count()}");
                IceLogging.Verbose($"任务总数：{missionList.Count()}");


                uint? bestMissionId = null;
                float bestScore = float.NegativeInfinity;
                foreach (var missionId in missionList)
                {
                    // IceLogging.Verbose($"Seeing if menu contains: {missionId}");
                    IceLogging.Verbose($"正在检查菜单是否包含：{missionId}");
                    var mission = missionInfo.StellerMissions.Where(x => x.MissionId == missionId).FirstOrDefault();
                    if (mission != null)
                    {
                        // IceLogging.Verbose($"Mission was valid option: {missionId}");
                        IceLogging.Verbose($"任务为有效选项：{missionId}");
                        if (CosmicHelper.SheetMissionDict.TryGetValue(mission.MissionId, out var sheetInfo))
                        {
                            float score = 0;
                            foreach (var reward in sheetInfo.RelicXpInfo)
                            {
                                if (urgency.TryGetValue(reward.Key, out var info))
                                {
                                    float contribution = info * reward.Value;
                                    if (contribution > 0)
                                    {
                                        score += contribution;
                                    }
                                }
                            }
                            // IceLogging.Verbose($"[{mission.MissionId}] score: {score}");
                            IceLogging.Verbose($"[{mission.MissionId}] 分数：{score}");
                            if (score > bestScore)
                            {
                                bestScore = score;
                                bestMissionId = missionId;
                            }
                        }
                    }
                }

                if (bestMissionId != null)
                {
                    return bestMissionId.Value;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                // IceLogging.Info("We're somehow not showing the window, so returning 0.");
                IceLogging.Info("由于某种原因未显示窗口，因此返回 0。");

                return 0;
            }
        }
    }
}
