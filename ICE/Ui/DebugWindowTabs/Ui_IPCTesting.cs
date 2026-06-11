using ECommons.ExcelServices.TerritoryEnumeration;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ICE.Utilities.Cosmic_Helper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Ui_IPCTesting
    {
        private static int Radius = 10;
        private static int XLoc = 0;
        private static int YLoc = 0;
        private static string PandoraFeature = "";
        private static int amount = 1000;

        private static string importString = new string('\0', 2048); // Pre-allocate buffer
        private static string SwapToPreset = string.Empty;
        private static uint missionId = 0;
        private static uint baitId = 0;
        private static bool baitSwapped = false;
        private static uint MMSAmount = 20;
        private static uint MMMaxUse = 1;
        private static bool tempMM = true;


        private static string SettingChange = "";
        private static bool SettingState = false;

        public static unsafe void Draw()
        {
            ImGui.Text($"Artisan 是否繁忙？{P.Artisan.IsBusy()}");
            ImGui.Text($"{EzThrottler.GetRemainingTime("[Main Item(s)] Starting Main Craft")}");
            if (ImGui.Button("让 Artisan 制作此物"))
            {
                P.Artisan.CraftItem(36026, 1);
            }

            ImGui.SetNextItemWidth(125);
            ImGui.InputInt("半径", ref Radius);
            ImGui.SetNextItemWidth(125);
            ImGui.InputInt("X 坐标", ref XLoc);
            ImGui.SetNextItemWidth(125);
            ImGui.InputInt("Y 坐标", ref YLoc);

            if (ImGui.Button($"测试半径"))
            {
                var agent = AgentMap.Instance();

                Utils.SetGatheringRing(agent->CurrentTerritoryId, XLoc, YLoc, Radius);
            }

            ImGui.Separator();
            ImGui.InputText("Pandora 功能", ref PandoraFeature);
            if (ImGui.Button("暂停功能"))
            {
                P.Pandora.PauseFeature(PandoraFeature, amount);
            }

            ImGui.Separator();
            ImGui.Text("AutoHook");
            ImGui.SetNextItemWidth(150);
            ImGui.InputText("预设字符串", ref importString, 2048);
            if (ImGui.Button("导入"))
            {
                P.AutoHook.ImportAndSelectPreset(importString);
                importString = string.Empty;
            }
            ImGui.SetNextItemWidth(150);
            ImGui.InputText("切换到预设", ref SwapToPreset);
            if (ImGui.Button("切换"))
            {
                P.AutoHook.SetPreset(SwapToPreset);
            }
            if (ImGui.Button("应用临时"))
            {
                P.AutoHook.CreateAndSelectAnonymousPreset(importString);
            }
            ImGui.SetNextItemWidth(200);
            ImGui.InputUInt("选择要导入的任务", ref missionId);
            ImGui.InputUInt("鱼饵 ID", ref baitId);
            if (ImGui.Button("切换鱼饵"))
            {
                 SwapBait(baitId);
            }
            if (ImGui.Button("切换鱼饵……简易"))
            {
                if (CosmicHelper.CurrentBait() == 0)
                {
                    IceLogging.Debug("Bait is not currently equipped");
                }

                P.AutoHook.SwapBaitById(baitId);
            }
            if (ImGui.Button("愚蠢测试"))
            {
                if (CosmicHelper.CurrentBait() == 0)
                {
                    IceLogging.Debug($"No bait is equipped");
                }
                else if (CosmicHelper.CurrentBait == null)
                {
                    IceLogging.Debug("Bait is null... aka not in the middle of a mission");
                }
                else
                {
                    IceLogging.Debug($"Current bait: {CosmicHelper.CurrentBait}");
                }
            }

            if (ImGui.Button("启用 AutoHook"))
            {
                P.AutoHook.SetPluginState(true);
            }
            if (ImGui.Button("禁用 AutoHook"))
            {
                P.AutoHook.SetPluginState(false);
            }

            ImGui.Separator();
            ImGui.Text($"ICE 是否运行中？| {P.IceIpc.IsRunning()}");
            if (ImGui.Button("仅通过 IPC 的任务"))
            {
                HashSet<uint> missionListIds = new() { 1, 3, 4, 7, 9, 11 };
                P.IceIpc.OnlyMissions(missionListIds);
            }
            if (ImGui.Button("切换到赌博"))
            {
                SchedulerMain.State = IceState.Gambling;
            }

            ImGui.Separator();

            ImGui.SetNextItemWidth(150);
            ImGui.InputText("设置名称", ref SettingChange);
            ImGui.Checkbox("设置布尔值", ref SettingState);

            if (ImGui.Button("切换设置"))
            {
                P.IceIpc.ChangeSetting(SettingChange, SettingState);
            }
            if (ImGui.Button("设置临时设置"))
            {
                P.Artisan.ChangeSolver(37084, "Progress Only Solver", true);
            }
            if (ImGui.Button("设置 Raphael 求解器"))
            {
                P.Artisan.ChangeSolver(37084, "Raphael Recipe Solver", true);
            }
            if (ImGui.Button("将当前任务设为 Raphael"))
            {
                if (CosmicHelper.CurrentLunarMission != 0)
                {
                    foreach (var craftItem in CosmicHelper.CurrentMissionInfo.Crafts_Main)
                    {
                        P.Artisan.ChangeSolver(craftItem.Value.RecipeId, "Raphael Recipe Solver", true);
                    }
                    foreach (var preCraft in CosmicHelper.CurrentMissionInfo.Crafts_Pre)
                    {
                        P.Artisan.ChangeSolver(preCraft.Value.RecipeId, "Raphael Recipe Solver", true);
                    }
                }
            }
            if (ImGui.Button("将当前任务设为 Progress"))
            {
                if (CosmicHelper.CurrentLunarMission != 0)
                {
                    foreach (var craftItem in CosmicHelper.CurrentMissionInfo.Crafts_Main)
                    {
                        P.Artisan.ChangeSolver(craftItem.Value.RecipeId, "Progress Only Solver", true);
                    }
                    foreach (var preCraft in CosmicHelper.CurrentMissionInfo.Crafts_Pre)
                    {
                        P.Artisan.ChangeSolver(preCraft.Value.RecipeId, "Progress Only Solver", true);
                    }
                }
            }
            ImGui.DragUInt("MM 步数使用", ref MMSAmount, 1, 0, 20);
            ImGui.DragUInt("MM 配方使用次数", ref MMMaxUse, 1, 0, 3);
            ImGui.Checkbox("临时设置 MM", ref tempMM);
            if (ImGui.Button("设置 Miracle 求解器"))
            {
                P.Artisan.ChangeStandardMinimumStepsBeforeMiracle(MMSAmount, tempMM);
                P.Artisan.ChangeStandardMaxMaterialMiracleUses(MMMaxUse, tempMM);
            }
            ImGui.SameLine();
            if (ImGui.Button("恢复临时 MM"))
            {
                P.Artisan.SetTempStandardMinimumStepsBeforeMiracleBackToNormal();
                P.Artisan.SetTempStandardMaxMaterialMiracleUsesBackToNormal();
            }

            if (ImGui.Button("恢复正常"))
            {
                if (CosmicHelper.CurrentLunarMission != 0)
                {
                    foreach (var craftItem in CosmicHelper.CurrentMissionInfo.Crafts_Main)
                    {
                        P.Artisan.SetTempSolverBackToNormal(craftItem.Value.RecipeId);
                    }
                    foreach (var preCraft in CosmicHelper.CurrentMissionInfo.Crafts_Pre)
                    {
                        P.Artisan.SetTempSolverBackToNormal(preCraft.Value.RecipeId);
                    }
                }
            }
            if (ImGui.Button("禁用 Endurance"))
            {
                P.Artisan.SetEnduranceStatus(false);
            }
            if (ImGui.Button("测试提示"))
            {
                string message = "[I.C.E.] 你没有阅读任务设置中的小提示\n" +
                    "你需要更新 AutoHook 才能在 Auxesia 钓鱼。请切换到测试版本";
                Svc.Chat.Print(new()
                {
                    Type = Dalamud.Game.Text.XivChatType.ErrorMessage,
                    Message = message,
                });
                Svc.Toasts.ShowNormal($"{message}");
            }
        }

        private static void SwapBait(uint baitId)
        {
            _ = Task.Run(async () =>
            {
                baitSwapped = await TaskSwapBait(baitId);
            });

            _ = Task.Run(async () =>
            {
                await P.AutoHook.SwapBaitById(baitId);
            });
        }

        private static async Task<bool> TaskSwapBait(uint bait)
        {
            return await P.AutoHook.SwapBaitById(bait);
        }
    }
}
