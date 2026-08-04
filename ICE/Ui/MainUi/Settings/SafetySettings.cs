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
            // ImGuiEx.IconWithText(FontAwesomeIcon.ExclamationTriangle, "Safety Settings");
            ImGuiEx.IconWithText(FontAwesomeIcon.ExclamationTriangle, "安全设置");
            ImGui.Dummy(new Vector2(0, 5));

            // if (ImGui.Checkbox("Ignore non-Cosmic prompts", ref rejectUnknownYesNo))
            if (ImGui.Checkbox("忽略非 Cosmic 提示框", ref rejectUnknownYesNo))
            {
                C.RejectUnknownYesno = rejectUnknownYesNo;
                C.Save();
            }
            ImGuiEx.HelpMarker(
                // "Warning! This is a safety feature to avoid joining random parties!\n" +
                // "If you you uncheck this, YOU WILL JOIN random party invites.\n" +
                // "You have been warned. Disable at your own risk."
                "警告！这是一项安全功能，用于避免加入随机队伍！\n" +
                "若取消勾选，你将加入随机队伍邀请。\n" +
                "后果自负，请谨慎关闭。"
            );
            // if (ImGui.Checkbox("Add delay to mission menu", ref delayGrabMission))
            if (ImGui.Checkbox("任务菜单增加延迟", ref delayGrabMission))
            {
                C.DelayGrabMission = delayGrabMission;
                C.Save();
            }
            ImGuiEx.HelpMarker(
                // "This is here for safety! If you want to decrease the delay between missions be my guest.\n" +
                // "Safety is around... 250? If you're having animation locks you can absolutely increase it higher\n" +
                // "Or if you're feeling daredevil. Lower it. I'm not your dad (will tell dad jokes though)."
                "出于安全考虑而设！可自行缩短任务间隔延迟。\n" +
                "建议值约为 250。若遇动画锁定可适当调高，\n" +
                "胆大的也可以调低。我不是你爸（但会讲冷笑话）。"
            );
            if (delayGrabMission)
            {
                ImGui.SetNextItemWidth(150);
                ImGui.SameLine();
                // if (ImGui.SliderInt("ms###Mission", ref delayAmount, 0, 1000))
                if (ImGui.SliderInt("毫秒###Mission", ref delayAmount, 0, 1000))
                {
                    if (C.DelayIncrease != delayAmount)
                    {
                        C.DelayIncrease = delayAmount;
                        C.SaveDebounced();
                    }
                }
            }
            // if (ImGui.Checkbox("Add delay to crafting menu", ref delayCraft))
            if (ImGui.Checkbox("制作菜单增加延迟", ref delayCraft))
            {
                C.DelayCraft = delayCraft;
                C.Save();
            }
            ImGuiEx.HelpMarker(
                // "This is here for safety! If you want to decrease the delay before turnin be my guest.\n" +
                // "Safety is around... 2500? If you're having animation locks you can absolutely increase it higher\n" +
                // "Or if you're feeling daredevil. Lower it. I'm not your dad (will tell dad jokes though)."
                "出于安全考虑而设！可自行缩短交付前延迟。\n" +
                "建议值约为 2500。若遇动画锁定可适当调高，\n" +
                "胆大的也可以调低。我不是你爸（但会讲冷笑话）。"
            );
            if (delayCraft)
            {
                ImGui.SetNextItemWidth(150);
                ImGui.SameLine();
                // if (ImGui.SliderInt("ms###Crafting", ref delayCraftAmount, 500, 5000))
                if (ImGui.SliderInt("毫秒###Crafting", ref delayCraftAmount, 500, 5000))
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
            // if (ImGui.SliderInt("Delay Post Relic Turnin", ref delayRelic, 0, 5000))
            if (ImGui.SliderInt("Relic 交付后延迟", ref delayRelic, 0, 5000))
            {
                C.DelayPostRelic = delayRelic;
                C.SaveDebounced();
            }
            bool gatherDelay = C.Delay_Gather;
            // if (ImGui.Checkbox("Add delay to gather", ref gatherDelay))
            if (ImGui.Checkbox("采集增加延迟", ref gatherDelay))
            {
                C.Delay_Gather = gatherDelay;
                C.Save();
            }
            bool closeRewardPopup = C.HideRewardWindow;
            // if (ImGui.Checkbox("Auto Close Reward Popups", ref closeRewardPopup))
            if (ImGui.Checkbox("自动关闭奖励弹窗", ref closeRewardPopup))
            {
                C.HideRewardWindow = closeRewardPopup;
                C.Save();
            }
        }
    }
}
