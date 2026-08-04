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

                // if (ImGui.BeginTabItem("Leveling Mode"))
                if (ImGui.BeginTabItem("练级模式"))
                {
                    LevelingMode();
                    ImGui.EndTabItem();
                }

                // if (ImGui.BeginTabItem("Gold Completion Grind"))
                if (ImGui.BeginTabItem("金牌完成模式"))
                {
                    GoldCompletion();
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
            // ImGui.BulletText("Auxesia - Rank VII Max");
            ImGui.BulletText("Auxesia — 最高 VII 级");
            // ImGui.TextWrapped("So make sure that you're on the correct planet to accomodate for the exp that you need" +
            //     "and to allow for completion your relic.");
            ImGui.TextWrapped("请务必待在正确的星球上，以满足所需经验" +
                "并完成你的 Relic。");
        }

        public static void GoldCompletion()
        {
            ImGui.Dummy(new(0, 5));
            // ImGuiEx.IconWithText(FontAwesomeIcon.Trophy, "Gold Completion");
            ImGuiEx.IconWithText(FontAwesomeIcon.Trophy, "金牌完成");

            // ImGui.TextWrapped(
            //     "A very direct mode of aiming to get a gold completion of every single mission that is not currently gold-completed." +
            //     "Removed the need of selecting each mission that you want to do, and will automatically pick->choose the missions based on" +
            //     "the priority that you currently have set.");
            ImGui.TextWrapped(
                "一种非常直接的模式，目标是把每一项尚未金牌完成的任务全部刷到金牌。" +
                "无需手动选择任务，插件会根据你当前设置的优先级自动挑选任务。");
            // ImGui.TextWrapped("PLEASE NOTE: that this has no other internal logic. It has no way to tell it can't do the mission due to either" +
            //     "a set of missing stats, no food... ect. This is just meant to be the most direct \"auto select missions that it can do\"." +
            //     "If you want some control over WHICH missions that you know you can do, select standard mode and choose the missions that need completed");
            ImGui.TextWrapped("请注意：此模式没有其他内部逻辑。它无法判断任务是否因属性不足、没有食物等原因而无法完成。" +
                "它只是最直接的「自动选择能做的任务」。" +
                "若想控制具体选择哪些任务，请使用标准模式并勾选需要完成的任务");
            // ImGui.TextWrapped("This mode also respects the settings of being able to grind off class provisionals");
            ImGui.TextWrapped("此模式同样遵循跨职业刷临时任务的相关设置");

        }

        public static void LevelingMode()
        {
            ImGui.Dummy(new(0, 5));
            // ImGuiEx.IconWithText(FontAwesomeIcon.Leaf, "Leveling Mode");
            ImGuiEx.IconWithText(FontAwesomeIcon.Leaf, "练级模式");

            // ImGui.TextWrapped("A mode designed around selecting the best missions that give both the most experience, while also" +
            //     "choosing the missions that can be done the quickest. These are all completed on bronze completion (aka the fastest you can complete a mission)" +
            //     "because experience doesn't scale off of the level of turnin. Meaning if a mission gives 125% exp, it'll alwayws give that");
            ImGui.TextWrapped("该模式旨在选择经验收益最高、同时完成速度最快的任务。" +
                "任务均以铜牌完成（即最快完成方式）" +
                "因为经验不随交付等级变化——任务给 125% 经验，就始终给 125%。");

            ImGui.Dummy(new(0, 5));
            // ImGui.Text("Crafters");
            ImGui.Text("制作职业");
            // ImGui.BulletText("Missions are selected with the lowest progress");
            ImGui.BulletText("选择进度最低的任务");
            // ImGui.BulletText("Quality DOES NOT MATTER for these");
            ImGui.BulletText("这些任务的品质无关紧要");
            // ImGui.BulletText("YOU WILL NEED TO GO UNLOCK COLLECTABLES IN MOR DHONA AT LV. 50 IF YOU HAVEN'T ALREADY");
            ImGui.BulletText("若尚未解锁，需在 50 级时前往摩杜纳解锁收藏品");
            // ImGui.BulletText("You can get away with leveling up your gear at the following levels if you really wanna be stingy like me:");
            ImGui.BulletText("如果你也想和我一样省事，以下等级再更换装备完全够用：");
            // ImGui.BulletText("Lv. 10 -> 35 -> 52 -> 80");
            ImGui.BulletText("Lv. 10 -> 35 -> 52 -> 80");
            // ImGui.BulletText("These are the points where I found i could just get away with, if you want to make it go faster absolutely can grab gear more often between but.");
            ImGui.BulletText("这些是我实测可以凑合的节点；若想更快，也可以更频繁地更换装备。");

            ImGui.Dummy(new(0, 5));
            // ImGui.Text("Gathering");
            ImGui.Text("采集职业");
            // ImGui.BulletText("A bit more tedious, and defitenly not the fastest, but it's the safest so far.");
            ImGui.BulletText("稍显繁琐，肯定不是最快的，但至今最稳妥。");
            // ImGui.BulletText("Fisher has profiles already built into the plugin, and Btn/Min will auto set profiles to be able to turnin missions ASAP");
            ImGui.BulletText("捕鱼已有插件内置配置，BTN/MIN 会自动设置配置以尽快交付任务");
            // ImGui.BulletText("You NEED to get gear more often here than crafters, about every 5-7 levels below lv 50, then about every 3 levels after");
            ImGui.BulletText("这里比制作更需要频繁更新装备：50 级前约每 5-7 级换一次，之后约每 3 级一次");
        }

        public static void CosmicAgenda()
        {
            ImGui.Dummy(new(0, 5));
            // ImGuiEx.IconWithText(FontAwesomeIcon.ClipboardList, "Cosmic Agenda | Agenda Mode");
            ImGuiEx.IconWithText(FontAwesomeIcon.ClipboardList, "宇宙议程 | 议程模式");

            // ImGui.TextWrapped("The mode to help combine (most) of the other modes into one little playlist so you can set and forget." +
            //     "The purpose of this is to allow you to organize when and what order you want to do things");
            ImGui.TextWrapped("该模式将（大部分）其他模式整合进一个简易播放列表，设置好后即可放手。" +
                "目的在于让你安排执行的时间与先后顺序");
            // ImGui.TextWrapped("This includes but not limited to:");
            ImGui.TextWrapped("包括但不限于：");
            // ImGui.BulletText("Leveling selected classes to to a specific level");
            ImGui.BulletText("将所选职业练到指定等级");
            // ImGui.BulletText("Farming specific classes scores to 500k");
            ImGui.BulletText("将指定职业分数刷到 500k");
            // ImGui.BulletText("Completed relics on all classes");
            ImGui.BulletText("完成所有职业的 Relic");
            // ImGui.TextWrapped("You can specify the modes that you want to use these from, and it will run continue attempting to do that class until that objective is complete." +
            //     "This will respect any setting that you currently have enabled, it's jsut a fancy way of letting you the user choose what to do");
            ImGui.TextWrapped("可指定使用哪些模式来完成目标，插件会持续运行该职业直至目标达成。" +
                "它会尊重你当前启用的所有设置，只是用更灵活的方式让你自行决定做什么");
        }
    }
}
