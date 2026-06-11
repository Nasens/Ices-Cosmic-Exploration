using Dalamud.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICE.Ui.MainUi.Settings
{
    internal class SafetySettings
    {
        private static bool rejectUnknownYesNo = C.RejectUnknownYesno;
        private static bool delayGrabMission = C.DelayGrabMission;
        private static int delayAmount = C.DelayIncrease;
        private static bool delayCraft = C.DelayCraft;
        private static int delayCraftAmount = C.DelayCraftIncrease;

        public static void Draw()
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.ExclamationTriangle, "安全设置");
            ImGui.Dummy(new Vector2(0, 5));

            if (ImGui.Checkbox("忽略非 Cosmic 的提示框", ref rejectUnknownYesNo))
            {
                C.RejectUnknownYesno = rejectUnknownYesNo;
                C.Save();
            }
            ImGuiEx.HelpMarker(
                "警告！这是一项安全功能，用于避免加入随机小队！\n" +
                "如果你取消勾选此项，你将会加入随机的组队邀请。\n" +
                "已经警告过你了。禁用风险自负。"
            );
            if (ImGui.Checkbox("为任务菜单添加延迟", ref delayGrabMission))
            {
                C.DelayGrabMission = delayGrabMission;
                C.Save();
            }
            ImGuiEx.HelpMarker(
                "这是为了安全！如果你想缩短任务之间的延迟，请随意。\n" +
                "安全值大约是……250？如果你遇到动作锁定，完全可以调得更高\n" +
                "或者你想冒险一下，就调低它。我又不是你爸（不过会讲冷笑话。");
            if (delayGrabMission)
            {
                ImGui.SetNextItemWidth(150);
                ImGui.SameLine();
                if (ImGui.SliderInt("ms###Mission", ref delayAmount, 0, 1000))
                {
                    if (C.DelayIncrease != delayAmount)
                    {
                        C.DelayIncrease = delayAmount;
                        C.SaveDebounced();
                    }
                }
            }
            if (ImGui.Checkbox("为制作菜单添加延迟", ref delayCraft))
            {
                C.DelayCraft = delayCraft;
                C.Save();
            }
            ImGuiEx.HelpMarker(
                "这是为了安全！如果你想缩短交付前的延迟，请随意。\n" +
                "安全值大约是……2500？如果你遇到动作锁定，完全可以调得更高\n" +
                "或者你想冒险一下，就调低它。我又不是你爸（不过会讲冷笑话。");
            if (delayCraft)
            {
                ImGui.SetNextItemWidth(150);
                ImGui.SameLine();
                if (ImGui.SliderInt("ms###Crafting", ref delayCraftAmount, 500, 5000))
                {
                    if (C.DelayCraftIncrease != delayCraftAmount)
                    {
                        C.DelayCraftIncrease = delayCraftAmount;
                        C.SaveDebounced();
                    }
                }
            }
            int delayRelic = C.DelayPostRelic;
            ImGui.SetNextItemWidth(150);
            if (ImGui.SliderInt("Relic 交付后延迟", ref delayRelic, 0, 5000))
            {
                C.DelayPostRelic = delayRelic;
                C.SaveDebounced();
            }
            bool gatherDelay = C.Delay_Gather;
            if (ImGui.Checkbox("为采集添加延迟", ref gatherDelay))
            {
                C.Delay_Gather = gatherDelay;
                C.Save();
            }
            bool closeRewardPopup = C.HideRewardWindow;
            if (ImGui.Checkbox("自动关闭奖励弹窗", ref closeRewardPopup))
            {
                C.HideRewardWindow = closeRewardPopup;
                C.Save();
            }
        }
    }
}
