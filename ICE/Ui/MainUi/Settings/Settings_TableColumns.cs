using Dalamud.Interface.Utility.Raii;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.ImGuiTools;

namespace ICE.Ui.MainUi.Settings;

public static class Settings_TableColumns
{
    private static string[] missionSortOptions = 
        // ["Id", "Name", "Cosmo Credits", "Lunar Credits", 
        // "Exp I", "Exp II", "Exp III", "Exp IV", "Exp V", 
        // "Map Location", "Class Score", "Class Exp"];
        ["ID", "名称", "宇宙点数", "行星点数", 
        "经验 I", "经验 II", "经验 III", "经验 IV", "经验 V", 
        "地图位置", "职业分数", "职业经验"];

    public static void ColumnSettings()
    {
        int missionSelectedOption = C.TableSortOption;
        // if (ImGui.BeginCombo("Sort By", missionSortOptions[missionSelectedOption]))
        if (ImGui.BeginCombo("排序方式", missionSortOptions[missionSelectedOption]))
        {
            for (int i = 0; i < missionSortOptions.Length; i++)
            {
                bool isSelected = (i == missionSelectedOption);
                if (ImGui.Selectable(missionSortOptions[i], isSelected))
                {
                    missionSelectedOption = i;
                }
                if (isSelected)
                {
                    ImGui.SetItemDefaultFocus();
                }
                if (missionSelectedOption != C.TableSortOption)
                {
                    C.TableSortOption = missionSelectedOption;
                    C.Save();
                }
            }
            ImGui.EndCombo();
        }

        bool hideUnsupported = C.HideUnsupportedMissions;
        // if (ImGui.Checkbox("Hide Unsupported Missions", ref hideUnsupported))
        if (ImGui.Checkbox("隐藏不支持的任务", ref hideUnsupported))
        {
            C.HideUnsupportedMissions = hideUnsupported;
            C.Save();
        }

        bool autoShowToken = C.Auto_ShowTokens;
        // if (ImGui.Checkbox("Auto Hide/Show Planet Tokens", ref autoShowToken))
        if (ImGui.Checkbox("自动隐藏/显示行星代币", ref autoShowToken))
        {
            C.Auto_ShowTokens = autoShowToken;
            C.Save();
        }

        ImGuiEx.HelpMarker(
            // "Only enable this if you want plan on doing missions YOURSELF. AND NOT AUTOMATING IT. " +
            // "Or if you're letting a different plugin do all the automating of turning in, craftings, gathering... and not letting I.C.E. handle interacting with those plugins"
            "仅在你打算手动做任务、不依赖自动化时启用。" +
            "或当你让其他插件负责交付/制作/采集，而不让 ICE 与这些插件交互时启用。"
        );
    }

    private static bool ApplyToAllClasses = true;
    private static bool ApplyToSpecicClass = false;
    private static int SpecificClass = 8;
    private static int selectedClassIndex = 0;

    private static readonly string[] classOptions = new[]
    {
        // "Carpenter (CRP)",      // 0
        // "Blacksmith (BSM)",     // 1
        // "Armorer (ARM)",        // 2
        // "Goldsmith (GSM)",      // 3
        // "Leatherworker (LTW)",  // 4
        // "Weaver (WVR)",         // 5
        // "Alchemist (ALC)",      // 6
        // "Culinarian (CUL)",     // 7
        // "Miner (MIN)",          // 8
        // "Botanist (BTN)",       // 9
        // "Fisher (FSH)"          // 10
        "刻木师 (CRP)",      // 0
        "锻铁匠 (BSM)",     // 1
        "铸甲匠 (ARM)",        // 2
        "雕金匠 (GSM)",      // 3
        "制革匠 (LTW)",  // 4
        "裁衣匠 (WVR)",         // 5
        "炼金术士 (ALC)",      // 6
        "烹调师 (CUL)",     // 7
        "采矿工 (MIN)",          // 8
        "园艺工 (BTN)",       // 9
        "捕鱼人 (FSH)"          // 10
    };

    private static readonly int[] classIds = new[]
    {
        8,  // Carpenter
        9,  // Blacksmith
        10, // Armorer
        11, // Goldsmith
        12, // Leatherworker
        13, // Weaver
        14, // Alchemist
        15, // Culinarian
        16, // Miner
        17, // Botanist
        18  // Fisher
    };

    private static TurninState HighestTurnin = TurninState.Gold;

    public static void GeneralMissionSettings()
    {
        // if (ImGui.Button("Quick Apply Turnins"))
        if (ImGui.Button("快速应用交付设置"))
        {
            ImGui.OpenPopup("Quick Apply_Mission Turnins");
        }

        if (ImGui.BeginPopup("Quick Apply_Mission Turnins"))
        {
            // if (ImGui.RadioButton("Apply to all classes", ApplyToAllClasses))
            if (ImGui.RadioButton("应用到全部职业", ApplyToAllClasses))
            {
                ApplyToAllClasses = true;
                ApplyToSpecicClass = false;
            }

            // if (ImGui.RadioButton("Apply to specific class", ApplyToSpecicClass))
            if (ImGui.RadioButton("应用到指定职业", ApplyToSpecicClass))
            {
                ApplyToAllClasses = false;
                ApplyToSpecicClass = true;
            }
            if (ImGui.Combo("##ClassSelector", ref selectedClassIndex, classOptions, classOptions.Length))
            {
                // Update SpecificClass when selection changes
                SpecificClass = classIds[selectedClassIndex];
                // IceLogging.Debug($"Selected class: {classOptions[selectedClassIndex]}, ID: {SpecificClass}");
                IceLogging.Debug($"已选择职业：{classOptions[selectedClassIndex]}，ID：{SpecificClass}");
            }
            ImGui.Separator();
            // ImGui.Text("Select Turnin Options");
            ImGui.Text("选择交付选项");
            ImGui.Dummy(new Vector2(0, 2));

            // if (ImGui.RadioButton("Gold", HighestTurnin is TurninState.Gold))
            if (ImGui.RadioButton("金", HighestTurnin is TurninState.Gold))
            {
                HighestTurnin = TurninState.Gold;
            }
            // if (ImGui.RadioButton("Silver", HighestTurnin is TurninState.Silver))
            if (ImGui.RadioButton("银", HighestTurnin is TurninState.Silver))
            {
                HighestTurnin = TurninState.Silver;
            }
            // if (ImGui.RadioButton("Bronze", HighestTurnin is TurninState.Bronze))
            if (ImGui.RadioButton("铜", HighestTurnin is TurninState.Bronze))
            {
                HighestTurnin = TurninState.Bronze;
            }

            ImGui.Separator();

            // if (ImGui.Button("Apply"))
            if (ImGui.Button("应用"))
            {
                var amountApplied = 0;
                foreach (var mission in C.MissionConfig)
                {
                    if (CosmicHelper.SheetMissionDict.TryGetValue(mission.Key, out var sheetInfo))
                    {
                        if (ApplyToSpecicClass && !sheetInfo.Jobs.Contains((uint)SpecificClass))
                            continue;

                        if (sheetInfo.Attributes.HasFlag(MissionAttributes.Score_TimeRemaining))
                            continue;

                        if (C.MissionConfig.TryGetValue(mission.Key, out var config))
                        {
                            config.TurninGoal = HighestTurnin;
                        }
                        amountApplied += 1;
                    }
                }
                C.SaveDebounced();

                // Notify.Success($"Applied settings to: {amountApplied} missions, just for you buddy.");
                Notify.Success($"已应用设置到 {amountApplied} 个任务。");
                ImGui.CloseCurrentPopup();
            }


            ImGui.EndPopup();
        }
    }
}
