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
            _ => "???"
        };
    }
}
