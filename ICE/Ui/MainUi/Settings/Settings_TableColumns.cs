using Dalamud.Interface.Utility.Raii;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.ImGuiTools;

namespace ICE.Ui.MainUi.Settings;

public static class Settings_TableColumns
{
    private static string[] missionSortOptions = 
        ["ID", "名称", "Cosmo Credits", "Lunar Credits", 
        "经验 I", "经验 II", "经验 III", "经验 IV", "经验 V", 
        "地图位置", "职业分数", "职业经验"];

    public static void ColumnSettings()
    {
        int missionSelectedOption = C.TableSortOption;
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
        if (ImGui.Checkbox("隐藏不支持的任务", ref hideUnsupported))
        {
            C.HideUnsupportedMissions = hideUnsupported;
            C.Save();
        }

        bool autoShowToken = C.Auto_ShowTokens;
        if (ImGui.Checkbox("自动隐藏/显示星球代币", ref autoShowToken))
        {
            C.Auto_ShowTokens = autoShowToken;
            C.Save();
        }

        ImGuiEx.HelpMarker("仅在你打算自己手动完成任务、且不使用自动化时启用此项。 " +
                           "或者当你让其他插件来处理所有交付、制作、采集的自动化……而不让 I.C.E. 来与这些插件交互时启用");
    }

    private static bool ApplyToAllClasses = true;
    private static bool ApplyToSpecicClass = false;
    private static int SpecificClass = 8;
    private static int selectedClassIndex = 0;

    private static readonly string[] classOptions = new[]
    {
        "刻木匠 (CRP)",      // 0
        "锻铁匠 (BSM)",     // 1
        "铸甲匠 (ARM)",        // 2
        "雕金匠 (GSM)",        // 3
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
        if (ImGui.Button("快速应用交付"))
        {
            ImGui.OpenPopup("Quick Apply_Mission Turnins");
        }

        if (ImGui.BeginPopup("Quick Apply_Mission Turnins"))
        {
            if (ImGui.RadioButton("应用到所有职业", ApplyToAllClasses))
            {
                ApplyToAllClasses = true;
                ApplyToSpecicClass = false;
            }

            if (ImGui.RadioButton("应用到指定职业", ApplyToSpecicClass))
            {
                ApplyToAllClasses = false;
                ApplyToSpecicClass = true;
            }
            if (ImGui.Combo("##ClassSelector", ref selectedClassIndex, classOptions, classOptions.Length))
            {
                // Update SpecificClass when selection changes
                SpecificClass = classIds[selectedClassIndex];
                IceLogging.Debug($"Selected class: {classOptions[selectedClassIndex]}, ID: {SpecificClass}");
            }
            ImGui.Separator();
            ImGui.Text("选择交付选项");
            ImGui.Dummy(new Vector2(0, 2));

            if (ImGui.RadioButton("金牌", HighestTurnin is TurninState.Gold))
            {
                HighestTurnin = TurninState.Gold;
            }
            if (ImGui.RadioButton("银牌", HighestTurnin is TurninState.Silver))
            {
                HighestTurnin = TurninState.Silver;
            }
            if (ImGui.RadioButton("铜牌", HighestTurnin is TurninState.Bronze))
            {
                HighestTurnin = TurninState.Bronze;
            }

            ImGui.Separator();

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

                Notify.Success($"已将设置应用到 {amountApplied} 个任务，专为你服务，伙计。");
                ImGui.CloseCurrentPopup();
            }


            ImGui.EndPopup();
        }
    }
}
