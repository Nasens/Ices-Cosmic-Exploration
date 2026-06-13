namespace ICE.Enums
{
    [Flags]
    internal enum IceState
    {
        Idle = 0,
        Start = 1,
        GrabMission = 2,
        ExecutingMission = 3,
        AbandonMission = 4,
        ForceTurnin = 5,
        ScoreCheck = 6,
        ManualMode = 7,
        Waiting = 8,

        HubReturn = 10,
        Repair = 11,
        Gambling = 12,
        RelicTurnin = 13,
        Spiritbond = 14,
        Shopping = 15,
        ArtifactSearch = 16,

        Craft = 20,
        Gather = 21,
        Fish = 22,
        DualClass = 23,

        ScoringMission = 30,
        AnimationLock = 31,
        TurninMission = 32,
    }

    internal static class IceStateExtensions
    {
        public static string DisplayName(IceState state) => state switch
        {
            // IceState.Idle => "Idle",
            IceState.Idle => "空闲",
            // IceState.Start => "Start",
            IceState.Start => "启动",
            // IceState.GrabMission => "GrabMission",
            IceState.GrabMission => "接取任务",
            // IceState.ExecutingMission => "ExecutingMission",
            IceState.ExecutingMission => "执行任务",
            // IceState.AbandonMission => "AbandonMission",
            IceState.AbandonMission => "放弃任务",
            // IceState.ForceTurnin => "ForceTurnin",
            IceState.ForceTurnin => "强制交付",
            // IceState.ScoreCheck => "ScoreCheck",
            IceState.ScoreCheck => "分数检查",
            // IceState.ManualMode => "ManualMode",
            IceState.ManualMode => "手动模式",
            // IceState.Waiting => "Waiting",
            IceState.Waiting => "等待中",
            // IceState.HubReturn => "HubReturn",
            IceState.HubReturn => "返回中枢",
            // IceState.Repair => "Repair",
            IceState.Repair => "修理",
            // IceState.Gambling => "Gambling",
            IceState.Gambling => "赌博中",
            // IceState.RelicTurnin => "RelicTurnin",
            IceState.RelicTurnin => "Relic 交付",
            // IceState.Spiritbond => "Spiritbond",
            IceState.Spiritbond => "精炼度",
            // IceState.Shopping => "Shopping",
            IceState.Shopping => "购物",
            // IceState.ArtifactSearch => "ArtifactSearch",
            IceState.ArtifactSearch => "无人机搜索",
            // IceState.Craft => "Craft",
            IceState.Craft => "制作",
            // IceState.Gather => "Gather",
            IceState.Gather => "采集",
            // IceState.Fish => "Fish",
            IceState.Fish => "捕鱼",
            // IceState.DualClass => "DualClass",
            IceState.DualClass => "双职业",
            // IceState.ScoringMission => "ScoringMission",
            IceState.ScoringMission => "计分任务",
            // IceState.AnimationLock => "AnimationLock",
            IceState.AnimationLock => "动画锁定",
            // IceState.TurninMission => "TurninMission",
            IceState.TurninMission => "交付任务",
            // _ => state.ToString(),
            _ => "未知",
        };
    }
}