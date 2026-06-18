using Dalamud.Game.ClientState.Conditions;
using ECommons.GameHelpers;
using ICE.Sounds;
using ICE.Utilities.Cosmic_Helper;
using TerraFX.Interop.Windows;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;

namespace ICE.Scheduler.Tasks
{
    internal static class Task_CheckState
    {
        public static void Enqueue()
        {
            P.TaskManager.Enqueue(() => CheckStateV2(), "Checking to see what state we should be in");
        }

        private static void UpdateMissionState(uint missionId)
        {
            // Clearing the current mission modifiers.
            SchedulerMain.MissionState = MissionAttributes.None;

            // Grabbing the mission info from the dictionary entry
            var missionDictInfo = CosmicHelper.SheetMissionDict[missionId];

            // Updating the Mission state to be the same as the current mission that's fired.
            SchedulerMain.MissionState = missionDictInfo.Attributes;
        }
        private static bool? CheckStateV2()
        {
            string tag = "[Task: Check State]";

            // IceLogging.Verbose("Updating the mission completion status", tag);
            IceLogging.Verbose("正在更新任务完成状态", tag);
            CosmicHelper.Update_MissionCompletion();

            var currentMode = C.SelectedMode;
            var currentMissionId = CosmicHelper.CurrentLunarMission;
            if (CosmicHelper.CurrentLunarMission != 0)
                UpdateMissionState(currentMissionId);

            if (GenericHelpers.TryGetAddonMaster<WKSLottery>("WKSLottery", out var lottery) && lottery.IsAddonReady)
            {
                // IceLogging.Info("We are currently gambling at the wheel, so going to continue on with that and wait for it to finish", tag);
                IceLogging.Info("当前正在转盘抽奖，将继续等待其完成", tag);
                SchedulerMain.State = IceState.Gambling;
                return true;
            }
            else if (currentMissionId != 0)
            {
                // IceLogging.Verbose($"Currently in the middle of a mission: {CosmicHelper.CurrentLunarMission}. Checking the state of what we should do");
                IceLogging.Verbose($"当前正在进行任务：{CosmicHelper.CurrentLunarMission}。正在检查应执行的操作");
                if (GenericHelpers.TryGetAddonMaster<WKSMissionInfomation>("WKSMissionInfomation", out var missionInfo) && missionInfo.IsAddonReady)
                {
                    // IceLogging.Debug($"Mission Infomation was active, checking if a mission is timed out.");
                    IceLogging.Debug($"任务信息界面已激活，正在检查任务是否超时。");
                    if (CosmicHandler.IsMissionTimedOut())
                    {
                        // Mission time has reached 0, checking the score/aborting if necessary
                        // IceLogging.Info("Mission is currently timed out. Going to abandon the mission state", "[Task: Check State]");
                        IceLogging.Info("任务当前已超时，将放弃该任务状态", "[Task: Check State]");
                        SchedulerMain.State = IceState.AbandonMission;
                        P.TaskManager.Tasks.Clear();
                        return true;
                    }
                    else
                    {
                        // IceLogging.Debug($"Mission isn't timed out... checking other states");
                        IceLogging.Debug($"任务未超时……正在检查其他状态");
                        UpdateMissionState(currentMissionId);
                        C.MissionConfig.TryGetValue(currentMissionId, out var config);

                        var s = SchedulerMain.MissionState;
                        bool dualMission = (s.HasFlag(MissionAttributes.Craft) && (s.HasFlag(MissionAttributes.Gather) || s.HasFlag(MissionAttributes.Fish)));
                        // In the middle of a dual mission. 
                        // First, checking to see if you're in the middle of a gathering or crafting action
                        if (C.OnlyGrabMission_Debug || UnsupportedMissions.Ids.Contains(currentMissionId))
                        {
                            // TODO: Remove this once properly coded
                            if (s.HasFlag(MissionAttributes.Fish))
                            {
                                // IceLogging.Info("Currently not built in/supported yet. Swapping to manual mode");
                                IceLogging.Info("当前尚未内置/支持，正在切换到手动模式");
                            }
                            else
                            {
                                // IceLogging.Info($"You have either manual mode enabled, or you have OnlyGrabMission enabled. Swapping to manual mode state");
                                IceLogging.Info($"你已启用手动模式，或已启用「仅接取任务」，正在切换到手动模式状态");
                            }
                            SchedulerMain.State = IceState.ManualMode;
                        }
                        else if (dualMission)
                        {
                            // IceLogging.Info("We're in a dual craft mission, going to kick it over there", "[Task: Check State]");
                            IceLogging.Info("当前处于双职业制作任务中，将切换至对应流程", "[Task: Check State]");
                            Mission_Settings.ResetNodeCounter();
                            SchedulerMain.State = IceState.DualClass;
                        }
                        else if (Svc.Condition[ConditionFlag.Crafting] || P.Artisan.IsBusy())
                        {
                            // IceLogging.Info("We are on a crafter, and either in the middle of crafting or need to start.", "[Task: Check State]");
                            IceLogging.Info("当前为生产职业，正在制作中或需要开始制作。", "[Task: Check State]");
                            SchedulerMain.State = IceState.Craft;
                        }
                        else if (Svc.Condition[ConditionFlag.Gathering])
                        {
                            Mission_Settings.ResetNodeCounter();
                            // IceLogging.Info("On a gathering class, kicking over to the gathering action", "[Task: Check State]");
                            IceLogging.Info("当前为采集职业，正在切换至采集操作", "[Task: Check State]");
                            SchedulerMain.State = IceState.Gather;
                        }
                        else if (s.HasFlag(MissionAttributes.Fish))
                        {
                            // IceLogging.Debug("We seem to be in the middle of a fishing mission. Going to check presets");
                            IceLogging.Debug("当前似乎处于钓鱼任务中，正在检查预设");
							var missionConfig = C.MissionConfig[currentMissionId];
							if (config.Use_BuildinPreset)
							{
								// IceLogging.Debug("Use Built-In Presets Checked. Resetting/Importing presets.");
								IceLogging.Debug("已勾选「使用内置预设」，正在重置/导入预设。");
								P.AutoHook.DeleteAllAnonymousPresets();
								Task_ExecuteMission.FishingTask(currentMissionId);
							}
							else
							{
								// IceLogging.Debug("Use Built-In Presets Unchecked. Setting configured preset.");
								IceLogging.Debug("未勾选「使用内置预设」，正在设置已配置的预设。");
								string presetName = missionConfig.AutoHookPresetName;
								P.AutoHook.SetPreset(presetName);
							}
                            SchedulerMain.State = IceState.ScoreCheck;
                        }
                        else
                        {
                            // Not currently in the middle of an action, so time to check score and go from there.
                            // IceLogging.Debug("Not in the middle of an action, swapping to score checking", "[Task_CheckState]");
                            IceLogging.Debug("当前未处于任何操作中，正在切换至分数检查", "[Task_CheckState]");
                            SchedulerMain.State = IceState.ScoreCheck;
                        }

                        return true;
                    }
                }
                else
                {
                    // The mission info (the one that contains the timer + current score while a mission is active) isn't loaded. Going to fix that.
                    if (EzThrottler.Throttle("Attempting to open the mission information window"))
                    {
                        // IceLogging.Info("Opening the mission information window, you're in the middle of one!", "[Check State]");
                        IceLogging.Info("正在打开任务信息界面（你正在进行任务中）", "[Check State]");
                        CosmicHelper.OpenStellarMission();
                    }
                    return false;
                }
            }
            else if (currentMode == ModeSelect.AgendaMode)
            {
                if (C.StopOnceHitLunarCredits)
                {
                    var territory = Player.Territory.RowId;
                    if (!CosmicMoonRegistry.TryGetPlanetCreditItemId(territory, out var itemId))
                        return false;

                    PlayerHelper.GetItemCount(itemId, out var credits);
                    if (credits >= C.LunarCreditsCap)
                    {
                        // IceLogging.ChatInfo($"You've either hit the Lunar Credit threshold, or gone above it.\n" +
                        //                     $"Stopping I.C.E.", "[I.C.E.]");
                        IceLogging.ChatInfo($"已达到或超过行星点数阈值。\n" +
                                            $"正在停止 ICE。", "[I.C.E.]");
                        SchedulerMain.State = IceState.Idle;
                        if (C.PlaySoundAlert)
                        {
                            _ = SoundPlayer.PlaySoundAsync();
                        }
                        return true;
                    }
                }
                if (C.StopOnceHitCosmoCredits && !C.BuyItems)
                {
                    if (GenericHelpers.TryGetAddonMaster<WKSHud>("WKSHud", out var hud) && hud.IsAddonReady && (hud.CosmoCredit >= C.CosmoCreditsCap))
                    {
                        // IceLogging.ChatInfo($"Stopping the plugin as you have {hud.CosmoCredit} Cosmocredits.", "[I.C.E.]");
                        IceLogging.ChatInfo($"宇宙点数已达 {hud.CosmoCredit}，正在停止插件。", "[I.C.E.]");
                        SchedulerMain.State = IceState.Idle;
                        if (C.PlaySoundAlert)
                        {
                            _ = SoundPlayer.PlaySoundAsync();
                        }
                        return true;
                    }
                }

                // IceLogging.Verbose("We're currently in agenda mode. We need to check to see if we have anything even in the agenda before we continue", tag);
                IceLogging.Verbose("当前处于宇宙议程模式，需要先检查议程中是否有内容才能继续", tag);
                if (C.Cosmic_Agenda.Count > 0)
                {
                    // IceLogging.Verbose($"We have a task list that we need to complete! Going to swap over to check and see what goal we need to complete");
                    IceLogging.Verbose($"当前有需要完成的任务列表！将切换以检查需要完成的目标");
                    P.TaskManager.Enqueue(() => AgendaCheck(), "Start Mode: Agenda Check");
                    return true;
                }
                else
                {
                    // IceLogging.Error($"We're currently set in agenda mode, and it says we have none... if you believe this is an error, and you can prove that" +
                    //     $"you have setup your agenda to do as you want, please let me know <3", tag);
                    IceLogging.Error($"当前为宇宙议程模式，但未配置任何议程。若你认为这是错误，且已正确设置议程，请反馈 <3", tag);
                    SchedulerMain.State = IceState.Idle;
                    P.TaskManager.Tasks.Clear();
                    return true;
                }
            }
            else
            {
                Mission_Settings.Mode = currentMode;
                var jobId = Mission_Settings.SelectedJob;

                // IceLogging.Info("We have a pre-selected mode enabled. So we're just going to run that down till we're told to stop\n" +
                IceLogging.Info("已启用预选模式，将持续运行直至停止条件触发\n" +
                    // $"Selected Mode: {currentMode}\n" +
                    $"所选模式：{currentMode}\n" +
                    // $"Main job for basic missions: {jobId}", tag);
                    $"基础任务主职业：{jobId}", tag);
                // IceLogging.Info("We're going to do our standard check of [If we need to stop] and [What we need to do before a mission]", tag);
                IceLogging.Info("正在执行标准检查：[是否需要停止] 与 [任务前准备]", tag);

                var cosmicClassInfo = CosmicHelper.Cosmic_ClassInfo();
                if (C.StopWhenLevel)
                {
                    var level = Player.GetLevel((Job)jobId);
                    if (level >= C.TargetLevel)
                    {
                        SchedulerMain.State = IceState.Idle;
                        // IceLogging.ChatInfo("Stop At Player Level is enabled. \n" +
                        //                    $"Your current level is: {Player.Level} and Goal: {C.TargetLevel}", "[I.C.E.]");
                        IceLogging.ChatInfo("「达到等级时停止」已启用。\n" +
                                           $"当前等级：{Player.Level}，目标：{C.TargetLevel}", "[I.C.E.]");
                        if (C.PlaySoundAlert)
                        {
                            _ = SoundPlayer.PlaySoundAsync();
                        }

                        return true;
                    }
                }
                if (C.StopOnceHitCosmicScore)
                {
                    var currentScore = cosmicClassInfo[jobId].Score;
                    if (currentScore >= C.CosmicScoreCap)
                    {
                        SchedulerMain.State = IceState.Idle;
                        // IceLogging.ChatInfo("Stop At Cosmic Score is enabled. \n" +
                        //     $"Your current level is: {currentScore} and Goal: {C.CosmicScoreCap}", "[I.C.E.]");
                        IceLogging.ChatInfo("「达到宇宙分数时停止」已启用。\n" +
                            $"当前分数：{currentScore}，目标：{C.CosmicScoreCap}", "[I.C.E.]");
                        if (C.PlaySoundAlert)
                        {
                            _ = SoundPlayer.PlaySoundAsync();
                        }
                        return true;
                    }
                }
                if (C.StopOnceHitLunarCredits)
                {
                    var territory = Player.Territory.RowId;
                    if (!CosmicMoonRegistry.TryGetPlanetCreditItemId(territory, out var itemId))
                        return false;

                    PlayerHelper.GetItemCount(itemId, out var credits);
                    if (credits >= C.LunarCreditsCap)
                    {
                        // IceLogging.ChatInfo($"You've either hit the Lunar Credit threshold, or gone above it.\n" +
                        //                     $"Stopping I.C.E.", "[I.C.E.]");
                        IceLogging.ChatInfo($"已达到或超过行星点数阈值。\n" +
                                            $"正在停止 ICE。", "[I.C.E.]");
                        SchedulerMain.State = IceState.Idle;
                        if (C.PlaySoundAlert)
                        {
                            _ = SoundPlayer.PlaySoundAsync();
                        }
                        return true;
                    }
                }
                if (C.StopOnceHitCosmoCredits && !C.BuyItems)
                {
                    if (GenericHelpers.TryGetAddonMaster<WKSHud>("WKSHud", out var hud) && hud.IsAddonReady && (hud.CosmoCredit >= C.CosmoCreditsCap))
                    {
                        // IceLogging.ChatInfo($"Stopping the plugin as you have {hud.CosmoCredit} Cosmocredits.", "[I.C.E.]");
                        IceLogging.ChatInfo($"宇宙点数已达 {hud.CosmoCredit}，正在停止插件。", "[I.C.E.]");
                        SchedulerMain.State = IceState.Idle;
                        if (C.PlaySoundAlert)
                        {
                            _ = SoundPlayer.PlaySoundAsync();
                        }
                        return true;
                    }
                }
                if (C.StopOnceRelicFinished)
                {
                    var relicInfo = cosmicClassInfo[(uint)jobId];
                    bool potentionalTurnin = relicInfo.Stage_Current < relicInfo.Stage_Next;
                    bool canTurnin = true;

                    if (potentionalTurnin)
                    {
                        // IceLogging.Verbose("We have a relic that we can potentionally turnin. These are the current Exp Stats", tag);
                        IceLogging.Verbose("存在一件可能可以交付的 Relic。以下是当前经验数据", tag);

                        var totalExpCount = relicInfo.CurrentExp.Count();
                        if (totalExpCount != 0)
                        {
                            // IceLogging.Verbose($"Current Lv: {relicInfo.Stage_Current} | Next Lv: {relicInfo.Stage_Next}");
                            IceLogging.Verbose($"当前等级：{relicInfo.Stage_Current} | 下一等级：{relicInfo.Stage_Next}");
                            // IceLogging.Verbose($"Total Exp Types: {relicInfo.CurrentExp.Count()}");
                            IceLogging.Verbose($"经验类型总数：{relicInfo.CurrentExp.Count()}");
                            foreach (var exp in relicInfo.CurrentExp)
                            {
                                // IceLogging.Verbose($"Kind [{exp.Key}] | Current: [{exp.Value.Current}] / Needed: [{exp.Value.Needed}] | Max: [{exp.Value.Max}]", tag);
                                IceLogging.Verbose($"种类 [{exp.Key}] | 当前：[{exp.Value.Current}] / 所需：[{exp.Value.Needed}] | 上限：[{exp.Value.Max}]", tag);
                                canTurnin &= exp.Value.Current >= exp.Value.Needed;
                            }

                            if (canTurnin)
                            {
                                // IceLogging.Verbose("We can turn in the relic! (Allegedly) So going to check to see if we need to do so", tag);
                                IceLogging.Verbose("可以交付该 Relic！（据称）将检查是否需要交付", tag);
                                if (C.TurninRelic)
                                {
                                    // IceLogging.Verbose("We have turnin set to true, going to queue up later turning the relic into researchingWay", tag);
                                    IceLogging.Verbose("交付已设为开启，稍后将排入交付 Relic 的队列", tag);
                                }
                                else
                                {
                                    // IceLogging.ChatInfo("We're at the point we can turn in the relic! Please do so, or disable stop when at relic turnin", tag);
                                    IceLogging.ChatInfo("Relic 已可交付！请手动交付，或关闭「Relic 完成时停止」。", tag);
                                    SchedulerMain.State = IceState.Idle;
                                    if (C.PlaySoundAlert)
                                    {
                                        _ = SoundPlayer.PlaySoundAsync();
                                    }
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            if (EzThrottler.Throttle("Force update exp"))
                            {
                                // IceLogging.Verbose("We seem... to be missing the exp? Which is odd. So going to force an update?");
                                IceLogging.Verbose("似乎……缺少经验数据？这很奇怪。将强制更新一次");
                                CosmicHelper.Task_UpdateRelicMissionInfo();
                            }
                            return false;
                        }
                    }
                    else
                    {
                        bool isCapped = true;

                        // IceLogging.Verbose("Checking Max Relic Exp", tag);
                        IceLogging.Verbose("正在检查 Relic 满级经验", tag);
                        var totalExpCount = relicInfo.CurrentExp.Count();
                        if (totalExpCount != 0)
                        {
                            // IceLogging.Verbose($"Total Exp Types: {relicInfo.CurrentExp.Count()}");
                            IceLogging.Verbose($"经验类型总数：{relicInfo.CurrentExp.Count()}");
                            foreach (var exp in relicInfo.CurrentExp)
                            {
                                // IceLogging.Verbose($"Kind [{exp.Key}] | Current: [{exp.Value.Current}] / Max: [{exp.Value.Max}]", tag);
                                IceLogging.Verbose($"种类 [{exp.Key}] | 当前：[{exp.Value.Current}] / 上限：[{exp.Value.Max}]", tag);
                                isCapped &= exp.Value.Current == exp.Value.Max;
                            }

                            if (isCapped)
                            {
                                // IceLogging.Info("We've reached the completed relic level wooo! Stopping for now", tag);
                                IceLogging.Info("Relic 等级已满！正在停止", tag);
                                SchedulerMain.State = IceState.Idle;
                                if (C.PlaySoundAlert)
                                {
                                    _ = SoundPlayer.PlaySoundAsync();
                                }
                                P.TaskManager.Tasks.Clear();
                                return true;
                            }
                        }
                        else
                        {
                            if (EzThrottler.Throttle("Force update exp"))
                            {
                                // IceLogging.Verbose("We seem... to be missing the exp? Which is odd. So going to force an update?");
                                IceLogging.Verbose("似乎……缺少经验数据？这很奇怪。将强制更新一次");
                                CosmicHelper.Task_UpdateRelicMissionInfo();
                            }
                            return false;
                        }
                    }
                }
                if (C.StopAtRelicLv)
                {
                    var relicInfo = cosmicClassInfo[(uint)jobId];
                    // if 15 <= 20
                    if (C.RelicLv <= relicInfo.Stage_Current)
                    {
                        // IceLogging.ChatInfo($"Stopping the plugin as your current tool is at {relicInfo.Stage_Current} and your goal was: {C.RelicLv}");
                        IceLogging.ChatInfo($"当前工具阶段为 {relicInfo.Stage_Current}，已达目标 {C.RelicLv}，正在停止插件。");
                        SchedulerMain.State = IceState.Idle;
                        if (C.PlaySoundAlert)
                        {
                            _ = SoundPlayer.PlaySoundAsync();
                        }
                        return true;
                    }
                }

                // IceLogging.Info("We have passed all stop when checks. So going to just do a general check on what we need to do", tag);
                IceLogging.Info("已通过所有停止条件检查，正在检查任务前准备", tag);
                P.TaskManager.Enqueue(() => HubActivityCheck(), "Checking for reasons to go to hub");
            }

            return true;
        }
        private static bool? AgendaCheck()
        {
            string tag = "[Agenda Check]";

            var agenda = C.Cosmic_Agenda;
            var relicProgress = CosmicHelper.Cosmic_ClassInfo();
            PlayerHelper.GetItemCount(CosmicHelper.CosmoCreditItemId, out var creditAmount);
            int planetCreditAmount = 10000;
            var territory = Player.Territory.RowId;
            if (PlayerHelper.IsInCosmicZone())
            {
                if (CosmicMoonRegistry.TryGetPlanetCreditItemId(territory, out var planetCreditId))
                    PlayerHelper.GetItemCount(planetCreditId, out planetCreditAmount);
            }

            // Dronebits exist on Oizys and Auxesia only — TryGetValue avoids throwing on Sinus/Phaenna
            int dronebitAmount = 5000;
            if (CosmicMoonRegistry.TryGetDronebit(territory, out var dronebit))
                PlayerHelper.GetItemCount(dronebit.creditId, out dronebitAmount);

            // IceLogging.Verbose("Checking to see which one we're going to start (if any)", tag);
            IceLogging.Verbose("正在检查将要开始执行哪一项（如果有）", tag);

            foreach (var entry in agenda)
            {
                // IceLogging.Verbose($"Checking:\n" +
                //     $"Job: {entry.SelectedJob}\n" +
                //     $"Agenda: {entry.SelectedMode}");
                IceLogging.Verbose($"正在检查：\n" +
                    $"职业：{entry.SelectedJob}\n" +
                    $"议程：{entry.SelectedMode}");

                var job = entry.SelectedJob;
                var relicInfo = relicProgress[job];

                var relicLevel = relicInfo.Stage_Current;
                var classScore = relicInfo.Score;
                var level = Player.GetLevel((Job)job);

                bool MaxLevelExp = true;
                foreach (var exp in relicInfo.CurrentExp)
                {
                    if (relicInfo.Stage_Current < relicInfo.Stage_Next)
                    {
                        MaxLevelExp = false;
                        break;
                    }

                    if (exp.Value.Current != exp.Value.Max)
                    {
                        MaxLevelExp = false;
                        break;
                    }
                }

                var sheetInfo = CosmicHelper.SheetMissionDict.Where(x => x.Value.Jobs.Contains(job))
                    .Where(x => x.Value.TerritoryId == Player.Territory.RowId);

                var totalCompleted = sheetInfo.Where(x => x.Value.CompletionStatus is CosmicHelper.Status.Completed).ToList().Count();
                var totalMissions = sheetInfo.Count();

                var goal = entry.SelectedOption;
                bool achieved = false;

                if (CosmicMoonRegistry.IsMaxRelicPlaylistGoal(goal))
                    achieved = relicLevel >= CosmicMoonRegistry.GetMaxRelicGoal(goal);
                else
                achieved = goal switch
                {
                    PlaylistOptions.SelectedRelicLv => relicLevel >= entry.SelectedRelicLevel,
                    PlaylistOptions.CreditAmount => creditAmount >= entry.CreditAmount,
                    PlaylistOptions.PlanetAmount => planetCreditAmount >= entry.PlanetAmount,
                    PlaylistOptions.DronebitAmount => dronebitAmount >= entry.DronebitAmount,
                    PlaylistOptions.ClassLevel => level >= entry.ClassLevel,
                    PlaylistOptions.ClassScore => classScore >= entry.ClassScore,
                    PlaylistOptions.ToolMaxExp => MaxLevelExp,
                    PlaylistOptions.GoldClassMissions => totalCompleted == totalMissions,
                    _ => true
                };

                if (!achieved)
                {
                    var progress = goal switch
                    {
                        _ when CosmicMoonRegistry.IsMaxRelicPlaylistGoal(goal) => $"{relicLevel}/{CosmicMoonRegistry.GetMaxRelicGoal(goal)}",
                        PlaylistOptions.SelectedRelicLv => $"{relicLevel}/{entry.SelectedRelicLevel}",
                        PlaylistOptions.CreditAmount => $"{creditAmount}/{entry.CreditAmount}",
                        PlaylistOptions.PlanetAmount => $"{planetCreditAmount}/{entry.PlanetAmount}",
                        PlaylistOptions.DronebitAmount => $"{dronebitAmount}/{entry.DronebitAmount}",
                        PlaylistOptions.ClassLevel => $"{level}/{entry.ClassLevel}",
                        PlaylistOptions.ClassScore => $"{classScore}/{entry.ClassScore}",
                        PlaylistOptions.ToolMaxExp or PlaylistOptions.GoldClassMissions => $"{achieved}",
                        _ => "?"
                    };
                    // IceLogging.Info($"Priority has been found to achieve: {goal}. Going to aim to complete this goal", tag);
                    IceLogging.Info($"已找到要优先达成的目标：{goal}。将以完成该目标为目标", tag);
                    // IceLogging.Debug($"[Goal Check] {goal}: {progress} (achieved={achieved})", tag);
                    IceLogging.Debug($"[目标检查] {goal}：{progress}（已达成={achieved}）", tag);
                    Mission_Settings.Mode = entry.SelectedMode;
                    Mission_Settings.SelectedJob = entry.SelectedJob;
                    P.TaskManager.Enqueue(() => HubActivityCheck(), "Checking for reason to go to hub");
                    return true;
                }
                else
                {
                    // IceLogging.Info($"The following goal is complete, ignoring it: Goal: {goal} Job: {job}", tag);
                    IceLogging.Info($"以下目标已完成，将忽略：目标：{goal} 职业：{job}", tag);
                }
            }

            // IceLogging.Info("We've actually finished our agenda! Congrats. Stopping the process", tag);
            IceLogging.Info("你已完成全部议程！恭喜。正在停止流程", tag);
            P.TaskManager.Tasks.Clear();
            SchedulerMain.State = IceState.Idle;

            return true;
        }
        private static bool? HubActivityCheck()
        {
            string tag = "Task Check State: Hub Activity Check";

            var territoryId = Player.Territory.RowId;
            var relicProgress = CosmicHelper.Cosmic_ClassInfo();

            bool BuyDrones = false;
            bool GambaWheel = false;
            bool BuyItems = false;
            bool RepairVendor = false;
            bool TurninRelic = false;

            bool repairSelfGear = PlayerHelper.NeedsRepair(Char_Info.RepairPercent);
            bool repairAllGear = PlayerHelper.AnyNeedsRepair(Char_Info.RepairPercent) && Char_Info.RepairAllGear;

            var eventInfo = CosmicHandler.EventInfo();
            var worldState = eventInfo.Value.wksEvent;

            if (C.DisableHub_Critical && worldState is CosmicHandler.WKSEvents.RedAlert_Progressing)
            {
                // IceLogging.Info("We currently have a red alert up, and we were told NOT to go to the hub for hub related activities, so we're not going to do so\n" +
                //     "Progressing to grabbing missions", tag);
                IceLogging.Info("当前有红色警报，且设置为「不前往 Hub 进行相关活动」，因此不会前往\n" +
                    "正在继续接取任务", tag);
                SchedulerMain.State = IceState.GrabMission;

                return true;
            }

            // IceLogging.Verbose($"Repair Class: {repairSelfGear} | Repair All: {repairAllGear}", tag);
            IceLogging.Verbose($"修理本职业：{repairSelfGear} | 修理全部：{repairAllGear}", tag);

            bool selfRepairCraft = Char_Info.SelfRepairCrafter && CosmicHelper.CrafterJobList.Contains((uint)Player.Job);
            bool selfRepairGathering = Char_Info.SelfRepairGather && CosmicHelper.GatheringJobList.Contains((uint)Player.Job);

            bool spiritbonded = C.SelfSpiritbondGather 
                && CosmicHelper.GatheringJobList.Contains((uint)Player.Job)
                && Task_Spiritbond.IsSpiritbondReadyAny();

            if (repairSelfGear || repairAllGear)
            {
                // IceLogging.Verbose($"We were told we needed repairs, one of these should be true...", tag);
                IceLogging.Verbose($"已判定需要修理，以下条件之一应为真……", tag);
                if (Char_Info.RepairAtVendor)
                {
                    // IceLogging.Debug("We were told to repair at the vendor, so we'll add that to the list of hub activities", tag);
                    IceLogging.Debug("设置为在商人处修理，将其加入 Hub 活动列表", tag);
                    RepairVendor = true;
                }
                else
                {
                    // IceLogging.Verbose($"Self Repair Crafter: {selfRepairCraft} | Self Repair Gathering: {selfRepairGathering}");
                    IceLogging.Verbose($"自助修理生产职业：{selfRepairCraft} | 自助修理采集职业：{selfRepairGathering}");
                    if (selfRepairCraft || selfRepairGathering)
                    {
                        // IceLogging.Debug("We were told that we can repair at ONE of these. So going to exit -> self repair", tag);
                        IceLogging.Debug("设置为可在其中之一进行修理，将退出 -> 自助修理", tag);
                        SchedulerMain.State = IceState.Repair;
                        return true;
                    }
                }
            }

            if (spiritbonded)
            {
                SchedulerMain.State = IceState.Spiritbond;
                return true;
            }

            if (CosmicMoonRegistry.TryGetDronebit(territoryId, out var dronebitAmount))
            {
                BuyDrones = C.Cosmodrone_Buy && Task_ArtifactSearch.CanBuyDroneBoxes();
                // IceLogging.Verbose($"Buying drones? {BuyDrones}", tag);
                IceLogging.Verbose($"是否购买无人机？{BuyDrones}", tag);
            }
            if (CosmicMoonRegistry.TryGetPlanetCreditItemId(territoryId, out var gambaCredits) && PlayerHelper.GetItemCount(gambaCredits, out var gambaAmount))
            {
                // IceLogging.Verbose($"{C.GambaAtAmount} >= {gambaAmount} && Gamba between runs {C.GambaBetweenRuns}");
                IceLogging.Verbose($"{C.GambaAtAmount} >= {gambaAmount} && 运行间抽奖 {C.GambaBetweenRuns}");
                GambaWheel = C.GambaAtAmount <= gambaAmount && C.GambaBetweenRuns;
            }
            if (C.BuyItems)
            {
                if (PlayerHelper.GetItemCount(CosmicHelper.CosmoCreditItemId, out var creditAmount))
                {
                    BuyItems = creditAmount >= C.CosmoBuyAtAmount && Task_BuyCosmoItems.CanPurchaseAnyItem();
                }
            }
            if (C.TurninRelic)
            {
                var jobId = Mission_Settings.SelectedJob;
                var relicInfo = relicProgress[jobId];

                bool isUpgradable = relicInfo.Stage_Current < relicInfo.Stage_Next;

                if (isUpgradable)
                {
                    var totalExpCount = relicInfo.CurrentExp.Count();
                    // IceLogging.Verbose($"Total Exp Types: {relicInfo.CurrentExp.Count()}");
                    IceLogging.Verbose($"经验类型总数：{relicInfo.CurrentExp.Count()}");
                    if (totalExpCount != 0)
                    {
                        bool canTurnin = true;
                        foreach (var exp in relicInfo.CurrentExp)
                        {
                            // IceLogging.Verbose($"Kind [{exp.Key}] | Current: [{exp.Value.Current}] / Needed: [{exp.Value.Needed}] | Max: [{exp.Value.Max}]", tag);
                            IceLogging.Verbose($"种类 [{exp.Key}] | 当前：[{exp.Value.Current}] / 所需：[{exp.Value.Needed}] | 上限：[{exp.Value.Max}]", tag);
                            canTurnin &= exp.Value.Current >= exp.Value.Needed;
                        }
                        TurninRelic = isUpgradable && canTurnin;

                    }
                    else
                    {
                        if (EzThrottler.Throttle("Force update exp"))
                        {
                            // IceLogging.Verbose("We seem... to be missing the exp? Which is odd. So going to force an update?");
                            IceLogging.Verbose("似乎……缺少经验数据？这很奇怪。将强制更新一次");
                            CosmicHelper.Task_UpdateRelicMissionInfo();
                        }
                        return false;
                    }
                }
            }

            if (BuyDrones || GambaWheel || BuyItems || RepairVendor || TurninRelic)
            {
                // IceLogging.Info("We have some reason to return back to the base so... we're doing so.\n" +
                //                   $"Can Buy Drones: {BuyDrones}\n" +
                //                   $"Gamba Wheel: {GambaWheel}\n" +
                //                   $"Buying Cosmocredit Items: {BuyItems}\n" +
                //                   $"Repair At Vendor: {RepairVendor}\n" +
                //                   $"Turnin Relic: {TurninRelic}", tag);
                IceLogging.Info("有理由返回基地，因此正在返回。\n" +
                                  $"可购买无人机：{BuyDrones}\n" +
                                  $"抽奖转盘：{GambaWheel}\n" +
                                  $"购买宇宙点数物品：{BuyItems}\n" +
                                  $"在商人处修理：{RepairVendor}\n" +
                                  $"交付 Relic：{TurninRelic}", tag);
                Task_HubActivities.CanBuyDrones = BuyDrones;
                Task_HubActivities.CanGamba = GambaWheel;
                Task_HubActivities.CosmoBuy = BuyItems;
                Task_HubActivities.RepairNpc = RepairVendor;
                Task_HubActivities.RelicTurnin = TurninRelic;
                SchedulerMain.State = IceState.HubReturn;
            }
            else
            {
                // IceLogging.Info("We have no reason to go to the hub. So going to just proceed to see about grabbing missions");
                IceLogging.Info("没有理由前往 Hub，将直接继续尝试接取任务");
                SchedulerMain.State = IceState.GrabMission;
            }

            return true;
        }
    }
}
