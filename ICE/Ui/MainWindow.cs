using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using ECommons.GameHelpers;
using ICE.Ui.MainUi;
using ICE.Ui.MainUi.HelpFolder;
using ICE.Ui.MainUi.ModeSelect_Modes;
using ICE.Ui.MainUi.Settings;
using ICE.Ui.MainUi.Settings.Settings_Table;
using System.Collections.Generic;
using System.Reflection;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace ICE.Ui
{
    internal class MainWindow : Window
    {
        public MainWindow() :
#if DEBUG
        // base($"Ice's Cosmic Exploration {P.GetType().Assembly.GetName().Version} [Debug Build] ###ICEMainWindow2")
        base($"Ice's Cosmic Exploration {P.GetType().Assembly.GetName().Version} [调试版] ###ICEMainWindow2")
#else
        base($"Ice's Cosmic Exploration {P.GetType().Assembly.GetName().Version} ###ICEMainWindow2")
#endif
        {
            Flags = ImGuiWindowFlags.NoScrollbar;
            SizeConstraints = new()
            {
                MinimumSize = new Vector2(500, 500),
                MaximumSize = new Vector2(4000, 4000),
            };
            TitleBarButtons.Add(new() { ShowTooltip = () => ImGui.SetTooltip("♥ Ko-fi (Buy me an ice coffee)"), Icon = FontAwesomeIcon.Heart, IconOffset = new(1, 1), Click = _ => GenericHelpers.ShellStart("https://ko-fi.com/ice643269") });

            P.windowSystem.AddWindow(this);

            AllowPinning = true;
            AllowClickthrough = true;
        }

        public void Dispose()
        {
            P.windowSystem.RemoveWindow(this);
        }

        public override void Draw()
        {
            using var style = ImRaii.PushStyle(ImGuiStyleVar.ChildRounding, 10).Push(ImGuiStyleVar.ChildBorderSize, 1);

            SelectableSidebar.Draw();

            ImGui.SameLine(0, 5);

            var windowSizeRemaining = ImGui.GetContentRegionAvail();
            using (var mainBody = ImRaii.Child("mainBody_WindowV3", windowSizeRemaining, true))
            {
                if (!mainBody.Success) return;
                MainBody();
            }
        }

        private static readonly Dictionary<WindowSelection, Action> SelectedView = new()
        {
            // Cosmic Helper
            [WindowSelection.MissionSetup] = () =>  Mission_Setup.Draw(),
            [WindowSelection.CosmicAgenda] = () => Cosmic_Agenda.Draw(),
            [WindowSelection.ExpeditionLogs] = () => Expedition_Log.Draw(),

            // Settings
            [WindowSelection.StopWhen] = () => StopWhen.Draw(),
            [WindowSelection.GatheringProfiles] = () => GatherSettings.Draw(),
            [WindowSelection.MissionPriority] = () => Priority_Settings.Draw(),
            [WindowSelection.CharacterSettings] = () => Character_Settings.Draw(),
            [WindowSelection.MiscSettings] = () => Misc_Settings.Draw(),
            [WindowSelection.TravelSettings] = () => TravelSettings.Draw(),

            // Hub Activities
            [WindowSelection.CreditShopping] = () => ShoppingTab.Draw(),
            [WindowSelection.GambaShopping] = () => GambaWheel.Draw(),
            [WindowSelection.DroneShopping] = () => Shop_Dronebit.Draw(),

            // Help Section
            [WindowSelection.Plugin_Install] = () => helpSelect_Required.Draw(),
            [WindowSelection.Plugin_Logs] = () => helpSelect_Logs.Draw_Helper(),
            [WindowSelection.Plugin_Tips] = () => helpSelect_Tips.Draw(),
        };

        private static void MainBody()
        {
            var selectedWindow = C.SelectedTab;

            if (SelectedView.TryGetValue(selectedWindow, out var drawAction))
            {
                drawAction();
            }
            else
            {
                ImGui.Text("Hehe");
            }
        }

        public static void ModeSelection()
        {
            bool standard = C.SelectedMode == ModeSelect.Standard;
            bool relicMode = C.SelectedMode == ModeSelect.RelicMode;
            bool xpLeveling = C.SelectedMode == ModeSelect.LevelMode;
            bool goldMode = C.SelectedMode == ModeSelect.MissionGoldMode;
            bool agendaMode = C.SelectedMode == ModeSelect.AgendaMode;

            // ImGui.Text("Select Mode");
            ImGui.Text("选择模式");
            ImGui.Separator();

            // if (ImGui.RadioButton("Standard", standard))
            if (ImGui.RadioButton("标准模式", standard))
            {
                C.SelectedMode = ModeSelect.Standard;
                C.Save();
            }
            ImGuiEx.HelpMarker(HelpInfoText(ModeSelect.Standard));
            // if (ImGui.RadioButton("Relic Grind", relicMode))
            if (ImGui.RadioButton("Relic 刷取模式", relicMode))
            {
                C.SelectedMode = ModeSelect.RelicMode;
                C.Save();
            }
            ImGuiEx.HelpMarker(HelpInfoText(ModeSelect.RelicMode));

            // if (ImGui.RadioButton("Leveling Grind", xpLeveling))
            if (ImGui.RadioButton("练级模式", xpLeveling))
            {
                C.SelectedMode = ModeSelect.LevelMode;
                C.Save();
            }
            ImGuiEx.HelpMarker(HelpInfoText(ModeSelect.LevelMode));
            // if (ImGui.RadioButton("Gold Completion Grind", goldMode))
            if (ImGui.RadioButton("金牌完成模式", goldMode))
            {
                C.SelectedMode = ModeSelect.MissionGoldMode;
                C.Save();
            }
            ImGuiEx.HelpMarker(HelpInfoText(ModeSelect.MissionGoldMode));
            // if (ImGui.RadioButton("Agenda Mode", agendaMode))
            if (ImGui.RadioButton("议程模式", agendaMode))
            {
                C.SelectedMode = ModeSelect.AgendaMode;
                C.Save();
            }
            ImGuiEx.HelpMarker(HelpInfoText(ModeSelect.AgendaMode));
        }
        public static string HelpInfoText(ModeSelect mode)
        {
            return mode switch
            {
                ModeSelect.Standard =>
                    // "Stand Mode \n" +
                    "标准模式 \n" +
                    // "-> Used to select which missions you want to grind. It'll priortize in the following order:\n" +
                    "-> 用于选择要刷取的任务，按以下优先级排序：\n" +
                    // "-> Critical -> Provisional [Sequence/Timed/Weather] -> Standard [A->D]\n" +
                    "-> 紧急 -> 临时 [序列/限时/天气] -> 标准 [A->D]\n" +
                    // "-> Select which missions you want to do, and go at it.",
                    "-> 选择要执行的任务，然后开始刷取。",
                ModeSelect.LevelMode =>
                    // "Leveling Grind\n" +
                    "练级模式\n" +
                    // "-> Will automatically select which mission is the best for leveling your current class based on what level bracket you're in\n" +
                    "-> 根据当前等级区间，自动选择最适合当前职业的练级任务\n" +
                    // "-> These are hand picked by me, and determined by the time it takes to complete it\n" +
                    "-> 这些任务经过人工筛选，以完成时间为依据\n" +
                    // "-> For crafters it's whatever missions take the least amount of progress" +
                    "-> 对制作职业而言，选择进度需求最少的任务" +
                    // "-> For gathering, it's whatever is the least pain to do w/ the minimum amount of skills\n" +
                    "-> 对采集职业而言，选择技能需求最少、最省力的任务\n" +
                    // "**These will automatically set settings for using these modes temporarily**",
                    "**使用这些模式时会自动临时调整相关设置**",
                ModeSelect.RelicMode =>
                    // "Relic Grind\n" +
                    "Relic 刷取模式\n" +
                    // "-> Automatically select which missions that are best to finish up your relic\n" +
                    "-> 自动选择最适合完成 Relic 进度的任务\n" +
                    // "-> These are weighed based on what is needed to complete the tool to the next step\n" +
                    "-> 根据工具升级所需材料进行权重计算\n" +
                    // "-> If you want to only do certain missions, enable the option and select which ones you want to do",
                    "-> 若只想执行特定任务，请启用选项并勾选目标任务",
                ModeSelect.AgendaMode =>
                    // "This mode is if you want to do a series of things in a particular order. So for example, if you wanted to grind out all the relics on all the classes back to back\n" +
                    "此模式用于按特定顺序执行一系列操作。例如，你想连续刷完所有职业的 Relic\n" +
                    // "Or if you wanted to do the relic on WVR -> Then farm score on BTN -> Farm credits on BSM\n" +
                    "或者先刷 WVR 的 Relic -> 再在 BTN 上刷分数 -> 最后在 BSM 上刷点数\n" +
                    // "Really is the \"I want to do this order of things\" kind of thing.\n" +
                    "本质上就是「我想按这个顺序做事」的模式。\n" +
                    // "Note. I'm not responsible if you leave this on and get banned for it. I'm not one for leaving things at their pc, but people are watching always. Keep this in mind",
                    "注意：请勿无人值守运行，存在封号风险。请自行承担后果。",
                ModeSelect.MissionGoldMode =>
                    // "Gold Completion Mode\n" +
                    "金牌完成模式\n" +
                    // "-> Will automatically pick all the missions that you do not have currently gold, AND ONLY THOSE MISSIONS.\n" +
                    "-> 自动选择所有尚未获得金牌的任务，且仅执行这些任务。\n" +
                    // "-> If it is apart of a sequence chain, it will grab the mission that are needed previously to help complete it, and the missions post if necessary\n" +
                    "-> 若属于序列任务链，会自动接取前置任务及必要后续任务\n" +
                    // "-> If it runs out of missions to reroll, it will just continually swap tabs until the mission is available (via provisional or critical)\n" +
                    "-> 若无任务可刷新，将持续切换标签页直到任务可用（通过临时或紧急任务）\n" +
                    // "**This will respect the want to grind off class provisionals, and criticals if you have those enabled",
                    "**若已启用，将尊重跨职业临时任务与紧急任务的设置**",
                _ => "???? 未知模式，请向作者反馈"
            };
        }
    }
}
