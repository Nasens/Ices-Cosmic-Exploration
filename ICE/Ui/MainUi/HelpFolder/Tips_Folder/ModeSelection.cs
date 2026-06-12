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
            // ImGui.TextWrapped("There is 5 different modes that exist currently (as of writing this) that all serve minorly differently functions.");
            ImGui.TextWrapped("目前（撰写本文时）共有 5 种模式，各自功能略有不同。");
            // ImGui.TextWrapped("Depending on what you want / what your goal is, these all serve all different functions.");
            ImGui.TextWrapped("根据你的目标，这些模式提供不同的功能。");

            if (ImGui.BeginTabBar("Mode Selection Info"))
            {
                // if (ImGui.BeginTabItem("Standard"))
                if (ImGui.BeginTabItem("标准模式"))
                {
                    StandardMode();
                    ImGui.EndTabItem();
                }

                // if (ImGui.BeginTabItem("Relic Grind"))
                if (ImGui.BeginTabItem("Relic 刷取"))
                {
                    RelicGrind();
                    ImGui.EndTabItem();
                }

                // if (ImGui.BeginTabItem("Agenda Mode"))
                if (ImGui.BeginTabItem("议程模式"))
                {
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }

        private static void StandardMode()
        {
            ImGui.Dummy(new(0, 5));
            // ImGuiEx.IconWithText(FontAwesomeIcon.List, "Standard");
            ImGuiEx.IconWithText(FontAwesomeIcon.List, "标准模式");
            ImGui.TextWrapped(
                // "The most straightforward mode. Standard runs only the missions you have enabled " +
                // "for your current class — you pick what you want done, and it handles the rest.");
                "最直观的模式。标准模式仅运行你为当前职业启用的任务——你选择要做什么，其余交给插件处理。");
            // ImGui.TextWrapped("This gives you full control over what missions to run, making it ideal for:");
            ImGui.TextWrapped("你可以完全控制要执行的任务，适合以下场景：");
            // ImGui.BulletText("Score Farming — select specific high-value missions");
            ImGui.BulletText("刷分数 — 选择特定高价值任务");
            // ImGui.BulletText("Exp Grinding");
            ImGui.BulletText("刷经验");
            // ImGui.BulletText("Credit / Planetary Credits / Token farming");
            ImGui.BulletText("刷点数 / 行星点数 / 代币");
            ImGui.TextWrapped(
                // "Basic missions (Ranks D→A) will only pull from the class you started on. " +
                // "Provisional missions (Weather, Timed, and Sequence) and Red Alerts will be picked up " +
                // "between missions — enable the multi-class setting to allow switching classes for these as well.");
                "基础任务（D→A 级）仅从起始职业中选取。" +
                "临时任务（天气、限时、序列）和紧急通告会在任务间隙接取——启用多职业设置后，这些任务也可切换职业。");
        }

        private static void RelicGrind()
        {
            ImGui.Dummy(new(0, 5));
            // ImGuiEx.IconWithText(FontAwesomeIcon.ArrowUpRightDots, "Relic Grind");
            ImGuiEx.IconWithText(FontAwesomeIcon.ArrowUpRightDots, "Relic 刷取");

            ImGui.TextWrapped(
                // "A mode designed to automate mission selection for relic progression with minimal intervention. " +
                // "It scans all available missions, evaluates the experience each one provides, and picks whichever " +
                // "yields the most for your current level.");
                "专为 Relic 进度自动化任务选择而设计的模式，干预最少。" +
                "它会扫描所有可用任务，评估每个任务提供的经验，并选择对当前等级收益最大的任务。");
            ImGui.TextWrapped(
                // "If you're high enough level to need the next rank category but haven't unlocked it yet " +
                // "(e.g. Rank D completed but Rank C not yet unlocked), make sure to unlock it manually before starting.");
                "若等级已够进入下一档但尚未解锁" +
                "（例如 D 级已完成但 C 级尚未解锁），请先手动解锁再启动。");
            // ImGui.TextWrapped("Note: this mode does not swap planets for you. Each planet also has an exp cap:");
            ImGui.TextWrapped("注意：此模式不会自动切换星球。每个星球也有经验上限：");
            // ImGui.BulletText("Sinus   — Rank IV Max");
            ImGui.BulletText("朔月湾 — 最高 IV 级");
            // ImGui.BulletText("Phaenna — Rank V Max");
            ImGui.BulletText("法恩娜 — 最高 V 级");
            // ImGui.BulletText("Oizys   — Rank VI Max");
            ImGui.BulletText("奥伊兹 — 最高 VI 级");
        }

        public static void GoldCompletion()
        {

        }
    }
}
