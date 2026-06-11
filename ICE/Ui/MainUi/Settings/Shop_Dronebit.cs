using Dalamud.Interface;
using ICE.Utilities.ImGuiTools;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.Ui.MainUi.Settings
{
    internal class Shop_Dronebit
    {
        public static void Draw()
        {
            if (ImGui.Button("运行无人机搜寻"))
            {
                SchedulerMain.State = IceState.ArtifactSearch;
            }

            if (ImGui.Button("停止"))
            {
                SchedulerMain.DisablePlugin();
            }

            bool buyDrones = C.Cosmodrone_Buy;
            if (ImGui.Checkbox("购买无人机", ref buyDrones))
            {
                C.Cosmodrone_Buy = buyDrones;
                C.Save();
            }
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle, 
                "你想购买无人机吗？如果想，请启用此项"
                );

            int drone_buyAtAmount = C.Cosmodrone_BuyAt;
            ImGui.SetNextItemWidth(200);
            if (ImGui.SliderInt("达到此数量时购买", ref drone_buyAtAmount, 200, 5000))
            {
                drone_buyAtAmount = (int)Math.Round(drone_buyAtAmount / 200.0) * 200;
                C.Cosmodrone_BuyAt = drone_buyAtAmount;
                C.SaveDebounced();
            }
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle, 
                "你想在什么时候从商人处购买无人机？\n" +
                "以 200 为增量设置，最大 5,000"
                );

            int maxCrateAmount = C.Cosmodrone_MaxKeep;
            ImGui.SetNextItemWidth(200);
            if (ImGui.InputInt("最大无人机数量", ref maxCrateAmount))
            {
                if (maxCrateAmount < 0)
                    maxCrateAmount = 0;
                C.Cosmodrone_MaxKeep = maxCrateAmount;
                C.SaveDebounced();
            }
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle,
                "你想最多保留多少架无人机？\n" +
                "0 = 将一直持续购买\n" +
                "大于 0 的数值则作为硬性上限，达到后将停止购买"
                );

            bool runDroneFinder = C.Cosmodrone_Run;
            if (ImGui.Checkbox("自动化 cosmodrone", ref runDroneFinder))
            {
                C.Cosmodrone_Run = runDroneFinder;
                C.Save();
            }
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle,
                "你想运行自动无人机搜寻吗？如果想，请启用此项\n" +
                "请注意。千。万。别。无。人。看。管。此功能仍在大力开发中");
        }
    }
}
