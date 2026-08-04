using Dalamud.Game.ClientState.Conditions;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.WKS;
using ICE.Sounds;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.GatheringHelper;
using System.Collections.Generic;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;
using MissionRank = FFXIVClientStructs.FFXIV.Client.Game.WKS.WKSMissionModule.MissionRank;

namespace ICE.Scheduler.Tasks
{
    internal static class Task_TurninMission
    {
        public static uint PreviousMissionId = 0;
        private static bool PathfoundToRed = false;
        private static int PreviousScore = 0;
        private static bool HasInteracted = false;
        private static int TickRate = 0;

        public static void Enqueue()
        {
            P.TaskManager.EnqueueMulti
                (
                    new(() => CheckRedAlert(), "Checking for Red Alert Info"),
                    new(() => Mission_TurninV2(), "Turning in the mission to the moon gods", Utils.TaskConfig),
                    new(() => GoldCheck(), "Checking if Gold Check Task needs to be completed"),
                    new(() => CommandCheck(), "Checking for post mission commands"),
                    new(() => JobSwapCheck(), "Checking for necessary job swap")
                );
        }

        public static bool CheckRedAlert()
        {
            string tag = "Red Alert Check";

            var id = CosmicHelper.CurrentLunarMission;
            if (CosmicHelper.SheetMissionDict.TryGetValue(id, out var sheetInfo))
            {
                if (sheetInfo.IsCritical)
                {
                    // IceLogging.Verbose("Critical mission was found, checking for location info", tag);
                    IceLogging.Verbose("找到紧急任务，正在检查位置信息", tag);

                    if (GatheringUtil.CriticalSpots.TryGetValue(sheetInfo.Critical_MapKey, out var criticalInfo) && criticalInfo.WorldCords != Vector3.Zero)
                    {
                        if (Player.DistanceTo(criticalInfo.WorldCords) < 75)
                        {
                            // IceLogging.Verbose("We're close enough to the base location that we don't need to do any fancy traveling, going to check if we need to interact", tag);
                            IceLogging.Verbose("已足够接近目标位置，无需特殊移动，正在检查是否需要交互", tag);
                            P.TaskManager.Insert(() => RedAlert_CloseToTurnin(), "Checking to make sure we're close enough");
                        }
                        else
                        {
                            // IceLogging.Verbose("We're far enough away that we need to consider taking the npc for getting there, so going to do so");
                            IceLogging.Verbose("距离较远，需要考虑乘坐 NPC 前往，正在执行");
                            P.TaskManager.Insert(() => Task_NavmeshMove.Enqueue_RedAlertNavmesh(criticalInfo.WorldCords, distance: 75, missionId: id), "Checking to make sure we're close enough");
                        }
                    }
                    else
                    {
                        if (EzThrottler.Throttle("No recorded site: 2000"))
                            // IceLogging.Error("There is currently not a preset destination that we have recorded, so this means it's a new red alert. Please give me time to add this", tag);
                            IceLogging.Error("该紧急通告尚无预设交付点（新任务），请等待后续更新", tag);

                        P.TaskManager.Insert(() => RedAlert_CloseToTurnin(), "Checking to make sure we have a turnin that is close");
                    }
                }
                else
                {
                    // IceLogging.Info("We don't need to worry about a turnin point, so we're good. Continuing on");
                    IceLogging.Info("无需关注交付点，一切正常，继续执行");
                }
            }
            else
            {
                // IceLogging.Info($"Somehow we found a mission that doesn't exist? Please report this: {id}", tag);
                IceLogging.Info($"发现了一个不存在的任务？请反馈：{id}", tag);
            }

            return true;
        }
        public static bool RedAlert_CloseToTurnin()
        {
            string tag = "Red Alert: Traveling to turnin";

            if (Utils.TryGetObjectCollectionPoint() is { } collectionPoint)
            {
                if (!Task_NavmeshMove.Task_NavTo(collectionPoint.Position, false, 4).Value)
                {
                    return false;
                }
                else
                {
                    if (P.Navmesh.IsRunning())
                    {
                        if (EzThrottler.Throttle("Telling navmesh to stop"))
                            P.Navmesh.Stop();

                        return false;
                    }

                    // IceLogging.Info("We've reached a point where we can turnin, doing so", tag);
                    IceLogging.Info("已到达可交付的位置，正在交付", tag);
                    return true;
                }
            }
            else
            {
                if (EzThrottler.Throttle("Null collection point found"))
                    // IceLogging.Verbose("You're not close to the collection point, we need to get closer", tag);
                    IceLogging.Verbose("尚未接近收集点，需要再靠近一些", tag);
            }

            return false;
        }
        public static bool? Mission_TurninV2()
        {
            string tag = "[Mission Turnin]";
            var id = CosmicHelper.CurrentLunarMission;

            if (id == 0)
            {
                CosmicHelper.Task_UpdateRelicMissionInfo();

                PathfoundToRed = false;
                HasInteracted = false;

                // Complete the timer and get duration
                var duration = P.MissionTimer.CompleteMission();

                if (P.AutoHook.Installed)
                {
                    P.AutoHook.DeleteAllAnonymousPresets();
                }

                UpdateScoreInfo();
                Mission_Settings.TurninState = TurninState.None;

                CosmicHelper.Task_UpdateRelicMissionInfo();

                if (Mission_Settings.StopAfterCurrent)
                {
                    // IceLogging.Debug($"Stop after current was enabled. Stopping now", "[Task Turnin]");
                    IceLogging.Debug($"已启用「完成当前任务后停止」，正在停止", "[Task Turnin]");
                    SchedulerMain.State = IceState.Idle;
                    return true;
                }
                else
                {
                    // IceLogging.Debug($"Stop after current wasn't enabled. Grabbing another mission", "[Task Turnin]");
                    IceLogging.Debug($"未启用「完成当前任务后停止」，正在获取下一个任务", "[Task Turnin]");
                    SchedulerMain.State = IceState.Start;
                    return true;
                }
            }
            else
            {
                if (CosmicHelper.SheetMissionDict.TryGetValue(id, out var sheetInfo))
                {
                    PreviousMissionId = id;

                    if (sheetInfo.IsCritical)
                    {
                        Mission_Settings.TurninState = TurninState.Critical;
                        if (!PathfoundToRed)
                        {
                            PathfoundToRed = true;
                            P.Navmesh.Stop();
                        }

                        var collectionPoint = Utils.TryGetObjectCollectionPoint();
                        if (!Task_NavmeshMove.Task_NavTo(collectionPoint.Position, false, 4).Value)
                        {
                            return false;
                        }

                        if (EzThrottler.Throttle("Log Throttle", 1000))
                        {
                            // IceLogging.Debug("Attempting to turnin/chekcing if we need to navmesh stop");
                            IceLogging.Debug("尝试交付/检查是否需要停止 navmesh");
                        }

                        if (P.Navmesh.IsRunning())
                        {
                            if (EzThrottler.Throttle("Telling navmesh to stop"))
                                P.Navmesh.Stop();

                            return false;
                        }

                        if (!HasInteracted)
                        {
                            if (Svc.Condition[ConditionFlag.OccupiedInQuestEvent] || Svc.Condition[ConditionFlag.OccupiedInEvent])
                            {
                                HasInteracted = true;
                            }
                            else
                            {
                                if (EzThrottler.Throttle("Interacting with thing", 500))
                                {
                                    Utils.TargetgameObject(collectionPoint);
                                    Utils.InteractWithObject(collectionPoint);
                                }
                            }
                        }
                        else
                        {
                            if (EzThrottler.Throttle("Telling it to wait this much before turning it off", 6000))
                            {
                                TickRate += 1;
                            }
                            if (TickRate > 1)
                            {
                                TickRate = 0;
                                HasInteracted = false;
                            }
                        }
                    }
                    else
                    {
                        if ((uint)Player.Job == 18 && Svc.Condition[ConditionFlag.Gathering])
                        {
                            if (EzThrottler.Throttle("Stop fishing so we can turn in this mission!", 2000))
                                Task_DualClass.StopFishing();

                            return false;
                        }
                        if (GenericHelpers.TryGetAddonMaster<Gathering>("Gathering", out var gather) && gather.IsAddonReady)
                        {
                            if (EzThrottler.Throttle("Closing the gathering window"))
                                GenericHandlers.FireCallback("Gathering", true, -1);

                            return false;
                        }
                        else if (GenericHelpers.TryGetAddonMaster<GatheringMasterpiece>("GatheringMasterpiece", out var gathMasterpiece) && gathMasterpiece.IsAddonReady)
                        {
                            if (EzThrottler.Throttle("Closing the collectable menu"))
                                GenericHandlers.FireCallback("GatheringMasterpiece", true, -1);

                            return false;
                        }
                        else if (GenericHelpers.TryGetAddonMaster<WKSRecipeNotebook>("WKSRecipeNotebook", out var WksRecipe) && WksRecipe.IsAddonReady)
                        {
                            if (EzThrottler.Throttle("Closing the crafting menu"))
                                GenericHandlers.FireCallback("WKSRecipeNotebook", true, -1);

                            return false;
                        }

                        if (EzThrottler.Throttle("Setting Turnin State", 2000))
                        {
                            var rank = Task_CheckScore.CurrentRank();
                            Mission_Settings.TurninState = (int)rank switch
                            {
                                6 => TurninState.Master_Score,
                                3 => TurninState.Gold,
                                2 => TurninState.Silver,
                                _ => TurninState.Bronze,
                            };
                        }

                        if (Player.IsBusy)
                            return false;

                        if (EzThrottler.Throttle("Report Mission"))
                        {
                            ReportMission();
                        }
                    }
                }
            }

            return false;
        }

        private static unsafe void ReportMission()
        {
            if (EzThrottler.Throttle("Previous Score Set"))
                PreviousScore = ScoreCheck();

            var WKSInstance = WKSManager.Instance();
            WKSInstance->MissionModule->ReportMission();
        }

        public static bool? JobSwapCheck()
        {
            if (CosmicHelper.SheetMissionDict[PreviousMissionId].Jobs.Count == 2)
            {
                if (Player.Job != (Job)Mission_Settings.SelectedJob && Mission_Settings.SelectedJob != 0)
                {
                    if (EzThrottler.Throttle("Swapping to crafter job", 1000))
                        GearsetHandler.TaskClassChange((Job)Mission_Settings.SelectedJob);

                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        public static unsafe bool? GoldCheck()
        {
            var managerPtr = WKSManager.Instance();
            if (managerPtr == null) return false;

            var isGold = managerPtr->IsMissionGolded(PreviousMissionId);

            var sheetInfo = CosmicHelper.SheetMissionDict[PreviousMissionId];

            if (C.RemoveAfterGold && isGold)
            {
                List<uint> seqMissions = new();
                foreach (var mission in sheetInfo.SequenceMissions_Next)
                    seqMissions.Add(mission);
                foreach (var mission in sheetInfo.SequenceMissions_Previous)
                    seqMissions.Add(mission);
                seqMissions.Add(PreviousMissionId);

                if (seqMissions.All(x => managerPtr->IsMissionGolded(x)))
                {
                    foreach (var mission in seqMissions)
                    {
                        if (CosmicHelper.SheetMissionDict.TryGetValue(mission, out var missionInfo))
                        {
                            bool special = missionInfo.IsCritical || missionInfo.IsProvisional;

                            if (!special && C.KeepARanks)
                                continue;

                            C.MissionConfig[mission].Enabled = false;
                        }
                    }
                    C.Save();
                }
            }
            if (C.RemoveAfterGold && !isGold)
            {
                foreach (var prevMission in sheetInfo.SequenceMissions_Previous)
                    C.MissionConfig[prevMission].Enabled = true;

                C.Save();
            }

            // IceLogging.Info("Gold Check is complete, and checking to see what state we need to be in post cleanup");
            IceLogging.Info("金牌检查完成，正在检查清理后应进入的状态");
            if (Mission_Settings.StopAfterCurrent)
            {
                // IceLogging.Info("We're stopping after this mission", "[Gold Check Task]");
                IceLogging.Info("将在本任务后停止", "[Gold Check Task]");
                Mission_Settings.StopAfterCurrent = false;
                SchedulerMain.State = IceState.Idle;

                if (C.PlaySoundAlert)
                    _ = SoundPlayer.PlaySoundAsync();
            }
            else
            {
                // IceLogging.Info("We're continuing after this mission", "[Gold Check Task]");
                IceLogging.Info("将在本任务后继续", "[Gold Check Task]");
                SchedulerMain.State = IceState.Start;
            }

            return true;
        }

        public static unsafe bool? CommandCheck()
        {
            string tag = "Turnin Mission: Command Check";

            if (Mission_Settings.Mode == ModeSelect.LevelMode && Utils.HasPlugin("Stylist"))
            {
                var jobId = (uint)Player.Job;

                if (CosmicHelper.CrafterJobList.Contains(jobId))
                {
                    // IceLogging.Info("Executing command [/stylist crafter]");
                    IceLogging.Info("正在执行命令 [/stylist crafter]");
                    ExecuteCommand("/stylist crafter");
                }
                else if (CosmicHelper.GatheringJobList.Contains(jobId))
                {
                    // IceLogging.Info("Executing command [/stylist gatherer]");
                    IceLogging.Info("正在执行命令 [/stylist gatherer]");
                    ExecuteCommand("/stylist gatherer");
                }
                P.TaskManager.EnqueueDelay(500);
            }

            foreach (var task in C.PostMissionCommands)
            {
                // IceLogging.Info($"Queueing up the following command:\n" +
                //     $"{task.command}\n" +
                //     $"Delay: {task.Delay}", tag);
                IceLogging.Info($"正在排入以下命令：\n" +
                    $"{task.command}\n" +
                    $"延迟：{task.Delay}", tag);
                P.TaskManager.Enqueue(() => ExecuteCommand(task.command));
                if (task.Delay > 0)
                    P.TaskManager.EnqueueDelay(task.Delay);
            }
            return true;
        }

        public static bool? ExecuteCommand(string command)
        {
            Svc.Commands.ProcessCommand(command);
            // IceLogging.Info($"Command has been processed: {command}", "Turnin Mission: Execute Command");
            IceLogging.Info($"命令已处理：{command}", "Turnin Mission: Execute Command");
            return true;
        }

        public static unsafe int ScoreCheck()
        {
            var wksManager = WKSManager.Instance();
            if (wksManager == null || wksManager->ResearchModule == null || !wksManager->ResearchModule->IsLoaded)
                return 0;

            var scores = wksManager->State.Scores;
            return scores[(int)(uint)Player.Job - 8];
        }

        public static void UpdateScoreInfo()
        {
            var multiplier = 1;
            var turnin = Mission_Settings.TurninState;
            if (turnin == TurninState.Gold)
                multiplier = 5;
            else if (turnin == TurninState.Silver)
                multiplier = 4;

            var scoreDifference = (ScoreCheck() - PreviousScore);
            if (scoreDifference > 0)
            {
                scoreDifference = scoreDifference / multiplier;
                // IceLogging.Debug($"Base Mission score is: {scoreDifference}");
                IceLogging.Debug($"基础任务得分为：{scoreDifference}");
                C.ScoreKeeper[PreviousMissionId] = (uint)scoreDifference;

                if (scoreDifference < 1000)
                {
                    if (CosmicHelper.SheetMissionDict.TryGetValue(PreviousMissionId, out var missionInfo))
                    {
                        missionInfo.ClassScore = (uint)scoreDifference;
                    }
                    C.Save();
                }
            }
            PreviousScore = 0;
        }

        public static bool? ClearAllPostTask()
        {
            P.TaskManager.Tasks.Clear();
            // IceLogging.Info("All task post turning in mission have been cleared. We should have a clean slate now");
            IceLogging.Info("交付任务后的所有任务已清除，现在应为干净状态");
            return true;
        }
    }
}
