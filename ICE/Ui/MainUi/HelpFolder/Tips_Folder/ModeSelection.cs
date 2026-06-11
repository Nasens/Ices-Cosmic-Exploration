using Dalamud.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.Ui.MainUi.HelpFolder.Tips_Folder
{
    internal class ModeSelection
    {
        public static void Draw()
        {
            ImGui.TextWrapped("目前（截至撰写本文时）共有 5 种不同的模式，它们的功能略有差异。");
            ImGui.TextWrapped("根据你的需求/目标不同，这些模式各自有不同的用途。");

            if (ImGui.BeginTabBar("Mode Selection Info"))
            {
                if (ImGui.BeginTabItem("标准"))
                {
                    StandardMode();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Relic 刷取"))
                {
                    RelicGrind();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("日程模式"))
                {
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }

        private static void StandardMode()
        {
            ImGui.Dummy(new(0, 5));
            ImGuiEx.IconWithText(FontAwesomeIcon.List, "标准");
            ImGui.TextWrapped(
                "最直接的模式。标准模式只运行你为当前职业启用的任务——" +
                "你来挑选想完成的任务，剩下的交给它处理。");
            ImGui.TextWrapped("这让你能够完全掌控要运行哪些任务，因此特别适合：");
            ImGui.BulletText("评分刷取——选择特定的高价值任务");
            ImGui.BulletText("经验刷取");
            ImGui.BulletText("点数 / 星球点数 / 代币刷取");
            ImGui.TextWrapped(
                "基础任务（D→A 级）只会从你开始时所用的职业中选取。" +
                "临时任务（天气、限时和序列）以及红色警报会在任务之间被选取——" +
                "启用多职业设置即可允许为这些任务切换职业。");
        }

        private static void RelicGrind()
        {
            ImGui.Dummy(new(0, 5));
            ImGuiEx.IconWithText(FontAwesomeIcon.ArrowUpRightDots, "Relic 刷取");

            ImGui.TextWrapped(
                "一个旨在以最少干预自动选择任务以推进 Relic 进度的模式。" +
                "它会扫描所有可用任务，评估每个任务提供的经验，并选取" +
                "对你当前等级收益最高的那个。");
            ImGui.TextWrapped(
                "如果你的等级已经足够高，需要下一个等级类别但尚未解锁它" +
                "（例如已完成 D 级但 C 级尚未解锁），请务必在开始前手动解锁它。");
            ImGui.TextWrapped("注意：此模式不会替你切换星球。每个星球还有经验上限：");
            ImGui.BulletText("Sinus   — 最高 IV 级");
            ImGui.BulletText("Phaenna — 最高 V 级");
            ImGui.BulletText("Oizys   — 最高 VI 级");
        }

        public static void GoldCompletion()
        {

        }
    }
}
