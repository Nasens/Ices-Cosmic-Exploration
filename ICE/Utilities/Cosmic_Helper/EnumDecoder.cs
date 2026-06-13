using ICE.ConfigFiles;
using ICE.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.Utilities.Cosmic_Helper;

public static unsafe partial class CosmicHelper
{
    public static string PlaylistOptionString(PlaylistOptions option)
    {
        if (CosmicMoonRegistry.TryGetMoonForMaxRelicOption(option, out var moon))
            // return $"Max {moon.DisplayName} Relic [Lv. {moon.MaxRelicStage}]";
            return $"{moon.DisplayName} Relic 已满 [Lv. {moon.MaxRelicStage}]";

        return option switch
        {
            // PlaylistOptions.None => "None",
            PlaylistOptions.None => "无",
            // PlaylistOptions.SelectedRelicLv => "Selected Relic Level",
            PlaylistOptions.SelectedRelicLv => "所选 Relic 等级",
            // PlaylistOptions.CreditAmount => "Credit Amount",
            PlaylistOptions.CreditAmount => "宇宙点数",
            // PlaylistOptions.PlanetAmount => "Planetary Credit Amount",
            PlaylistOptions.PlanetAmount => "行星点数",
            // PlaylistOptions.DronebitAmount => "Planetary Dronebit Amount",
            PlaylistOptions.DronebitAmount => "行星无人机代币",
            // PlaylistOptions.ClassLevel => "Class Level",
            PlaylistOptions.ClassLevel => "职业等级",
            // PlaylistOptions.ClassScore => "Class Score",
            PlaylistOptions.ClassScore => "职业分数",
            // PlaylistOptions.GoldClassMissions => "All Missions Golded",
            PlaylistOptions.GoldClassMissions => "全部任务已金牌",
            // PlaylistOptions.ToolMaxExp => "Max Tool Exp",
            PlaylistOptions.ToolMaxExp => "工具经验已满",
            _ => "未知"
        };
    }

    public static string TurninStateDisplayName(TurninState state) => state switch
    {
        // TurninState.None => "None",
        TurninState.None => "无",
        // TurninState.Bronze => "Bronze",
        TurninState.Bronze => "铜",
        // TurninState.Silver => "Silver",
        TurninState.Silver => "银",
        // TurninState.Gold => "Gold",
        TurninState.Gold => "金",
        // TurninState.Critical => "Critical",
        TurninState.Critical => "紧急",
        // TurninState.SequenceGold => "SequenceGold",
        TurninState.SequenceGold => "序列金",
        // TurninState.TimeExpired => "TimeExpired",
        TurninState.TimeExpired => "限时结束",
        // TurninState.Master_Score => "Master_Score",
        TurninState.Master_Score => "大师分数",
        _ => state.ToString()
    };

    internal static string GambaTypeDisplayName(Config.GambaType type) => type switch
    {
        // Config.GambaType.Mount => "Mount",
        Config.GambaType.Mount => "坐骑",
        // Config.GambaType.Emote => "Emote",
        Config.GambaType.Emote => "动作",
        // Config.GambaType.Minion => "Minion",
        Config.GambaType.Minion => "宠物",
        // Config.GambaType.Outfit => "Outfit",
        Config.GambaType.Outfit => "套装",
        // Config.GambaType.Accessory => "Accessory",
        Config.GambaType.Accessory => "配饰",
        // Config.GambaType.Orchestrion => "Orchestrion",
        Config.GambaType.Orchestrion => "管弦乐谱",
        // Config.GambaType.Housing => "Housing",
        Config.GambaType.Housing => "家具",
        // Config.GambaType.Dye => "Dye",
        Config.GambaType.Dye => "染色",
        // Config.GambaType.Other => "Other",
        Config.GambaType.Other => "其他",
        // Config.GambaType.Materia => "Materia",
        Config.GambaType.Materia => "魔晶石",
        _ => type.ToString()
    };

    internal static string LogLevelDisplayName(IceLogging.LogLevel level) => level switch
    {
        // IceLogging.LogLevel.Error => "Error",
        IceLogging.LogLevel.Error => "错误",
        // IceLogging.LogLevel.Warning => "Warning",
        IceLogging.LogLevel.Warning => "警告",
        // IceLogging.LogLevel.Info => "Info",
        IceLogging.LogLevel.Info => "信息",
        // IceLogging.LogLevel.Verbose => "Verbose",
        IceLogging.LogLevel.Verbose => "详细",
        // IceLogging.LogLevel.Debug => "Debug",
        IceLogging.LogLevel.Debug => "调试",
        _ => level.ToString()
    };
}
