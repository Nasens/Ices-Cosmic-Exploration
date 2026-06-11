using Dalamud.Interface.Utility.Raii;
using ECommons.GameHelpers;
using ICE.Scheduler.Handlers;
using ICE.Utilities;
using ICE.Utilities.Cosmic_Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICE.Ui.MainUi.Settings.Settings_Table
{
    internal class DebugTab
    {
        public static void Draw()
        {
            ImGui.Checkbox("强制 OOM 主", ref SchedulerMain.DebugOOMMain);
            ImGui.Checkbox("强制 OOM 副", ref SchedulerMain.DebugOOMSub);

            if (ImGui.Button("获取当前中心天气预报"))
            {
                // Same fallback as other debug tabs: current hub, or Sinus when not in cosmic.
                var territoryId = PlayerHelper.IsInCosmicZone()
                    ? Player.Territory.RowId
                    : CosmicMoonRegistry.Sinus.TerritoryId;
                List<WeatherForecast> forecast = WeatherForecastHandler.GetTerritoryForecast((ushort)territoryId);
                Func<WeatherForecast, string> formatTime = (forecast) => WeatherForecastHandler.FormatForecastTime(forecast.Time);
                var hubName = CosmicMoonRegistry.GetDisplayName(territoryId);

                Svc.Chat.Print(new Dalamud.Game.Text.XivChatEntry()
                {
                    Message = $"{hubName} 天气 - {forecast[0].Name}",
                    Type = Dalamud.Game.Text.XivChatType.Echo,
                });
                for (int i = 1; i < forecast.Count; i++)
                {
                    Svc.Chat.Print(new Dalamud.Game.Text.XivChatEntry()
                    {
                        Message = $"{forecast[i].Name} 将在 {formatTime(forecast[i])} 后到来",
                        Type = Dalamud.Game.Text.XivChatType.Echo,
                    });
                }
            }

            using (ImRaii.Disabled(!PlayerHelper.IsInCosmicZone()))
            {
                if (ImGui.Button("刷新天气预报"))
                {
                    WeatherForecastHandler.GetForecast();
                }
            }
            bool gatherDebug = C.ShowDebugGatherInfo;
            if (ImGui.Checkbox("显示采集调试信息", ref gatherDebug))
            {
                C.ShowDebugGatherInfo = gatherDebug;
                C.Save();
            }

            bool highlightTable = C.HighlightVisibleMissions;
            if (ImGui.Checkbox("高亮可见任务", ref highlightTable))
            {
                C.HighlightVisibleMissions = highlightTable;
                C.Save();
            }

            bool onlyGrabMission = C.OnlyGrabMission_Debug;
            if (ImGui.Checkbox($"仅领取任务", ref onlyGrabMission))
            {
                C.OnlyGrabMission_Debug = onlyGrabMission;
                C.Save();
            }
        }
    }
}
