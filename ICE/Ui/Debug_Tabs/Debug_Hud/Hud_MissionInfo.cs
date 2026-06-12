using FFXIVClientStructs.FFXIV.Client.Game.WKS;
using ICE.Utilities.Cosmic_Helper;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;

namespace ICE.Ui.Debug_Tabs.Debug_Hud
{
    internal class Hud_MissionInfo
    {
        public static unsafe void Draw()
        {
            if (GenericHelpers.TryGetAddonMaster<WKSMissionInfomation>("WKSMissionInfomation", out var x) && x.IsAddonReady)
            {
                var isAddonReady = AddonHelper.IsAddonActive("WKSMissionInfomation");
                // ImGui.Text($"Addon Ready: {isAddonReady}");
                ImGui.Text($"界面就绪: {isAddonReady}");
                if (isAddonReady)
                {
                    // ImGui.Text($"Node Text: {AddonHelper.GetNodeText("WKSMissionInfomation", 27)}");
                    ImGui.Text($"节点文本: {AddonHelper.GetNodeText("WKSMissionInfomation", 27)}");
                }

                ImGuiTableFlags tableFlags = ImGuiTableFlags.RowBg |
                                             ImGuiTableFlags.Borders |
                                             ImGuiTableFlags.SizingFixedFit |
                                             ImGuiTableFlags.Resizable |           // Allow column resizing
                                             ImGuiTableFlags.Reorderable |         // Allow column reordering
                                             ImGuiTableFlags.Hideable;             // Allow hiding columns via right-click

                if (ImGui.BeginTable("WKSMissionInfomationAddon_Table", 2, tableFlags))
                {
                    ImGui.TableSetupColumn("###Info", ImGuiTableColumnFlags.WidthFixed, 150);
                    ImGui.TableSetupColumn("###UiInfo", ImGuiTableColumnFlags.WidthFixed, 100);

                    var missionId = CosmicHelper.CurrentLunarMission;
                    

                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    // ImGui.Text("Current Mission:");
                    ImGui.Text("当前任务:");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{missionId}");

                    if (CosmicHelper.SheetMissionDict.TryGetValue(missionId, out var mission))
                    {
                        ImGui.TableNextColumn();
                        ImGui.TableSetColumnIndex(0);
                        // ImGui.Text("Current Score:");
                        ImGui.Text("当前分数:");
                        ImGui.TableNextColumn();

                        ImGui.Text($"{CosmicHandler.GetScore()}");

                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        // ImGui.Text($"Current State");
                        ImGui.Text($"当前状态");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{Task_CheckScore.CurrentRank()}");


                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        // ImGui.Text("Is Mission Timed out");
                        ImGui.Text("任务是否超时");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{CosmicHandler.IsMissionTimedOut()}");
                    }
                    else if (mission.Attributes.HasFlag(MissionAttributes.Critical))
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        // ImGui.Text("Critical Value:");
                        ImGui.Text("关键值:");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{x.CriticalScore}");
                    }
                    
                    if (mission.Attributes.HasFlag(MissionAttributes.Fish))
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        // ImGui.Text("Current Bait");
                        ImGui.Text("当前鱼饵");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{CosmicHelper.CurrentBait()
                            }");
                    }
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    // ImGui.Text("Collected Individual");
                    ImGui.Text("单项收集");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{CosmicHelper.CurrentIndividual()}");

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    // ImGui.Text($"Collected Total");
                    ImGui.Text($"总计收集");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{CosmicHelper.CurrentTotal()}");
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    // if (ImGui.Button("Cosmo Pouch"))
                    if (ImGui.Button("Cosmo Pouch"))
                    {
                        x.CosmoPouch();
                    }

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    // if (ImGui.Button("Cosmo Crafting Log"))
                    if (ImGui.Button("Cosmo Crafting Log"))
                    {
                        x.CosmoCraftingLog();
                    }

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    // if (ImGui.Button("Steller Reduction"))
                    if (ImGui.Button("Steller Reduction"))
                    {
                        x.StellerReduction();
                    }

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    // if (ImGui.Button("Report"))
                    if (ImGui.Button("提交"))
                    {
                        x.Report();
                    }

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    // if (ImGui.Button("Abandon"))
                    if (ImGui.Button("放弃"))
                    {
                        x.Abandon();
                    }

                    var wks = WKSManager.Instance();
                    if (wks == null)
                        return;

                    var scores = wks->State.Scores;

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    // ImGui.Text("Score 1");
                    ImGui.Text("分数 1");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{scores.Length}");

                    for (int score = 0; score < scores.Length; score++)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        // ImGui.Text($"Score: [{score}]");
                        ImGui.Text($"分数: [{score}]");
                        ImGui.TableNextColumn();
                        ImGui.Text($"{scores[score]}");
                    }

                    /*
                    var currentlyEquippped = wks->FishingBait | 0;

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text("Bait:");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{currentlyEquippped}");
                    */


                    ImGui.EndTable();
                }
            }
            else
            {
                // ImGui.Text("Waiting for \"WKSMissionInfomation\" to be visible");
                ImGui.Text("等待 \"WKSMissionInfomation\" 界面可见");
            }
        }
    }
}
