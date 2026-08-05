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
            // if (ImGui.Button("Run Drone Finder"))
            if (ImGui.Button("运行无人机搜索"))
            {
                SchedulerMain.State = IceState.ArtifactSearch;
            }

            // if (ImGui.Button("Stop"))
            if (ImGui.Button("停止"))
            {
                SchedulerMain.DisablePlugin();
            }

            bool buyDrones = C.Cosmodrone_Buy;
            // if (ImGui.Checkbox("Buy Drones", ref buyDrones))
            if (ImGui.Checkbox("购买无人机", ref buyDrones))
            {
                C.Cosmodrone_Buy = buyDrones;
                C.Save();
            }
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle, 
                // "Do you want to buy drones? If yes, enable this"
                "是否购买无人机？若是，请启用此项"
                );

            int drone_buyAtAmount = C.Cosmodrone_BuyAt;
            ImGui.SetNextItemWidth(200);
            // if (ImGui.SliderInt("Buy At Amount", ref drone_buyAtAmount, 200, 5000))
            if (ImGui.SliderInt("购买阈值", ref drone_buyAtAmount, 200, 5000))
            {
                drone_buyAtAmount = (int)Math.Round(drone_buyAtAmount / 200.0) * 200;
                C.Cosmodrone_BuyAt = drone_buyAtAmount;
                C.SaveDebounced();
            }
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle, 
                // "When do you wanna buy drones from the vendor?\n" +
                // "Set in incriments of 200, max of 5,000"
                "何时从商人处购买无人机？\n" +
                "以 200 为步进，最大 5,000"
                );

            int maxCrateAmount = C.Cosmodrone_MaxKeep;
            ImGui.SetNextItemWidth(200);
            // if (ImGui.InputInt("Maximum Drones", ref maxCrateAmount))
            if (ImGui.InputInt("无人机上限", ref maxCrateAmount))
            {
                if (maxCrateAmount < 0)
                    maxCrateAmount = 0;
                C.Cosmodrone_MaxKeep = maxCrateAmount;
                C.SaveDebounced();
            }
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle,
                // "What's the maximum amount of drones you wanna keep?\n" +
                // "0 = will just keep buying\n" +
                // "Anything above 0 will just be a hard cap and will stop buying if it reaches this"
                "最多保留多少无人机？\n" +
                "0 = 持续购买\n" +
                "大于 0 则为硬上限，达到后停止购买"
                );

            bool runDroneFinder = C.Cosmodrone_Run;
            // if (ImGui.Checkbox("Automate cosmodrone", ref runDroneFinder))
            if (ImGui.Checkbox("自动化宇宙无人机", ref runDroneFinder))
            {
                C.Cosmodrone_Run = runDroneFinder;
                C.Save();
            }
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle,
                // "Do you want to run the automated drone finding? If yes, enable this\n" +
                // "PLEASE NOTE. DO. NOT. LEAVE. THIS. ALONE. This is still being worked on heavily"
                "是否运行自动无人机搜索？若是，请启用此项\n" +
                "请注意：请勿无人值守运行，此功能仍在大量开发中"
                );
        }
    }
}
