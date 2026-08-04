using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.WKS;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ICE.Sounds;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.GatheringHelper;
using ICE.Utilities.GatheringHelper.RouteLoader;
using System.Collections.Generic;
using System.Linq;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;

namespace ICE.Scheduler.Tasks
{
    internal static class Task_CheckMissions
    {
        public enum MissionKind
        {
            Critical,
            Weather,
            Timed,
            Sequence,
            Ex,
            Master,
            A,
            B,
            C,
            D,
            Unknown,
        }

        private static Dictionary<MissionKind, List<uint>> MissionLibrary = new()
        {
            [MissionKind.Critical] = new(),
            [MissionKind.Weather] = new(),
            [MissionKind.Timed] = new(),
            [MissionKind.Sequence] = new(),
            [MissionKind.Ex] = new(),
            [MissionKind.Master] = new(),
            [MissionKind.A] = new(),
            [MissionKind.B] = new(),
            [MissionKind.C] = new(),
            [MissionKind.D] = new(),
            [MissionKind.Unknown] = new(),
        };

        private static readonly Random _random = new Random();

        public static void Enqueue()
        {
            P.TaskManager.EnqueueMulti
                (
                    new(() => RefreshMissionLibrary(), "Refreshing the mission library"),
                    new(() => OpenMissionUi(), "Opening Mission Ui"),
                    new(() => CheckTabs(), "Checking tabs for valid missions")
                );
        }
        private static void ReOpenMissionUi(string tag)
        {
            if (GenericHelpers.TryGetAddonMaster<WKSMission>("WKSMission", out var missionUi) && missionUi.IsAddonReady)
                return;

            if (GenericHelpers.TryGetAddonMaster<WKSHud>("WKSHud", out var moonHud) && moonHud.IsAddonReady)
            {
                if (EzThrottler.Throttle("Opening the mission ui"))
                {
                    // IceLogging.Info("Opening the moon mission selection hud", tag);
                    IceLogging.Info("正在打开任务选择界面", tag);
                    moonHud.Mission();
                }
            }
        }

        private static readonly MissionKind[] HuntSpecialMissionKinds =
            [MissionKind.Critical, MissionKind.Weather, MissionKind.Timed, MissionKind.Sequence];

        private static readonly MissionKind[] StandardMissionKinds =
            [MissionKind.Ex, MissionKind.A, MissionKind.B, MissionKind.C, MissionKind.D];

        private static int EnabledStandardMissionCount()
        {
            var count = 0;
            foreach (var rank in StandardMissionKinds)
                count += MissionLibrary[rank].Count;
            return count;
        }

        /// <summary>
        /// Gold completion grind only: idle-wait when special missions remain ungolded,
        /// none are on the board, and there are no standard missions left to reroll for.
        /// </summary>
        private static bool WaitingForSpecialMissions() =>
            Mission_Settings.Mode == ModeSelect.MissionGoldMode
            && HuntSpecialMissionKinds.Any(kind => MissionLibrary[kind].Count > 0)
            && EnabledStandardMissionCount() == 0;

        private static void EnterWaitForSpecialMissions(string tag)
        {
            if (SchedulerMain.State != IceState.Waiting)
            {
                // IceLogging.Info("Gold completion grind: waiting for a timed, weather, or critical mission to appear on the board.", tag);
                IceLogging.Info("金牌任务刷取：等待限时、天气或紧急任务出现在任务板上。", tag);
                SchedulerMain.State = IceState.Waiting;
            }

            CosmicHandler.EnsureStandardMissionTab(Mission_Settings.SelectedJob);
            P.TaskManager.Tasks.Clear();
        }

        public static void EnqueueWaitRecheck()
        {
            P.TaskManager.Enqueue(() => WaitForSpecialMissionRecheck(), "Waiting for special mission availability");
        }

        private static bool? WaitForSpecialMissionRecheck()
        {
            string tag = "[Check Missions: Wait for Special]";

            if (!EzThrottler.Throttle("Recheck special missions", 15_000))
                return false;

            CosmicHandler.EnsureStandardMissionTab(Mission_Settings.SelectedJob);
            // IceLogging.Verbose("Rechecking mission board for timed/weather/critical missions", tag);
            IceLogging.Verbose("正在重新检查任务板上的限时/天气/紧急任务", tag);
            SchedulerMain.State = IceState.GrabMission;
            return true;
        }
        private static MissionKind LibraryInfo(KeyValuePair<uint, CosmicHelper.CosmicInfo> mission)
        {
            MissionKind entry = MissionKind.Unknown;
            var attribute = mission.Value.Attributes;
            var rank = mission.Value.Rank;

            if (attribute.HasFlag(MissionAttributes.ProvisionalWeather))
                entry = MissionKind.Weather;
            else if (attribute.HasFlag(MissionAttributes.ProvisionalTimed))
                entry = MissionKind.Timed;
            else if (attribute.HasFlag(MissionAttributes.ProvisionalSequential))
                entry = MissionKind.Sequence;
            else if (attribute.HasFlag(MissionAttributes.Critical))
                entry = MissionKind.Critical;
            else if (mission.Value.IsMaster)
                entry = MissionKind.Master;
            else if (rank != 0)
            {
                entry = rank switch
                {
                    5 => MissionKind.Ex,
                    4 => MissionKind.A,
                    3 => MissionKind.B,
                    2 => MissionKind.C,
                    1 => MissionKind.D,
                    _ => MissionKind.Unknown,
                };
            }

            return entry;
        }
        public static bool? RefreshMissionLibrary()
        {
            string tag = "Task Check Mission: Refresh Mission Library";

            foreach (var entry in MissionLibrary)
            {
                entry.Value.Clear();
            }

            var playerTerritory = Player.Territory.RowId;

            var enabledPerMoon = string.Join("\n",
                CosmicMoonRegistry.All.Select(m =>
                    $"{m.DisplayName} [{m.TerritoryId}] = [{CosmicMoonRegistry.CountEnabledMissions(m.TerritoryId)}]"));

            // IceLogging.Info("This is just general message to let me know WHAT planet you're on, and where you have things enabled\n" +
            //     "If you're not running things that requires these to be enabled, you can ignore this if you're reading this.\n" +
            //     $"{enabledPerMoon}\n" +
            //     $"Current TerritoryID: {playerTerritory}");
            IceLogging.Info("此消息仅用于告知你当前所在的星球，以及你在哪些区域启用了内容\n" +
                "如果你没有运行需要启用这些内容的功能，看到此消息可以忽略。\n" +
                $"{enabledPerMoon}\n" +
                $"当前区域 ID：{playerTerritory}");

            var modeSelected = Mission_Settings.Mode;
            foreach (var mission in CosmicHelper.SheetMissionDict)
            {
                if (mission.Value.TerritoryId != Player.Territory.RowId)
                    continue;

                bool provisional = mission.Value.IsProvisional;

                var missionId = mission.Key;

                if (C.MissionConfig.TryGetValue(missionId, out var config))
                {
                    if (modeSelected == ModeSelect.LevelMode && CosmicHelper.QuickLevelList.Contains(mission.Key))
                    {
                        var job = Mission_Settings.SelectedJob;
                        var jobLevel = Player.GetLevel((Job)job);
                        var missionLevel = mission.Value.Level;

                        if (!mission.Value.Jobs.Contains(job))
                            continue;

                        // Short end of it all, making sure to see what tier the player should be doing
                        // Taking the players level and making sure it matches to the tier
                        // 90+ = 90
                        // 50-89 = 50
                        // 10-49 = 10
                        int playerTier = jobLevel >= 90 ? 90 : jobLevel >= 50 ? 50 : 10;

                        if (missionLevel != playerTier)
                            continue;
                        else
                        {
                            MissionLibrary[LibraryInfo(mission)].Add(missionId);
                        }
                    }
                    else if (modeSelected == ModeSelect.RelicMode)
                    {
                        if (provisional)
                            continue;

                        if (mission.Value.IsCritical && !C.Relic_IncludeCriticals)
                            continue;

                        var jobLevel = Math.Min(Player.GetLevel((Job)mission.Value.Jobs.First()), Player.GetLevel((Job)mission.Value.Jobs.Last()));
                        if (jobLevel < mission.Value.Level)
                            continue;

                        if (C.XPRelicOnlyEnabled)
                        {
                            if (config.Enabled && mission.Value.Jobs.Contains(Mission_Settings.SelectedJob))
                                MissionLibrary[LibraryInfo(mission)].Add(missionId);
                        }
                        else
                        {
                            if (mission.Value.Jobs.Contains(Mission_Settings.SelectedJob))
                                MissionLibrary[LibraryInfo(mission)].Add(missionId);
                        }
                    }
                    else if (modeSelected == ModeSelect.Standard)
                    {
                        if (!config.Enabled)
                            continue;

                        var jobLevel = Math.Min(Player.GetLevel((Job)mission.Value.Jobs.First()), Player.GetLevel((Job)mission.Value.Jobs.Last()));
                        if (jobLevel < mission.Value.Level)
                            continue;

                        if (provisional)
                        {
                            if (C.GrindAllProvisionals)
                            {
                                MissionLibrary[LibraryInfo(mission)].Add(missionId);
                            }
                            else if (mission.Value.Jobs.Contains(Mission_Settings.SelectedJob))
                            {
                                MissionLibrary[LibraryInfo(mission)].Add(missionId);
                            }
                        }
                        else if (mission.Value.Attributes.HasFlag(MissionAttributes.Critical))
                        {
                            if (C.GrindOffClassRedAlert)
                                MissionLibrary[LibraryInfo(mission)].Add(missionId);
                            else if (mission.Value.Jobs.Contains(Mission_Settings.SelectedJob))
                                MissionLibrary[LibraryInfo(mission)].Add(missionId);
                        }
                        else
                        {
                            if (mission.Value.Jobs.Contains(Mission_Settings.SelectedJob))
                                MissionLibrary[LibraryInfo(mission)].Add(missionId);
                        }
                    }
                    else if (modeSelected == ModeSelect.MissionGoldMode)
                    {
                        if (MissionGolded(missionId))
                            continue;

                        if (mission.Value.Attributes.HasFlag(MissionAttributes.Critical))
                        {
                            if (C.GrindOffClassRedAlert)
                                MissionLibrary[LibraryInfo(mission)].Add(missionId);
                            else if (mission.Value.Jobs.Contains(Mission_Settings.SelectedJob))
                                MissionLibrary[LibraryInfo(mission)].Add(missionId);
                        }
                        else if (provisional)
                        {
                            if (C.GrindAllProvisionals || mission.Value.Jobs.Contains(Mission_Settings.SelectedJob))
                            {
                                if (mission.Value.SequenceMissions_Previous.Count() != 0 || mission.Value.SequenceMissions_Next.Count() != 0)
                                {
                                    foreach (var prevSeqMission in mission.Value.SequenceMissions_Previous)
                                    {
                                        var seqMission = CosmicHelper.SheetMissionDict.Where(x => x.Key == prevSeqMission).FirstOrDefault();
                                        if (!MissionLibrary[LibraryInfo(seqMission)].Contains(prevSeqMission))
                                            MissionLibrary[LibraryInfo(seqMission)].Add(prevSeqMission);
                                    }
                                    foreach (var nextSeqMission in mission.Value.SequenceMissions_Next)
                                    {
                                        var seqMission = CosmicHelper.SheetMissionDict.Where(x => x.Key == nextSeqMission).FirstOrDefault();
                                        if (!MissionLibrary[LibraryInfo(seqMission)].Contains(nextSeqMission))
                                            MissionLibrary[LibraryInfo(seqMission)].Add(nextSeqMission);
                                    }
                                }
                                MissionLibrary[LibraryInfo(mission)].Add(missionId);
                            }
                        }
                        else if (mission.Value.Jobs.Contains(Mission_Settings.SelectedJob))
                            MissionLibrary[LibraryInfo(mission)].Add(missionId);
                    }
                }
                else
                {
                    // IceLogging.Error("We're missing a mission from the config, please report back so I can fix this.\n" +
                    IceLogging.Error("配置中缺少任务，请反馈以便修复。\n" +
                        $"任务 ID：{mission.Key} | 职业（首个）{mission.Value.Jobs.First()} | 等级：{mission.Value.Rank}");
                }
            }

            if (MissionLibrary.All(x => x.Value.Count == 0))
            {
                if (modeSelected == ModeSelect.RelicMode && C.XPRelicOnlyEnabled)
                {
                    // IceLogging.ChatInfo("\"Only selected missions\" is enabled for Relic Grind, but no selected missions match your current job. Please select missions for this job, switch jobs, or disable the option.", "[I.C.E.]");
                    IceLogging.ChatInfo("Relic 刷取模式已启用「仅已启用任务」，但当前职业无匹配任务。请为该职业选择任务、切换职业，或关闭此选项。", "[I.C.E.]");
                    if (C.PlaySoundAlert)
                    {
                        _ = SoundPlayer.PlaySoundAsync();
                    }
                }
                else
                {
                    // IceLogging.Verbose("We currently have no viable missions... which is odd. Please make sure you have some enabled, or report back if this is incorrect\n" +
                    //     $"Config Mode: {C.SelectedMode} | Mode going into this: {Mission_Settings.Mode}", tag);
                    IceLogging.Verbose("当前没有可用任务……这有点奇怪。请确认你已启用了一些任务，若确实有误请反馈\n" +
                        $"配置模式：{C.SelectedMode} | 进入此流程的模式：{Mission_Settings.Mode}", tag);
                }

                SchedulerMain.State = IceState.Idle;
                P.TaskManager.Tasks.Clear();
                return true;
            }
            else
            {
                // IceLogging.Verbose("We've reached the end of the mission sorter, going to report back what our current mission counts are at:", tag);
                IceLogging.Verbose("已到达任务排序流程的末尾，下面汇报当前各类任务的数量：", tag);
                foreach (var key in MissionLibrary)
                {
                    IceLogging.Verbose($"[{key.Key}] = {key.Value.Count()}", tag);
                }
                // IceLogging.Verbose("Going to run the sorter one more time to make sure that the priority is set for all of these (it should but ya never know)", tag);
                IceLogging.Verbose("再运行一次排序，以确保所有任务的优先级都已正确设置（理论上已设置，但以防万一）", tag);
                foreach (var key in MissionLibrary.Keys.ToList())
                {
                    MissionLibrary[key] = MissionLibrary[key]
                        .OrderBy(x =>
                        {
                            var jobs = CosmicHelper.SheetMissionDict[x].Jobs;
                            var bestIndex = jobs
                                .Select(job => C.JobPrio.IndexOf(job))
                                .Where(i => i >= 0)
                                .DefaultIfEmpty(int.MaxValue)
                                .Min();
                            return bestIndex;
                        })
                        .ToList();
                }

                // IceLogging.Verbose($"Mission finder says we have a valid mission list. So we gonna go find one", tag);
                IceLogging.Verbose($"任务查找器表示我们有可用的任务列表，接下来去找一个任务", tag);
                // IceLogging.Verbose($"Stardard tab missions job: {Mission_Settings.SelectedJob}");
                IceLogging.Verbose($"标准标签页任务职业：{Mission_Settings.SelectedJob}");
                return true;
            }
        }
        public static bool? OpenMissionUi()
        {
            string tag = "[Task Check Mission: Open Mission UI]";

            if (GenericHelpers.TryGetAddonMaster<Talk>("Talk", out var talkUi) && talkUi.IsAddonReady)
            {
                if (EzThrottler.Throttle("Closing the talk"))
                {
                    // IceLogging.Info("Talk ui was visible, clicking through", tag);
                    IceLogging.Info("检测到对话框界面，点击跳过", tag);
                    talkUi.Click();
                }

                return false;
            }

            if (CosmicHandler.CanQueryMissionsWithoutUi())
            {
                CosmicHandler.EnsureStandardMissionTab(Mission_Settings.SelectedJob);

                if (WaitingForSpecialMissions())
                {
                    IceLogging.Verbose("Mission agent is active — reading the board without opening WKSMission UI", tag);
                    return true;
                }
            }

            if (GenericHelpers.TryGetAddonMaster<WKSMission>("WKSMission", out var hud) && hud.IsAddonReady)
            {
                // IceLogging.Info("The Mission Selection Ui is visible! Continuing on", tag);
                IceLogging.Info("任务选择界面已显示，继续执行", tag);
                return true;
            }

            ReOpenMissionUi(tag);

            return false;
        }
        public static bool? CheckTabs()
        {
            string tag = "Check Missions: Check Tabs";
            var priority = C.MissionTypePrio;

            if (GenericHelpers.TryGetAddonMaster<WKSMission>("WKSMission", out var x) && x.IsAddonReady
                || CosmicHandler.CanQueryMissionsWithoutUi())
            {
                foreach (var type in C.MissionTypePrio)
                {
                    switch (type)
                    {
                        case MissionTypes.Critical:
                        {
                            if (MissionLibrary[MissionKind.Critical].Count > 0)
                            {
                                P.TaskManager.Enqueue(() => CheckMissions(MissionLibrary[MissionKind.Critical], type), "Checking Critical tab for missions");
                            }
                            break;
                        }
                        case MissionTypes.Provisional:
                        {
                            List<uint> provisionals = new();
                            foreach (var ProvisionalPrio in C.MissionPrio)
                            {
                                MissionKind key = ProvisionalPrio switch
                                {
                                    ProvisionalTypes.ProvisionalWeather => MissionKind.Weather,
                                    ProvisionalTypes.ProvisionalSequential => MissionKind.Sequence,
                                    ProvisionalTypes.ProvisionalTimed => MissionKind.Timed,
                                    _ => MissionKind.Unknown
                                };

                                if (MissionLibrary.TryGetValue(key, out var missionList))
                                {
                                    foreach (var jobId in C.JobPrio)
                                    {
                                        // Add missions that match this provisional type AND this job
                                        foreach (var missionId in missionList)
                                        {
                                            if (CosmicHelper.SheetMissionDict.TryGetValue(missionId, out var missionInfo) && missionInfo.Jobs.Contains(jobId) && !provisionals.Contains(missionId))
                                            {
                                                provisionals.Add(missionId);
                                            }
                                        }
                                    }
                                }
                            }
                            if (provisionals.Count > 0)
                            {
                                P.TaskManager.Enqueue(() => CheckMissions(provisionals, type), "Checking Provisional tab for missions");
                            }
                            break;
                        }
                        case MissionTypes.Standard:
                        {
                            var mode = Mission_Settings.Mode;
                            if (mode is ModeSelect.MissionGoldMode)
                            {
                                List<uint> basicMissions = new();
                                List<MissionKind> MissionRanks = new() { MissionKind.D, MissionKind.C, MissionKind.B, MissionKind.A, MissionKind.Ex };
                                foreach (var rank in MissionRanks)
                                {
                                    if (MissionLibrary.TryGetValue(rank, out var missionList))
                                    {
                                        foreach (var mission in missionList)
                                        {
                                            if (!basicMissions.Contains(mission))
                                                basicMissions.Add(mission);
                                        }
                                    }
                                }
                                P.TaskManager.Enqueue(() => CheckMissions(basicMissions, type, Mission_Settings.SelectedJob));
                                /*
                                foreach (var job in C.JobPrio)
                                {
                                    if (job == Mission_Settings.SelectedJob)
                                        continue;
                                    else
                                        P.TaskManager.Enqueue(() => CheckMissions(basicMissions, type, job));
                                }
                                */
                                break;
                            }
                            else
                            {
                                List<uint> basicMissions = new();
                                List<MissionKind> MissionRanks = new() { MissionKind.Ex, MissionKind.A, MissionKind.B, MissionKind.C, MissionKind.D };
                                foreach (var rank in MissionRanks)
                                {
                                    if (MissionLibrary.TryGetValue(rank, out var missionList))
                                    {
                                        foreach (var mission in missionList)
                                        {
                                            if (!basicMissions.Contains(mission))
                                                basicMissions.Add(mission);
                                        }
                                    }
                                }
                                P.TaskManager.Enqueue(() => CheckMissions(basicMissions, type), "Checking Basic Mission tab for missions");
                                break;
                            }
                        }
                        case MissionTypes.DroneSearch:
                        {
                            if (C.Cosmodrone_Run && CosmicMoonRegistry.TryGetMoon(Player.Territory.RowId, out var hub) && hub.HasCosmodrome)
                            {
                                P.TaskManager.Enqueue(() => Task_ArtifactSearch.RefreshMapInfo(), "Inserting Drone Task");
                            }
                            break;
                        }
                        case MissionTypes.ToolMastery:
                        {
                            if (MissionLibrary[MissionKind.Master].Count > 0)
                            {
                                P.TaskManager.Enqueue(() => CheckMissions(MissionLibrary[MissionKind.Master], type), "Checking for master missions");
                            }
                            break;
                        }
                    }
                }

                // Tool Mastery missions live in their own in-game tab (no dedicated getter),
                // so handle them explicitly regardless of MissionTypePrio ordering.
                if (MissionLibrary[MissionKind.Master].Count > 0)
                {
                    P.TaskManager.Enqueue(() => CheckMissions(MissionLibrary[MissionKind.Master], MissionTypes.ToolMastery), "Checking Tool Mastery tab for missions");
                }

                P.TaskManager.Enqueue(() => FindReroll(), "Find mission to reroll for");
            }
            else
            {
                ReOpenMissionUi(tag);
            }
            return true;
        }
        private static bool? CheckMissions(List<uint> missionList, MissionTypes type, uint Goldjob = 0)
        {
            string tag = "[Check Missions: Queue]";
            void LogInfo(uint missionId)
            {
                var sheetInfo = CosmicHelper.SheetMissionDict[missionId];
                bool provisional = sheetInfo.IsProvisional;
                var redAlert = sheetInfo.IsCritical;
                string jobs = string.Join(", ", sheetInfo.Jobs);

                // IceLogging.Info($"We found a mission! We're going to exit out of this task and grab the following: \n " +
                //     $"[Id] = {missionId}\n" +
                //     $"[Selected Job] = {Mission_Settings.SelectedJob}\n" +
                //     $"[Mission Job] = {jobs}\n" +
                //     $"Red Alert: {redAlert}\n" +
                //     $"Provisional: {provisional}", tag);
                IceLogging.Info($"找到任务！将退出此流程并接取以下任务： \n " +
                    $"[任务 ID] = {missionId}\n" +
                    $"[选定职业] = {Mission_Settings.SelectedJob}\n" +
                    $"[任务职业] = {jobs}\n" +
                    $"紧急警报：{redAlert}\n" +
                    $"临时任务：{provisional}", tag);
            }

            if (CosmicHandler.CanQueryMissionsWithoutUi()
                || (GenericHelpers.TryGetAddonMaster<WKSMission>("WKSMission", out var missionInfo) && missionInfo.IsAddonReady))
            {
                var basicMissionList = CosmicHandler.Basic_AvailableMissions();
                var specialMissionList = CosmicHandler.Provisional_AvailableMissions();
                var criticalMissions = CosmicHandler.Critical_AvailableMissions();
                var masteryMissions = CosmicHandler.Mastery_AvailableMissions();
                var mode = Mission_Settings.Mode;

                var job = Goldjob != 0 ? Goldjob : Mission_Settings.SelectedJob;

                // Tool Mastery missions live on their own category tab (3); everything else is Basic (0).
                byte categoryTab = type is MissionTypes.ToolMastery ? CosmicHandler.ToolMasteryTab : (byte)0;

                if (CorrectJobTab(job, categoryTab))
                {
                    if (mode == ModeSelect.LevelMode)
                    {
                        var levelingMission = missionList.FirstOrDefault();
                        // IceLogging.Verbose($"Leveling Mission: Job: {Mission_Settings.SelectedJob} | Mission: {levelingMission} | Level: {CosmicHelper.SheetMissionDict[levelingMission].Level}", debugOnly: true);
                        IceLogging.Verbose($"练级任务：职业：{Mission_Settings.SelectedJob} | 任务：{levelingMission} | 等级：{CosmicHelper.SheetMissionDict[levelingMission].Level}", debugOnly: true);
                        if (basicMissionList.Contains(levelingMission))
                        {
                            LogInfo(levelingMission);
                            Insert_GrabMissionTask(levelingMission);
                            return true;
                        }

                        // IceLogging.Verbose($"We seem to have not found the mission. Going to double check to make sure we have the tab unlocked", tag);
                        IceLogging.Verbose($"似乎没有找到该任务，再次确认对应标签页是否已解锁", tag);

                        var highestRank = basicMissionList.Max(x => CosmicHelper.SheetMissionDict[x].Rank);
                        var level = Player.GetLevel((Job)Mission_Settings.SelectedJob);
                        uint missionId = 0;

                        if (level >= 50 && highestRank < 2)
                        {
                            // IceLogging.Verbose("We need to unlock the Lv. 50 Missions [C Rank] so we get better exp gains", tag);
                            IceLogging.Verbose("我们需要解锁 50 级任务 [C 级] 以获得更高的经验收益", tag);
                            missionId = basicMissionList
                                .Where(x => CosmicHelper.Unlock_MissionList.Contains(x))
                                .Where(x => CosmicHelper.SheetMissionDict[x].Drank)
                                .Where(x => CosmicHelper.SheetMissionDict[x].CompletionStatus is CosmicHelper.Status.None)
                                .FirstOrDefault();
                            // IceLogging.Verbose($"Lv. 50 Mission: {missionId}", tag);
                            IceLogging.Verbose($"50 级任务：{missionId}", tag);
                        }
                        else if (level >= 90 && highestRank < 3)
                        {
                            // IceLogging.Verbose("We need to unlock the Lv. 90 Missions [B Rank] so we get better exp gains", tag);
                            IceLogging.Verbose("我们需要解锁 90 级任务 [B 级] 以获得更高的经验收益", tag);
                            missionId = basicMissionList
                                .Where(x => CosmicHelper.Unlock_MissionList.Contains(x))
                                .Where(x => CosmicHelper.SheetMissionDict[x].CRank)
                                .Where(x => CosmicHelper.SheetMissionDict[x].CompletionStatus is CosmicHelper.Status.None)
                                .FirstOrDefault();
                            // IceLogging.Verbose($"Lv. 90 Mission: {missionId}", tag);
                            IceLogging.Verbose($"90 级任务：{missionId}", tag);
                        }

                        if (missionId != 0)
                        {
                            // IceLogging.Verbose("We found a mission that we need to complete for one reason or another, going to queue it up for leveling!", tag);
                            IceLogging.Verbose("找到一个出于某种原因需要完成的任务，将其加入练级队列！", tag);
                            LogInfo(missionId);
                            Insert_GrabMissionTask(missionId);
                            return true;
                        }
                        else
                        {
                            // IceLogging.Verbose("For one reason or another, we seem to have reached the bottom. Which either means rerolling for specific mission or just rerolling for unlocking purposes", tag);
                            IceLogging.Verbose("出于某种原因，我们似乎已检索到底部。这意味着要么为特定任务重刷，要么只是为了解锁而重刷", tag);
                            return true;
                        }
                    }
                    else if (mode == ModeSelect.RelicMode)
                    {
                        var relicInfo = CosmicHelper.Cosmic_ClassInfo();
                        var classInfo = relicInfo[job];

                        var jobLv = Player.GetLevel((Job)job);

                        var urgency = new Dictionary<int, float>();
                        // IceLogging.Verbose("Relic mode was enabled. So going to do checks to see what exp we need", tag);
                        IceLogging.Verbose("已启用 Relic 模式，开始检查需要哪种经验", tag);
                        // IceLogging.Verbose($"Current Stage is the max stage? {classInfo.Stage_Current == classInfo.Stage_Next}", tag);
                        IceLogging.Verbose($"当前阶段是否为最大阶段？{classInfo.Stage_Current == classInfo.Stage_Next}", tag);
                        // IceLogging.Verbose($"Exp Current Tallies: [Check before finding missions]", tag);
                        IceLogging.Verbose($"当前经验统计：[查找任务前的检查]", tag);
                        foreach (var exp in classInfo.CurrentExp)
                        {
                            // IceLogging.Verbose($"Kind: [{exp.Key}] | Current: {exp.Value.Current} / Needed: {exp.Value.Needed} | Max: {exp.Value.Max}", tag);
                            IceLogging.Verbose($"类型：[{exp.Key}] | 当前：{exp.Value.Current} / 需要：{exp.Value.Needed} | 上限：{exp.Value.Max}", tag);
                            if (classInfo.Stage_Current != classInfo.Stage_Next)
                                urgency[exp.Key] = exp.Value.Needed > 0 ? 1f - (float)exp.Value.Current / exp.Value.Needed : 0f;
                            else
                                urgency[exp.Key] = 1f - (float)exp.Value.Current / exp.Value.Max;
                        }
                        if (urgency.Count() == 0 || urgency.All(x => x.Value <= 0))
                        {
                            // IceLogging.Verbose("We seem to be still grinding out relic exp (either by choice or cause someone didn't turnin) so we're going to just assign it to go for maxing exp", tag);
                            IceLogging.Verbose("我们似乎仍在刷取 Relic 经验（可能是有意为之，或是有人没有交付任务），因此将其设定为尽量获取最大经验", tag);
                            foreach (var exp in classInfo.CurrentExp)
                                urgency[exp.Key] = 1f - (float)exp.Value.Current / exp.Value.Max;
                        }
                        
                        if (urgency.All(x => x.Value <= 0))
                        {
                            // IceLogging.Verbose("We seem to be completed with the exp, but also, I don't have a mode setup for score farming yet. So setting the last exp value to be 1 so it just grabs a mission", tag);
                            IceLogging.Verbose("经验似乎已经刷满，但目前还没有专门的刷分模式。因此将最后一项经验值设为 1，让它直接接取一个任务", tag);
                            var lastEntry = urgency.LastOrDefault();
                            urgency[lastEntry.Key] = 1;
                        }

                        // IceLogging.Verbose("Going to check to see if we need to complete a specific mission...", tag);
                        IceLogging.Verbose("开始检查我们是否需要完成某个特定任务……", tag);

                        var highestRank = basicMissionList.Max(x => CosmicHelper.SheetMissionDict[x].Rank);

                        bool TryQueueFirstIncomplete(Func<uint, bool> rankFilter, string rankLabel)
                        {
                            var mission = basicMissionList
                                .Where(x => CosmicHelper.SheetMissionDict[x].CompletionStatus < CosmicHelper.Status.Completed)
                                .Where(x => rankFilter(x))
                                .FirstOrDefault();

                            if (mission != 0)
                            {
                                // IceLogging.Verbose($"Found an incomplete mission, queuing it now [{rankLabel}]", tag);
                                IceLogging.Verbose($"找到一个未完成的任务，现在将其加入队列 [{rankLabel}]", tag);
                                Insert_GrabMissionTask(mission);
                                return true;
                            }
                            return false;
                        }

                        bool TryQueueFirstNonGold(Func<uint, bool> rankFilter, string rankLabel)
                        {
                            var mission = basicMissionList
                                .Where(x => CosmicHelper.SheetMissionDict[x].CompletionStatus < CosmicHelper.Status.Gold)
                                .Where(x => rankFilter(x))
                                .FirstOrDefault();

                            if (mission != 0)
                            {
                                // IceLogging.Verbose($"Found a mission that can be golded, queuing it [{rankLabel}]", tag);
                                IceLogging.Verbose($"找到一个可以刷金的任务，将其加入队列 [{rankLabel}]", tag);
                                Insert_GrabMissionTask(mission);
                                return true;
                            }
                            return false;
                        }

                        bool isDRank(uint x) => CosmicHelper.SheetMissionDict[x].Drank;
                        bool isCRank(uint x) => CosmicHelper.SheetMissionDict[x].CRank;
                        bool isBRank(uint x) => CosmicHelper.SheetMissionDict[x].BRank;

                        if (jobLv >= 100 && highestRank < 4)
                        {
                            // IceLogging.Verbose("Hey! Lv 100 Missions still need to be unlocked, so going to check to see what need to do unlock those..", tag);
                            IceLogging.Verbose("嘿！100 级任务还需要解锁，开始检查解锁它们需要做些什么..", tag);
                            if (highestRank < 2)
                            {
                                // IceLogging.Verbose("ABSOLUTELY no ranks are unlocked yet (We're at D Rank Currently) Going to start with that and work our way up.", tag);
                                IceLogging.Verbose("目前完全没有解锁任何等级（当前处于 D 级），将从 D 级开始逐步往上推进。", tag);
                                if (TryQueueFirstIncomplete(isDRank, "D Rank")) return true;
                            }
                            else if (highestRank < 3)
                            {
                                // IceLogging.Verbose("Status Report. C Ranks are unlocked, but missing B Ranks, so we're going to aim to complete a C Rank.", tag);
                                IceLogging.Verbose("状态汇报：C 级已解锁，但还缺 B 级，因此我们的目标是完成一个 C 级任务。", tag);
                                if (TryQueueFirstIncomplete(isCRank, "C Rank")) return true;
                            }
                            else
                            {
                                // IceLogging.Verbose("Woooooo B Ranks unlocked! Checking to see if there's a gold need to be completed, or just general completions.", tag);
                                IceLogging.Verbose("耶！B 级已解锁！检查是否还需要刷金，或者只是普通完成即可。", tag);

                                var goldCount = CosmicHelper.SheetMissionDict
                                    .Where(x => x.Value.TerritoryId == Player.Territory.RowId)
                                    .Where(x => x.Value.BRank)
                                    .Where(x => x.Value.CompletionStatus == CosmicHelper.Status.Gold)
                                    .Where(x => x.Value.Jobs.Contains(job))
                                    .Count();

                                var completedStatus = CosmicHelper.SheetMissionDict
                                    .Where(x => x.Value.TerritoryId == Player.Territory.RowId)
                                    .Where(x => x.Value.BRank)
                                    .Where(x => x.Value.CompletionStatus > CosmicHelper.Status.None)
                                    .Where(x => x.Value.Jobs.Contains(job))
                                    .Count();
                                // IceLogging.Verbose($"Status | Gold [{goldCount} / 3] | Completed: [{completedStatus} / 5]", tag);
                                IceLogging.Verbose($"状态 | 金牌 [{goldCount} / 3] | 已完成：[{completedStatus} / 5]", tag);

                                if (goldCount < 3)
                                {
                                    // IceLogging.Verbose($"Missing Gold to help unlock B Ranks... so going to find one with that ideally", tag);
                                    IceLogging.Verbose($"还缺少用于解锁 B 级的金牌……因此最好去找一个能刷金的任务", tag);
                                    if (TryQueueFirstNonGold(isBRank, "B Rank")) return true;
                                }

                                if (completedStatus < 5)
                                {
                                    // IceLogging.Verbose($"We just need to complete more B Rank Missions (So close...).", tag);
                                    IceLogging.Verbose($"我们只需再完成几个 B 级任务（就快好了……）。", tag);
                                    if (TryQueueFirstIncomplete(isBRank, "B Rank")) return true;
                                }
                            }
                        }
                        else if (jobLv >= 90 && highestRank < 3)
                        {
                            // IceLogging.Verbose("We've hit Lv 90, and we STILL don't have B ranks unlocked, so going to focus that down.", tag);
                            IceLogging.Verbose("我们已达到 90 级，但仍未解锁 B 级，因此将集中精力解锁它。", tag);
                            if (TryQueueFirstIncomplete(isCRank, "C Rank")) return true;
                        }
                        else if (jobLv >= 50 && highestRank < 2)
                        {
                            // IceLogging.Verbose("We've atleast hit Lv. 50, and Absolutely no ranks unlocked right now besides D Ranks, going to focus on getting that done.", tag);
                            IceLogging.Verbose("我们至少已达到 50 级，但目前除 D 级外没有解锁任何等级，将集中精力完成它。", tag);
                            if (TryQueueFirstIncomplete(isDRank, "D Rank")) return true;
                        }


                        // IceLogging.Verbose($"Relic Mode, Exp Requirements/Results", tag);
                        IceLogging.Verbose($"Relic 模式，经验需求/结果", tag);
                        foreach (var exp in urgency)
                        {
                            // IceLogging.Verbose($"{exp.Key} : Value: {exp.Value:N2}", tag);
                            IceLogging.Verbose($"{exp.Key} : 数值：{exp.Value:N2}", tag);
                        }

                        var filteredList = missionList.Where(x => CosmicHelper.SheetMissionDict[x].RelicXpInfo.Any(kvp => urgency.ContainsKey(kvp.Key) && urgency[kvp.Key] > 0));
                        if (filteredList.Count() == 0)
                        {
                            if (EzThrottler.Throttle("No viable missions throttle"))
                                // IceLogging.Info("We've hit a point where somehow, there's no possible missions that could be grabbed to help you increase your exp to the point it's needed\n" +
                                //                 "So... this is an interesting spot... check to make sure that you're on the right planet to ", tag);
                                IceLogging.Info("我们遇到了一种情况：没有任何可接取的任务能帮你把经验提升到所需的程度\n" +
                                                "所以……这是个有点尴尬的情况……请确认你是否在正确的星球上 ", tag);

                            return true;
                        }
                        else
                        {
                            uint bestMissionId = 0;
                            float bestScore = float.NegativeInfinity;

                            foreach (var missionId in filteredList)
                            {
                                if (CosmicHelper.SheetMissionDict.TryGetValue(missionId, out var sheetInfo) && (basicMissionList.Contains(missionId) || specialMissionList.Contains(missionId)))
                                {
                                    if (jobLv < sheetInfo.Level)
                                    {
                                        // IceLogging.Verbose($"Skipping Mission: {missionId} due to not high enough lv [Player: {jobLv} | Mission: {sheetInfo.Level}].\n");
                                        IceLogging.Verbose($"跳过任务：{missionId}，因为等级不足 [玩家：{jobLv} | 任务：{sheetInfo.Level}]。\n");
                                        continue;
                                    }

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
                                    if (score > bestScore)
                                    {
                                        bestScore = score;
                                        bestMissionId = missionId;
                                    }
                                }
                            }

                            if (bestMissionId != 0)
                            {
                                LogInfo(bestMissionId);
                                Insert_GrabMissionTask(bestMissionId);
                                return true;
                            }
                            else
                            {
                                // IceLogging.Info("We've searched through all the missions and none had the exp we needed, which means time to reoll WOOOO!\n" +
                                //     $"Best Score: {bestScore}\n" +
                                //     $"Best Mission (not): {bestMissionId}", tag);
                                IceLogging.Info("我们搜索了所有任务，没有一个能提供所需的经验，这意味着该重刷了，耶！\n" +
                                    $"最佳分数：{bestScore}\n" +
                                    $"最佳任务（其实没有）：{bestMissionId}", tag);

                                return true;
                            }
                        }
                    }
                    else if (mode is ModeSelect.Standard or ModeSelect.MissionGoldMode)
                    {
                        if (type is MissionTypes.Standard)
                        {
                            // IceLogging.Verbose($"Checking Standard missions.\n" +
                            //     $"Loaded mission Count: {missionList.Count()}\n" +
                            //     $"Amount of viable missions: {basicMissionList.Count()}", tag);
                            IceLogging.Verbose($"正在检查标准任务。\n" +
                                $"已加载任务数量：{missionList.Count()}\n" +
                                $"可用任务数量：{basicMissionList.Count()}", tag);

                            foreach (var missionId in missionList)
                            {
                                if (basicMissionList.Contains(missionId))
                                {
                                    LogInfo(missionId);
                                    Insert_GrabMissionTask(missionId);
                                    return true;
                                }
                            }

                            // IceLogging.Info("No missions were found for basic missions tab. Continuing on", tag);
                            IceLogging.Info("基础任务标签页中未找到任务，继续执行", tag);
                            return true;
                        }
                        else if (type is MissionTypes.Provisional)
                        {
                            // IceLogging.Verbose($"Checking missions for the following mode:\n" +
                            //     $"Mode: {type}\n" +
                            //     $"Loaded mission count: {missionList.Count()}\n" +
                            //     $"Amount of viable missions: {specialMissionList.Count()}", tag);
                            IceLogging.Verbose($"正在检查以下模式的任务：\n" +
                                $"模式：{type}\n" +
                                $"已加载任务数量：{missionList.Count()}\n" +
                                $"可用任务数量：{specialMissionList.Count()}", tag);

                            foreach (var missionId in missionList)
                            {
                                if (specialMissionList.Contains(missionId))
                                {
                                    LogInfo(missionId);
                                    Insert_GrabMissionTask(missionId);
                                    return true;
                                }
                            }

                            // IceLogging.Verbose($"No missions were found for: {type}. Continuing on", tag);
                            IceLogging.Verbose($"未找到以下类型的任务：{type}，继续执行", tag);
                            return true;
                        }
                        else if (type is MissionTypes.Critical)
                        {
                            // IceLogging.Verbose($"Checking missions for the following mode:\n" +
                            //     $"Mode: {type}\n" +
                            //     $"Loaded mission count: {missionList.Count()}\n" +
                            //     $"Amount of available missions: {criticalMissions.Count()}", tag);
                            IceLogging.Verbose($"正在检查以下模式的任务：\n" +
                                $"模式：{type}\n" +
                                $"已加载任务数量：{missionList.Count()}\n" +
                                $"可用任务数量：{criticalMissions.Count()}", tag);

                            foreach (var missionId in missionList)
                            {
                                if (criticalMissions.Contains(missionId))
                                {
                                    LogInfo(missionId);
                                    Insert_GrabMissionTask(missionId);
                                    return true;
                                }
                            }

                            // IceLogging.Info("No missions were found for the critical missions, so continuing on", tag);
                            IceLogging.Info("未找到紧急任务，继续执行", tag);
                            return true;
                        }
                        else if (type is MissionTypes.ToolMastery)
                        {
                            // IceLogging.Verbose($"Checking missions for the following mode:\n" +
                            //     $"Mode: {type}\n" +
                            //     $"Loaded mission count: {missionList.Count()}\n" +
                            //     $"Amount of available missions: {masteryMissions.Count()}", tag);
                            IceLogging.Verbose($"正在检查以下模式的任务：\n" +
                                $"模式：{type}\n" +
                                $"已加载任务数量：{missionList.Count()}\n" +
                                $"可用任务数量：{masteryMissions.Count()}", tag);

                            foreach (var missionId in missionList)
                            {
                                if (masteryMissions.Contains(missionId))
                                {
                                    LogInfo(missionId);
                                    Insert_GrabMissionTask(missionId);
                                    return true;
                                }
                            }
                            return true;
                        }
                    }
                    else
                    {
                        if (EzThrottler.Throttle("Dumb dumb message"))
                            // IceLogging.Verbose("Not a valid mode was found. ICE. FIX THIS", tag);
                            IceLogging.Verbose("未找到有效的模式。ICE，修复这个问题。", tag);
                    }
                }
            }
            else
            {
                ReOpenMissionUi(tag);
            }

            return false;
        }
        private static void Insert_GrabMissionTask(uint missionId)
        {
            P.TaskManager.Tasks.Clear();

            // Extract materia between missions if spiritbond is ready and next mission is not EX+
            if (C.SelfSpiritbondGather && Task_Spiritbond.IsSpiritbondReadyAny())
            {
                if (CosmicHelper.SheetMissionDict.TryGetValue(missionId, out var nextMission) && nextMission.Rank < 6)
                {
                    // IceLogging.Info($"Next mission rank {nextMission.Rank} is below EX+, extracting materia first");
                    IceLogging.Info($"下一个任务的等级 {nextMission.Rank} 低于 EX+，先精炼魔晶石");
                    P.TaskManager.Enqueue(() => Task_Spiritbond.ExtractMateria(), "Extracting materia before next mission");
                }
            }

            P.TaskManager.EnqueueMulti
                (
                    new(() => CheckForMovementRequired(missionId), "Checking to see if we need to move to mission"),
                    new(() => Mission_ChangeJob(missionId), "Changing to correct job for mission"),
                    new(() => GrabMission(missionId), "Grabbing mission to initate")
                );
        }
        private static bool? Mission_ChangeJob(uint missionId)
        {
            // IceLogging.Verbose("Starting to change job");
            IceLogging.Verbose("开始切换职业");

            var mission = CosmicHelper.SheetMissionDict[missionId];
            var jobId = mission.Jobs.First();

            if ((uint)Player.Job == jobId)
                return true;
            else
            {
                if (EzThrottler.Throttle("Swapping to job for mission"))
                {
                    GearsetHandler.TaskClassChange((Job)jobId);
                    // IceLogging.Debug($"Swapping to job: {jobId}");
                    IceLogging.Debug($"正在切换到职业：{jobId}");
                }
                return false;
            }
        }
        private static Vector3 randomFishingHole = Vector3.Zero;
        private static bool? CheckForMovementRequired(uint missionId)
        {
            string tag = "[Check Missions: Movement Check]";

            var sheetInfo = CosmicHelper.SheetMissionDict[missionId];
            var missionConfig = C.MissionConfig[missionId];

            IceLogging.Info($"[MoveCheck] id={missionId} attrs=[{sheetInfo.Attributes}] gather={sheetInfo.IsGatherMission} fish={sheetInfo.IsFishMission} unsupported={UnsupportedMissions.Ids.Contains(missionId)} mapPos=({sheetInfo.MapPosition.X},{sheetInfo.MapPosition.Y})", tag);

            if (UnsupportedMissions.Ids.Contains(missionId))
            {
                // IceLogging.Info("Mission is currently in manual mode, or not supported. So not going to pathfind to it.", tag);
                IceLogging.Info("任务为手动模式或不支持，不进行寻路", tag);
                return true;
            }
            else if (!P.Navmesh.Installed)
            {
                // IceLogging.Error("HEY. YOU DIDN'T READ THE HELP ME PAGE. AND NOW YOU'RE MISSING NAVMESH. So... yeah... if things break this is why");
                IceLogging.Error("未安装 Navmesh！请查看帮助页。若出现问题，这很可能是原因。");
                return true;
            }
            else if (sheetInfo.IsGatherMission)
            {
                var route = sheetInfo.Gather_MapKey;

                var gatherInfo = GatheringRouteLoader.GetRoute(route);

                if (gatherInfo == null || gatherInfo.Nodes.Count == 0)
                {
                    // IceLogging.Error("Hey, so this is actually missing the information for it. So going to just actually add it to the unsupported mission list", tag);
                    IceLogging.Error("该任务缺少路线信息，已加入不支持任务列表", tag);
                    UnsupportedMissions.Ids.Add(missionId);
                    return true;
                }
                else
                {
                    var startNode = gatherInfo.Nodes[0];

                    foreach (var node in gatherInfo.Nodes)
                    {
                        if (Player.DistanceTo(node.Position) < 5)
                        {
                            // IceLogging.Info("We're close enough to the node! So continuing onto grabbing the mission", tag);
                            IceLogging.Info("我们离采集点已足够近！继续接取任务", tag);
                            return true;
                        }
                    }

                    // IceLogging.Verbose("If we've gotten this far, that means we need to figure out a path to go to the node. Doing so now", tag);
                    IceLogging.Verbose("如果走到这一步，说明我们需要计算前往采集点的路径，现在开始计算", tag);
                    var randomPosition = Task_NavmeshMove.Gather_RandomFanPosition(startNode);
                    Task_NavmeshMove.Enqueue_NavmeshTask(randomPosition);

                    return true;
                }
            }
            else if (sheetInfo.IsFishMission)
            {
                var location = sheetInfo.MapPosition;
                var territory = sheetInfo.TerritoryId;
                if (!GatheringUtil.MoonFishingLocations.TryGetValue(territory, out var zoneFishing)
                    || !zoneFishing.TryGetValue(location, out var fishingHole)
                    || fishingHole.Count == 0)
                {
                    // IceLogging.Error("We've seemed to have ran into a problem with the fishing hole... either it's missing spots, or it doesn't exist. Please report back to me on this with logs leading up to this\n" +
                    IceLogging.Error("钓鱼点数据异常（缺少点位或不存在），请附带日志反馈\n" +
                        $"任务 ID：{missionId} | 地图坐标：{location} | 区域：{territory}\n" +
                        $"已加入不支持列表，暂时在界面上标记", tag);
                    UnsupportedMissions.Ids.Add(missionId);
                    return true;
                }

                var customFishingHole = C.Personal_FishLocation.Where(x => x.MapCoords == location).FirstOrDefault();
                if (customFishingHole != null)
                {
                    var fishingLoc = customFishingHole.WorldPosition;

                    if (fishingLoc != null)
                    {
                        if (Player.DistanceTo(fishingLoc.Value) < 3)
                        {
                            // IceLogging.Info($"We have a custom fishing hole set, and we're close to it. {fishingLoc.Value}", tag);
                            IceLogging.Info($"已设置自定义钓鱼点，且我们离它很近。{fishingLoc.Value}", tag);
                            randomFishingHole = Vector3.Zero;
                            return true;
                        }
                        else
                        {
                            // IceLogging.Verbose($"We have a custom fishing hole set, and we're not within fishing range. Queueing up moving to it: {fishingLoc.Value}");
                            IceLogging.Verbose($"已设置自定义钓鱼点，但我们不在钓鱼范围内。将其加入移动队列：{fishingLoc.Value}");
                            Task_NavmeshMove.Enqueue_NavmeshTask(fishingLoc.Value);
                            randomFishingHole = Vector3.Zero;
                            return true;
                        }
                    }
                }

                foreach (var fishingSpot in fishingHole)
                {
                    if (Player.DistanceTo(fishingSpot.FishingSpot) < 3)
                    {
                        // IceLogging.Info($"We've reached our fishing spot! We are current at: {fishingSpot.FishingSpot}", tag);
                        IceLogging.Info($"我们已到达钓鱼点！当前位置：{fishingSpot.FishingSpot}", tag);
                        randomFishingHole = Vector3.Zero;
                        return true;
                    }
                }

                if (randomFishingHole == Vector3.Zero)
                {
                    var _random = new Random();
                    var randomIndex = _random.Next(fishingHole.Count);
                    if (EzThrottler.Throttle("Setting fishing hole destination"))
                    {
                        // IceLogging.Debug($"Random number spot said we're going to the following fishing hole #: {randomIndex}");
                        IceLogging.Debug($"随机数结果指示我们前往第 {randomIndex} 号钓鱼点");
                        randomFishingHole = fishingHole[randomIndex].FishingSpot;
                    }
                }
                else
                {
                    // IceLogging.Verbose("If we've gotten this far, that means we need to figure out a path to go to the node. Doing so now");
                    IceLogging.Verbose("如果走到这一步，说明我们需要计算前往采集点的路径，现在开始计算");
                    Task_NavmeshMove.Enqueue_NavmeshTask(randomFishingHole);
                    randomFishingHole = Vector3.Zero;
                    return true;
                }
            }
            else if (C.PersonalReturnSpot)
            {
                if (sheetInfo.Attributes.HasFlag(MissionAttributes.Critical))
                {
                    // IceLogging.Info($"We are currently aimed to do a critical mission, and we're on a crafter(?) so we're not going to move from our spot", tag);
                    IceLogging.Info($"我们当前的目标是做紧急任务，且处于生产职业（？），因此不会从当前位置移动", tag);
                    return true;
                }
                else
                {
                    var territory = Player.Territory.RowId;
                    if (C.CrafterLocations.TryGetValue(territory, out var location))
                    {
                        // IceLogging.Verbose("If we've gotten this far, that means we need to figure out a path to go to the node. Doing so now");
                    IceLogging.Verbose("如果走到这一步，说明我们需要计算前往采集点的路径，现在开始计算");
                        Task_NavmeshMove.Enqueue_NavmeshTask(location);
                        return true;
                    }
                    else
                    {
                        // IceLogging.Debug("No location is set for this place, so continuing on", tag);
                        IceLogging.Debug("此地点未设置位置，继续执行", tag);
                        return true;
                    }
                }
            }
            else
            {
                // IceLogging.Info("Mission was not a gathering or critical mission. Navmesh moving was not necessary. Moving onto next step", tag);
                IceLogging.Info("任务不是采集或紧急任务，无需 Navmesh 移动，进入下一步", tag);
                return true;
            }

            return false;
        }
        private static int retryCheck = 0;
        private static bool? GrabMission(uint missionId, bool reroll = false)
        {
            string tag = "[Check Missions: Grab Mission]";

            if (CosmicHelper.CurrentLunarMission != 0)
            {
                retryCheck = 0;
                Mission_Settings.ResetNodeCounter();

                if (reroll)
                {
                    SchedulerMain.State = IceState.AbandonMission;
                    Task_AbandonMission.ForceAbandon = true;
                }
                else
                {
                    SchedulerMain.State = IceState.ExecutingMission;
                    Task_AbandonMission.ForceAbandon = false;
                }
                Mission_Settings.nodeTotal = 0;
                P.TaskManager.Tasks.Clear();
                // IceLogging.Debug($"State upon exiting: {SchedulerMain.State}");
                IceLogging.Debug($"退出时的状态：{SchedulerMain.State}");
                return true;
            }
            else
            {
                if (GenericHelpers.TryGetAddonMaster<WKSMission>("WKSMission", out var missionInfo) && missionInfo.IsAddonReady)
                {
                    List<uint> viableMissions = new();
                    viableMissions.Add(missionId);

                    var job = CosmicHelper.SheetMissionDict[missionId].Jobs.First();

                    // TODO: Need to just clean this up later, the function to directly grab it is no longer necessary
                    // Tool Mastery missions are only readable/grabbable from their own tab (3).
                    byte categoryTab = CosmicHelper.SheetMissionDict[missionId].IsMaster ? CosmicHandler.ToolMasteryTab : (byte)0;

                    if (CorrectJobTab(job, categoryTab))
                    {
                        // IceLogging.Verbose("On the correct tab, we're going to see the total mission count", tag);
                        IceLogging.Verbose("已在正确的标签页，下面查看任务总数", tag);
                        var allmissions = CosmicHandler.All_AvailableMissions();
                        // IceLogging.Verbose($"All mission count: {allmissions.Count()} | Goal: {missionId}");
                        IceLogging.Verbose($"全部任务数量：{allmissions.Count()} | 目标：{missionId}");
                        foreach (var mission in allmissions.OrderBy(x => CosmicHelper.SheetMissionDict[x].Rank))
                        {
                            var sheetInfo = CosmicHelper.SheetMissionDict[mission];
                            IceLogging.Verbose($"ID: [{mission}] | Rank: [{sheetInfo.Rank}]", tag, true);
                        }


                        if (allmissions.Contains(missionId))
                        {
                            if (EzThrottler.Throttle("Selecting Mission", 1000))
                                InitiateMission(missionId);
                        }
                        else
                        {
                            if (FrameThrottler.Throttle("Counter added", 8))
                                retryCheck += 1;

                            if (retryCheck >= 4)
                            {
                                // IceLogging.Verbose($"Mission could no longer be found: {missionId}, retrying the process", tag);
                                IceLogging.Verbose($"已无法再找到任务：{missionId}，重新尝试该流程", tag);
                                retryCheck = 0;
                                P.TaskManager.Tasks.Clear();
                                return true;
                            }
                        }
                    }
                    else
                    {
                    }
                }
                else
                {
                    ReOpenMissionUi(tag);
                }
            }

            return false;
        }
        private static unsafe void InitiateMission(uint missionId)
        {
            var WKSInstance = WKSManager.Instance();
            if (WKSInstance != null)
            {
                WKSInstance->MissionModule->InitiateMission((ushort)missionId);
            }
        }
        private static bool? FindReroll()
        {
            string tag = "[Check Missions: Find Reroll]";

            if (WaitingForSpecialMissions())
            {
                EnterWaitForSpecialMissions(tag);
                return true;
            }

            if (GenericHelpers.TryGetAddonMaster<WKSMission>("WKSMission", out var missionInfo) && missionInfo.IsAddonReady)
            {
                var testMission = missionInfo.StellerMissions.FirstOrDefault();
                uint missionToAbandon = 0;
                if (testMission != null)
                {
                    var attribute = CosmicHelper.SheetMissionDict[testMission.MissionId].Attributes;
                    bool nonStandard = attribute.HasFlag(MissionAttributes.ProvisionalSequential) || attribute.HasFlag(MissionAttributes.ProvisionalTimed) 
                                    || attribute.HasFlag(MissionAttributes.ProvisionalWeather) || attribute.HasFlag(MissionAttributes.Critical);
             
                    if (nonStandard)
                    {
                        if (FrameThrottler.Throttle("Selecting proper tab", 8))
                        {
                            missionInfo.BasicMissions();
                        }
                        return false;
                    }
                    else
                    {
                        List<uint> AExRank = new List<uint>();
                        List<uint> ARank = new List<uint>();
                        List<uint> BRank = new List<uint>();
                        List<uint> CRank = new List<uint>();
                        List<uint> DRank = new List<uint>();

                        // IceLogging.Info($"We're abandoning mission... so this should be the right tab for this: Rank: {CosmicHelper.SheetMissionDict[testMission.MissionId].Rank}");
                        IceLogging.Info($"我们要放弃任务……所以这应该是对应的正确标签页：等级：{CosmicHelper.SheetMissionDict[testMission.MissionId].Rank}");

                        // Track mission appearance counts
                        foreach (var mission in missionInfo.StellerMissions)
                        {
                            var missionId = mission.MissionId;

                            // Increment appearance count
                            if (!Mission_Settings.missionApperenceCount.ContainsKey(missionId))
                                Mission_Settings.missionApperenceCount[missionId] = 0;
                            Mission_Settings.missionApperenceCount[missionId]++;

                            var rank = CosmicHelper.SheetMissionDict[missionId].Rank;
                            // IceLogging.Verbose($"Checking: {missionId} | Rank: {rank}");
                            IceLogging.Verbose($"正在检查：{missionId} | 等级：{rank}");

                            switch (rank)
                            {
                                case 6: // Master, treated as EX+ tier
                                case 5: AExRank.Add(missionId); break;
                                case 4: ARank.Add(missionId); break;
                                case 3: BRank.Add(missionId); break;
                                case 2: CRank.Add(missionId); break;
                                case 1:
                                default: DRank.Add(missionId); break;
                            }
                        }

                        bool CheckARanks = (MissionLibrary[MissionKind.Ex].Count > 0 || MissionLibrary[MissionKind.A].Count > 0) && (AExRank.Count > 0 || ARank.Count > 0);
                        bool CheckBRanks = (MissionLibrary[MissionKind.B].Count > 0 && BRank.Count > 0);
                        bool CheckCRanks = (MissionLibrary[MissionKind.C].Count > 0 && CRank.Count > 0);
                        bool CheckDRanks = (MissionLibrary[MissionKind.D].Count > 0 && DRank.Count > 0);

                        IceLogging.Verbose($"[Ex] = {AExRank.Count()}\n" +
                            $"[A] = {ARank.Count()}\n" +
                            $"[B] = {BRank.Count()}\n" +
                            $"[C] = {CRank.Count()}\n" +
                            $"[D] = {DRank.Count()}", tag);

                        List<MissionKind> ranks = new() { MissionKind.Ex, MissionKind.A, MissionKind.B, MissionKind.C, MissionKind.D };
                        var enabledCount = 0;
                        foreach (var rank in ranks)
                        {
                            enabledCount += MissionLibrary[rank].Count();
                        }

                        if (enabledCount == 0)
                        {
                            // IceLogging.Info("We don't have any basic missions enabled under the following class\n" +
                            //     $"{Mission_Settings.SelectedJob}. So we're just going to clear -> Reset (Assuming we're checking for timed and such)");
                            IceLogging.Info("我们在以下职业下没有启用任何基础任务\n" +
                                $"{Mission_Settings.SelectedJob}。因此将直接清空 -> 重置（假定我们在检查限时等任务）");
                            P.TaskManager.Tasks.Clear();
                            return true;
                        }

                        var random = new Random();
                        void ShuffleList<T>(List<T> list, Random rnd)
                        {
                            for (int i = list.Count - 1; i > 0; i--)
                            {
                                int j = rnd.Next(i + 1);
                                (list[i], list[j]) = (list[j], list[i]);
                            }
                        }

                        ShuffleList(AExRank, random);
                        ShuffleList(ARank, random);
                        ShuffleList(BRank, random);
                        ShuffleList(CRank, random);
                        ShuffleList(DRank, random);

                        // small function to find a frequent mission that might be locking us
                        uint FindFrequentMission(List<uint> missionList, int threshold = 3)
                        {
                            foreach (var missionId in missionList)
                            {
                                if (CosmicHelper.SheetMissionDict.TryGetValue(missionId, out var mission))
                                {
                                    if (mission.Jobs.Count == 2)
                                    {
                                        if (!(Player.GetLevel((Job)mission.Jobs[0]) >= 100 && Player.GetLevel((Job)mission.Jobs[1]) >= 100))
                                            continue;
                                    }
                                }

                                if (Mission_Settings.missionApperenceCount.TryGetValue(missionId, out int count) && count >= threshold)
                                {
                                    return missionId;
                                }
                            }
                            return 0;
                        }

                        if (CheckARanks)
                        {
                            // Check for frequent missions in A/AEx ranks first
                            uint frequentAEx = FindFrequentMission(AExRank, Mission_Settings.rerollThreshold);
                            uint frequentA = FindFrequentMission(ARank, Mission_Settings.rerollThreshold);

                            if (AExRank.Count > 2)
                            {
                                if (frequentAEx != 0)
                                {
                                    missionToAbandon = frequentAEx;
                                    // IceLogging.Debug($"Abandoning frequently appearing AEX mission (appeared {Mission_Settings.missionApperenceCount[frequentAEx]} times)", tag);
                                    IceLogging.Debug($"放弃频繁出现的 AEX 任务（已出现 {Mission_Settings.missionApperenceCount[frequentAEx]} 次）", tag);
                                    Mission_Settings.previousAbandonRank = 5;
                                }
                                else
                                {
                                    // IceLogging.Debug($"Only AEX Rank missions are available. Forcing an AEX rank to be accepted");
                                    IceLogging.Debug($"只有 AEX 级任务可用，强制接取一个 AEX 级任务");
                                    missionToAbandon = AExRank.First();
                                    Mission_Settings.previousAbandonRank = 5;
                                }
                            }
                            else if (ARank.Count > 2)
                            {
                                if (frequentA != 0)
                                {
                                    missionToAbandon = frequentA;
                                    // IceLogging.Debug($"Abandoning frequently appearing A mission (appeared {Mission_Settings.missionApperenceCount[frequentA]} times)", tag);
                                    IceLogging.Debug($"放弃频繁出现的 A 级任务（已出现 {Mission_Settings.missionApperenceCount[frequentA]} 次）", tag);
                                    Mission_Settings.previousAbandonRank = 4;
                                }
                                else
                                {
                                    // IceLogging.Debug($"Only A Rank missions are available. Forcing an A rank to be accepted", tag);
                                    IceLogging.Debug($"只有 A 级任务可用，强制接取一个 A 级任务", tag);
                                    missionToAbandon = ARank.First();
                                    Mission_Settings.previousAbandonRank = 4;
                                }
                            }
                            else
                            {
                                if (Mission_Settings.previousAbandonRank == 5)
                                {
                                    if (frequentA != 0)
                                    {
                                        missionToAbandon = frequentA;
                                        // IceLogging.Debug($"Abandoning frequently appearing A mission (appeared {Mission_Settings.missionApperenceCount[frequentA]} times)", tag);
                                    IceLogging.Debug($"放弃频繁出现的 A 级任务（已出现 {Mission_Settings.missionApperenceCount[frequentA]} 次）", tag);
                                        Mission_Settings.previousAbandonRank = 4;
                                    }
                                    else
                                    {
                                        missionToAbandon = ARank.First();
                                        // IceLogging.Debug($"Abandoning Rank 4 Mission.");
                                        IceLogging.Debug($"放弃 4 级任务。");
                                        Mission_Settings.previousAbandonRank = 4;
                                    }
                                }
                                else if (Mission_Settings.previousAbandonRank == 4)
                                {
                                    if (frequentAEx != 0)
                                    {
                                        missionToAbandon = frequentAEx;
                                        // IceLogging.Debug($"Abandoning frequently appearing AEX mission (appeared {Mission_Settings.missionApperenceCount[frequentAEx]} times)", tag);
                                    IceLogging.Debug($"放弃频繁出现的 AEX 任务（已出现 {Mission_Settings.missionApperenceCount[frequentAEx]} 次）", tag);
                                        Mission_Settings.previousAbandonRank = 5;
                                    }
                                    else
                                    {
                                        missionToAbandon = AExRank.First();
                                        // IceLogging.Debug($"Abandoning Rank 5 Mission", tag);
                                        IceLogging.Debug($"放弃 5 级任务", tag);
                                        Mission_Settings.previousAbandonRank = 5;
                                    }
                                }
                                else
                                {
                                    missionToAbandon = ARank.First();
                                    // IceLogging.Debug($"Starting off w/ abandoning an A rank", tag);
                                    IceLogging.Debug($"先从放弃一个 A 级任务开始", tag);
                                    Mission_Settings.previousAbandonRank = 4;
                                }
                            }
                        }
                        else if (CheckBRanks)
                        {
                            uint frequentB = FindFrequentMission(BRank, Mission_Settings.rerollThreshold);
                            if (frequentB != 0)
                            {
                                missionToAbandon = frequentB;
                                // IceLogging.Debug($"Abandoning frequently appearing B mission (appeared {Mission_Settings.missionApperenceCount[frequentB]} times)", tag);
                                IceLogging.Debug($"放弃频繁出现的 B 级任务（已出现 {Mission_Settings.missionApperenceCount[frequentB]} 次）", tag);
                            }
                            else
                            {
                                missionToAbandon = BRank.First();
                            }
                            Mission_Settings.previousAbandonRank = 3;
                        }
                        else if (CheckCRanks)
                        {
                            uint frequentC = FindFrequentMission(CRank, Mission_Settings.rerollThreshold);
                            if (frequentC != 0)
                            {
                                missionToAbandon = frequentC;
                                // IceLogging.Debug($"Abandoning frequently appearing C mission (appeared {Mission_Settings.missionApperenceCount[frequentC]} times)", tag);
                                IceLogging.Debug($"放弃频繁出现的 C 级任务（已出现 {Mission_Settings.missionApperenceCount[frequentC]} 次）", tag);
                            }
                            else
                            {
                                missionToAbandon = CRank.First();
                            }
                            Mission_Settings.previousAbandonRank = 2;
                        }
                        else if (CheckDRanks)
                        {
                            uint frequentD = FindFrequentMission(DRank, Mission_Settings.rerollThreshold);
                            if (frequentD != 0)
                            {
                                missionToAbandon = frequentD;
                                // IceLogging.Debug($"Abandoning frequently appearing D mission (appeared {Mission_Settings.missionApperenceCount[frequentD]} times)", tag);
                                IceLogging.Debug($"放弃频繁出现的 D 级任务（已出现 {Mission_Settings.missionApperenceCount[frequentD]} 次）", tag);
                            }
                            else
                            {
                                missionToAbandon = DRank.First();
                            }
                            Mission_Settings.previousAbandonRank = 1;
                        }
                        else if (Mission_Settings.Mode == ModeSelect.LevelMode)
                        {
                            if (MissionLibrary[MissionKind.B].Count > 0)
                            {
                                // IceLogging.Debug("Leveling mode is active. Need to find a valid C or D Rank mission", tag);
                                IceLogging.Debug("练级模式已启用，需要找到一个有效的 C 级或 D 级任务", tag);
                                var mission = missionInfo.StellerMissions.Where(m => CosmicHelper.SheetMissionDict[m.MissionId].Level == 50).FirstOrDefault();

                                if (mission != null)
                                {
                                    missionToAbandon = mission.MissionId;
                                }
                                else
                                {
                                    mission = missionInfo.StellerMissions.Where(m => CosmicHelper.SheetMissionDict[m.MissionId].Level == 10).FirstOrDefault();

                                    if (mission != null)
                                        missionToAbandon = mission.MissionId;
                                }
                            }
                            else if (MissionLibrary[MissionKind.C].Count > 0)
                            {
                                var mission = missionInfo.StellerMissions.Where(m => CosmicHelper.SheetMissionDict[m.MissionId].Level == 10).FirstOrDefault();
                                if (mission != null)
                                    missionToAbandon = mission.MissionId;
                            }
                        }

                        if (missionToAbandon != 0)
                        {
                            P.TaskManager.EnqueueMulti
                                (
                                    new(() => Mission_ChangeJob(missionToAbandon)),
                                    new(() => GrabMission(missionToAbandon, true))
                                );
                            return true;
                        }
                    }
                }
                else
                {
                    if (FrameThrottler.Throttle("Selecting proper tab", 8))
                    {
                        missionInfo.BasicMissions();
                    }
                    return false;
                }
            }
            else
            {
                ReOpenMissionUi(tag);
            }

            return false;
        }
        private static unsafe bool MissionGolded(uint id)
        {
            var managerPtr = WKSManager.Instance();
            if (managerPtr == null) return false;

            var isGolded = managerPtr->IsMissionGolded(id);

            return isGolded;
        }

        // functions that are used across things
        private static unsafe bool CorrectJobTab(uint job, byte categoryTab = 0)
        {
            var agent = AgentWKSMission.Instance();
            if (agent == null)
            {
                if (EzThrottler.Throttle("AgentWKSMission Error", 2000))
                    // IceLogging.Error("AgentWKSMission has returned null. CS code might need an update...", "Task: Check Mission | Open Job Tab");
                    IceLogging.Error("AgentWKSMission 返回 null，CS 代码可能需要更新...", "Task: Check Mission | Open Job Tab");

                return false;
            }

            return AgentWKSMissionEx.SetSelectedJobTab(agent, (byte)job, categoryTab);
        }
        private static void Notes()
        {
            /*
             * This is kind of my place to just... figure out how tf the logic is going to work. 
             * Right now, the logic is 
             * 1: Store all the missions in the dictionary.
             *   - This doesn't matter if what kind of mode, we're just storing it. It should... allow for re-rolling of missions even when in relic mode on weird edge cases (aka, only selected missions for some reason)
             * 2: Added in logic for checking each tab, and adding drone checking somewhere in there. 
             *   - The way this works should be: Check each tab for a mission. If one exist in that place, we're just going to clear the queue -> just proceed to the grab mission task 
             *   - If not, then it continues onto the next kind
             *   - Drone mode is in there as a general "Hey, we gonna check to see if we can open a drone/have a drone running -> find it between missions (this is nice cause it allows users to dictate when they're going to go looking for a box in case of weather. red alert...)
             * 3: If we get to this point in the queue and we STILL haven't grabbed a mission, it means that we need to reroll for one. 
             *   - Logic will be the same here as before. Check to see what ones need to be rerolled if possible
             *
            */
        }
    }
}
