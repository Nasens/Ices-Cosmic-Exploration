using ECommons.ExcelServices.TerritoryEnumeration;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ICE.Utilities.Cosmic_Helper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ICE.Ui.Debug_Tabs.Debug_Ui
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
            // ImGui.Text($"Artisan Is Busy? {P.Artisan.IsBusy()}");
            ImGui.Text($"Artisan 是否忙碌? {P.Artisan.IsBusy()}");
            ImGui.Text($"{EzThrottler.GetRemainingTime("[Main Item(s)] Starting Main Craft")}");
            // if (ImGui.Button("Artisan, craft this"))
            if (ImGui.Button("Artisan，制作此物品"))
            {
                P.Artisan.CraftItem(36026, 1);
            }

            ImGui.SetNextItemWidth(125);
            // ImGui.InputInt("Radius", ref Radius);
            ImGui.InputInt("半径", ref Radius);
            ImGui.SetNextItemWidth(125);
            // ImGui.InputInt("X Location", ref XLoc);
            ImGui.InputInt("X 坐标", ref XLoc);
            ImGui.SetNextItemWidth(125);
            // ImGui.InputInt("Y Location", ref YLoc);
            ImGui.InputInt("Y 坐标", ref YLoc);

            // if (ImGui.Button($"Test Radius"))
            if (ImGui.Button($"测试半径"))
            {
                var agent = AgentMap.Instance();

                Utils.SetGatheringRing(agent->CurrentTerritoryId, XLoc, YLoc, Radius);
            }

            ImGui.Separator();
            // ImGui.InputText("Pandora Feature", ref PandoraFeature);
            ImGui.InputText("Pandora 功能", ref PandoraFeature);
            // if (ImGui.Button("Pause Feature"))
            if (ImGui.Button("暂停功能"))
            {
                P.Pandora.PauseFeature(PandoraFeature, amount);
            }

            ImGui.Separator();
            // ImGui.Text("AutoHook");
            ImGui.Text("AutoHook");
            ImGui.SetNextItemWidth(150);
            // ImGui.InputText("Preset String", ref importString, 2048);
            ImGui.InputText("预设字符串", ref importString, 2048);
            // if (ImGui.Button("Import"))
            if (ImGui.Button("导入"))
            {
                P.AutoHook.ImportAndSelectPreset(importString);
                importString = string.Empty;
            }
            ImGui.SetNextItemWidth(150);
            // ImGui.InputText("Swap to preset", ref SwapToPreset);
            ImGui.InputText("切换至预设", ref SwapToPreset);
            // if (ImGui.Button("Swap"))
            if (ImGui.Button("切换"))
            {
                P.AutoHook.SetPreset(SwapToPreset);
            }
            // if (ImGui.Button("Apply Temp"))
            if (ImGui.Button("应用临时"))
            {
                P.AutoHook.CreateAndSelectAnonymousPreset(importString);
            }
            ImGui.SetNextItemWidth(200);
            // ImGui.InputUInt("Select mission to import", ref missionId);
            ImGui.InputUInt("选择要导入的任务", ref missionId);
            // ImGui.InputUInt("Bait ID", ref baitId);
            ImGui.InputUInt("鱼饵 ID", ref baitId);
            // if (ImGui.Button("Swap to bait"))
            if (ImGui.Button("切换鱼饵"))
            {
                 SwapBait(baitId);
            }
            // if (ImGui.Button("Swap Bait... simple"))
            if (ImGui.Button("切换鱼饵... 简单"))
            {
                if (CosmicHelper.CurrentBait() == 0)
                {
                    // IceLogging.Debug("Bait is not currently equipped");
                    IceLogging.Debug("当前未装备鱼饵");
                }

                P.AutoHook.SwapBaitById(baitId);
            }
            // if (ImGui.Button("Stupid Test"))
            if (ImGui.Button("简单测试"))
            {
                if (CosmicHelper.CurrentBait() == 0)
                {
                    // IceLogging.Debug($"No bait is equipped");
                    IceLogging.Debug($"未装备任何鱼饵");
                }
                else if (CosmicHelper.CurrentBait == null)
                {
                    // IceLogging.Debug("Bait is null... aka not in the middle of a mission");
                    IceLogging.Debug("鱼饵为 null……即当前不在任务进行中");
                }
                else
                {
                    // IceLogging.Debug($"Current bait: {CosmicHelper.CurrentBait}");
                    IceLogging.Debug($"当前鱼饵：{CosmicHelper.CurrentBait}");
                }
            }

            // if (ImGui.Button("Enable AutoHook"))
            if (ImGui.Button("启用 AutoHook"))
            {
                P.AutoHook.Ah_State(true);
            }
            // if (ImGui.Button("Disable Autohook"))
            if (ImGui.Button("禁用 AutoHook"))
            {
                P.AutoHook.Ah_State(false);
            }

            ImGui.Separator();
            // ImGui.Text($"Is ICE Running? | {P.IceIpc.IsRunning()}");
            ImGui.Text($"ICE 是否运行? | {P.IceIpc.IsRunning()}");
            // if (ImGui.Button("Only Missions Via IPC"))
            if (ImGui.Button("仅通过 IPC 执行任务"))
            {
                HashSet<uint> missionListIds = new() { 1, 3, 4, 7, 9, 11 };
                P.IceIpc.OnlyMissions(missionListIds);
            }
            // if (ImGui.Button("Change to gamba"))
            if (ImGui.Button("切换至好运道"))
            {
                SchedulerMain.State = IceState.Gambling;
            }

            ImGui.Separator();

            ImGui.SetNextItemWidth(150);
            // ImGui.InputText("Setting Name", ref SettingChange);
            ImGui.InputText("设置名称", ref SettingChange);
            // ImGui.Checkbox("Setting Bool", ref SettingState);
            ImGui.Checkbox("设置布尔值", ref SettingState);

            // if (ImGui.Button("Toggle Setting"))
            if (ImGui.Button("切换设置"))
            {
                P.IceIpc.ChangeSetting(SettingChange, SettingState);
            }
            // if (ImGui.Button("Set temp setting"))
            if (ImGui.Button("设置临时配置"))
            {
                P.Artisan.ChangeSolver(37084, "Progress Only Solver", true);
            }
            // if (ImGui.Button("Set raphael solver"))
            if (ImGui.Button("设置 Raphael 求解器"))
            {
                P.Artisan.ChangeSolver(37084, "Raphael Recipe Solver", true);
            }
            // if (ImGui.Button("Set current mission to Raphael"))
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
            // if (ImGui.Button("Set current mission to Progress"))
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
            // ImGui.DragUInt("MM Step Use", ref MMSAmount, 1, 0, 20);
            ImGui.DragUInt("MM 步数使用", ref MMSAmount, 1, 0, 20);
            // ImGui.DragUInt("MM Recipe Usage", ref MMMaxUse, 1, 0, 3);
            ImGui.DragUInt("MM 配方使用", ref MMMaxUse, 1, 0, 3);
            // ImGui.Checkbox("Set MM Temp", ref tempMM);
            ImGui.Checkbox("设置 MM 临时", ref tempMM);
            // if (ImGui.Button("Set Miracle Solver"))
            if (ImGui.Button("设置 Miracle 求解器"))
            {
                P.Artisan.ChangeStandardMinimumStepsBeforeMiracle(MMSAmount, tempMM);
                P.Artisan.ChangeStandardMaxMaterialMiracleUses(MMMaxUse, tempMM);
            }
            ImGui.SameLine();
            // if (ImGui.Button("Restore Temp MM"))
            if (ImGui.Button("恢复临时 MM"))
            {
                P.Artisan.SetTempStandardMinimumStepsBeforeMiracleBackToNormal();
                P.Artisan.SetTempStandardMaxMaterialMiracleUsesBackToNormal();
            }

            // if (ImGui.Button("Return back to normal"))
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
            // if (ImGui.Button("Disable Endurance"))
            if (ImGui.Button("禁用 Endurance"))
            {
                P.Artisan.SetEnduranceStatus(false);
            }
            // if (ImGui.Button("Test Toast"))
            if (ImGui.Button("测试 Toast"))
            {
                // string message = "[I.C.E.] You didn't read the little warning in the mission setup\n" +
                //     "You need to update autohook for you to be able to fish here on Auxesia. Please swap to testing version";
                string message = "[I.C.E.] 您未注意任务设置中的警告提示\n" +
                    "需要更新 AutoHook 才能在 Auxesia 自动捕鱼。\n" +
                    "请切换至测试版";
                Svc.Chat.Print(new()
                {
                    Type = Dalamud.Game.Text.XivChatType.ErrorMessage,
                    Message = message,
                });
                Svc.Toasts.ShowNormal($"{message}");
            }
            // if (ImGui.Button("Test Glamour"))
            if (ImGui.Button("测试 Glamour"))
            {
                P.GlamourIpc.SetClownHead();
            }
            // if (ImGui.Button("Test Hat"))
            if (ImGui.Button("测试帽子"))
            {
                P.GlamourIpc.SetHat();
            }
            // if (ImGui.Button("Test Visor"))
            if (ImGui.Button("测试帽檐"))
            {
                P.GlamourIpc.SetVisor();
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
