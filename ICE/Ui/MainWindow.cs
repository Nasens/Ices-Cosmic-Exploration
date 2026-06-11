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
        base($"Ice's Cosmic Exploration {P.GetType().Assembly.GetName().Version} [Debug Build] ###ICEMainWindow2")
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
            TitleBarButtons.Add(new() { ShowTooltip = () => ImGui.SetTooltip("♥ Ko-fi（请我喝杯冰咖啡）"), Icon = FontAwesomeIcon.Heart, IconOffset = new(1, 1), Click = _ => GenericHelpers.ShellStart("https://ko-fi.com/ice643269") });

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
                ImGui.Text("嘿嘿");
            }
        }

        public static void ModeSelection()
        {
            bool standard = C.SelectedMode == ModeSelect.Standard;
            bool relicMode = C.SelectedMode == ModeSelect.RelicMode;
            bool xpLeveling = C.SelectedMode == ModeSelect.LevelMode;
            bool goldMode = C.SelectedMode == ModeSelect.MissionGoldMode;
            bool agendaMode = C.SelectedMode == ModeSelect.AgendaMode;

            ImGui.Text("选择模式");
            ImGui.Separator();

            if (ImGui.RadioButton("标准", standard))
            {
                C.SelectedMode = ModeSelect.Standard;
                C.Save();
            }
            ImGuiEx.HelpMarker(HelpInfoText(ModeSelect.Standard));
            if (ImGui.RadioButton("Relic 刷取", relicMode))
            {
                C.SelectedMode = ModeSelect.RelicMode;
                C.Save();
            }
            ImGuiEx.HelpMarker(HelpInfoText(ModeSelect.RelicMode));

            if (ImGui.RadioButton("练级刷取", xpLeveling))
            {
                C.SelectedMode = ModeSelect.LevelMode;
                C.Save();
            }
            ImGuiEx.HelpMarker(HelpInfoText(ModeSelect.LevelMode));
            if (ImGui.RadioButton("金牌完成刷取", goldMode))
            {
                C.SelectedMode = ModeSelect.MissionGoldMode;
                C.Save();
            }
            ImGuiEx.HelpMarker(HelpInfoText(ModeSelect.MissionGoldMode));
            if (ImGui.RadioButton("日程模式", agendaMode))
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
                    "标准模式 \n" +
                    "-> 用于选择你想刷取的任务。它会按以下顺序优先处理：\n" +
                    "-> Critical -> Provisional [序列/限时/天气] -> 标准 [A->D]\n" +
                    "-> 选择你想做的任务，然后开干。",
                ModeSelect.LevelMode =>
                    "练级刷取\n" +
                    "-> 会根据你当前所处的等级区间，自动选择最适合为当前职业练级的任务\n" +
                    "-> 这些都是我手动挑选的，依据完成所需的时间来决定\n" +
                    "-> 对于制作职业，选择消耗进展最少的任务" +
                    "-> 对于采集职业，选择用最少技能、最省事就能完成的任务\n" +
                    "**这些模式会临时自动设置相关选项**",
                ModeSelect.RelicMode =>
                    "Relic 刷取\n" +
                    "-> 自动选择最适合完成你 Relic 的任务\n" +
                    "-> 这些任务会根据将工具推进到下一阶段所需的内容来权衡\n" +
                    "-> 如果你只想做特定任务，启用该选项并选择你想做的任务",
                ModeSelect.AgendaMode =>
                    "如果你想按特定顺序做一系列事情，就用这个模式。比如，你想接连刷完所有职业的 Relic\n" +
                    "或者你想先做 WVR 的 Relic -> 然后用 BTN 刷分 -> 再用 BSM 刷点数\n" +
                    "本质上就是\"我想按这个顺序做这些事\"的模式。\n" +
                    "注意。如果你一直开着它而因此被封号，我概不负责。我不赞成把任务挂在电脑前不管，但总会有人盯着的。请牢记这一点",
                ModeSelect.MissionGoldMode =>
                    "金牌完成模式\n" +
                    "-> 会自动选择所有你当前尚未获得金牌的任务，且仅选择这些任务。\n" +
                    "-> 如果它属于某个序列链，会抓取此前需要完成的任务来帮助完成它，必要时也会抓取后续任务\n" +
                    "-> 如果没有可重抽的任务了，它会不断切换标签页，直到该任务可用（通过 provisional 或 critical）\n" +
                    "**如果你启用了相关选项，这会尊重你刷取非本职 provisional 和 critical 的需求",
                _ => "???? 不知为何这里缺失了内容。请把此问题报告给 I"
            };
        }
    }
}
