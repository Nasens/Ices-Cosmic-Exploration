using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using ICE.Ui.Debug_Tabs.Debug_CS;
using ICE.Ui.Debug_Tabs.Debug_Hud;
using ICE.Ui.Debug_Tabs.Debug_Tables;
using ICE.Ui.Debug_Tabs.Debug_Ui;
using ICE.Ui.DebugWindowTabs;
using ICE.Ui.MainUi.HelpFolder;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ICE.Ui;

internal class DebugWindow : Window
{
    private bool _showSidebar = true;

    public DebugWindow() :
        // base($"ICE {P.GetType().Assembly.GetName().Version} Debugger ###IceCosmicDebug1")
        base($"ICE {P.GetType().Assembly.GetName().Version} 调试器 ###IceCosmicDebug1")
    {
        Flags = ImGuiWindowFlags.None;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(100, 100),
            MaximumSize = new Vector2(3000, 3000)
        };

        // Title-bar toggle for the left tab list, so the window can be shrunk down to just the content.
        TitleBarButtons.Add(new TitleBarButton
        {
            Icon = FontAwesomeIcon.Bars,
            IconOffset = new Vector2(2, 1),
            Click = _ => _showSidebar = !_showSidebar,
            // ShowTooltip = () => ImGui.SetTooltip(_showSidebar ? "Hide tab list" : "Show tab list"),
            ShowTooltip = () => ImGui.SetTooltip(_showSidebar ? "隐藏标签列表" : "显示标签列表"),
        });

        P.windowSystem.AddWindow(this);
    }

    public void Dispose()
    {
        P.windowSystem.RemoveWindow(this);
    }

    private readonly Dictionary<string, Dictionary<string, Action>> DebugViewGroups = new()
    {
        ["Hud"] = new()
        {
            ["Shop"] = () => Hud_Shop.Draw(),
            // ["Moon Main"] = () => Hud_MainMoon.Draw(),
            ["HUD：星球主界面"] = () => Hud_MainMoon.Draw(),
            // ["Mission"] = () => Hud_Mission.Draw(),
            ["HUD：任务"] = () => Hud_Mission.Draw(),
            // ["Mission Info"] = () => Hud_MissionInfo.Draw(),
            ["HUD：任务信息"] = () => Hud_MissionInfo.Draw(),
            // ["Wheel of Fortune!"] = () => Hud_WheelofFortune.Draw(),
            ["HUD：幸运转盘"] = () => Hud_WheelofFortune.Draw(),
            // ["Moon Recipe"] = () => Hud_MoonRecipe.Draw(),
            ["HUD：星球配方"] = () => Hud_MoonRecipe.Draw(),
            // ["Gather Collectable"] = () => Hud_CollectableGathering.Draw(),
            ["HUD：采集收藏品"] = () => Hud_CollectableGathering.Draw(),
            // ["Item Exchange"] = () => Hud_ItemExchange.Draw(),
            ["HUD：物品兑换"] = () => Hud_ItemExchange.Draw(),
            ["Token Exchange"] = () => Hud_ShopExchange.Draw(),
            ["Aethernet"] = () => Hud_Aethernet.Draw(),
        },
        ["Table"] = new()
        {
            // ["Mission Info"] = () => Table_MissionInfo.Draw(),
            ["表格：任务信息"] = () => Table_MissionInfo.Draw(),
            // ["Gathering Missions"] = () => Table_GatheringInfo.Draw(),
            ["表格：采集任务"] = () => Table_GatheringInfo.Draw(),
            // ["Special Missions"] = () => Table_TimeWeather.Draw(),
            ["表格：特殊任务"] = () => Table_TimeWeather.Draw(),
            // ["Mission Text"] = () => Table_MissionText.Draw(),
            ["表格：任务文本"] = () => Table_MissionText.Draw(),
            // ["Recipes"] = () => Table_MoonRecipies.Draw(),
            ["表格：配方"] = () => Table_MoonRecipies.Draw(),
            // ["Fish Info"] = () => Table_FishInfo.Draw(),
            ["表格：捕鱼信息"] = () => Table_FishInfo.Draw(),
            // ["Leveling Missions"] = () => Table_LevelingMissions.Draw(),
            ["表格：练级任务"] = () => Table_LevelingMissions.Draw(),
            // ["Mission Select"] = () => Table_MissionSelect.Draw(),
            ["表格：任务选择"] = () => Table_MissionSelect.Draw(),
            ["Mission V3"] = () => Table_MissionsV3.Draw(),
        },
        ["Ui"] = new()
        {
            // ["Select String"] = () => Ui_RedAlertString.Draw(),
            ["界面：选项字符串"] = () => Ui_RedAlertString.Draw(),
            // ["Fishing Hole Editor"] = () => Ui_Fish_HoleEditor.Draw(),
            ["界面：钓点编辑器"] = () => Ui_Fish_HoleEditor.Draw(),
            // ["Fishing Presets"] = () => Ui_FishPresets.Draw(),
            ["界面：捕鱼预设编辑器"] = () => Ui_FishPresets.Draw(),
            // ["Gather Editor"] = () => Ui_GatherEditor.Draw(),
            ["界面：采集编辑器"] = () => Ui_GatherEditor.Draw(),
            // ["Log Viewer"] = () => helpSelect_Logs.Draw_Debug(),
            ["界面：日志查看器"] = () => helpSelect_Logs.Draw_Debug(),
            // ["Player Gearsets"] = () => Ui_Gearsets.Draw(),
            ["界面：玩家套装"] = () => Ui_Gearsets.Draw(),
            // ["Player Info"] = () => Ui_PlayerInfo.Draw(),
            ["玩家信息"] = () => Ui_PlayerInfo.Draw(),
            // ["Relic Info"] = () => Ui_RelicInfo.Draw(),
            ["Relic 信息"] = () => Ui_RelicInfo.Draw(),
            // ["Relic Info V2"] = () => Ui_ClassInfo.Draw(),
            ["Relic 信息 V2"] = () => Ui_ClassInfo.Draw(),
            // ["NPC Box Viewer"] = () => Ui_NpcViewer.Draw(),
            ["NPC 框查看器"] = () => Ui_NpcViewer.Draw(),
            // ["Oizyr Map Stuff"] = () => Ui_OyzinMap.Draw(),
            ["Oizys 地图"] = () => Ui_OyzinMap.Draw(),
            // ["Aethernet Test"] = () => Ui_Aethernet.Draw(),
            ["以太之光测试"] = () => Ui_Aethernet.Draw(),
        },
        ["Misc"] = new()
        {
            // ["CS: Timer Info"] = () => CS_TimerInfo.Draw(),
            ["客户端：计时信息"] = () => CS_TimerInfo.Draw(),
            // ["CS: Available Missions"] = () => CS_Missions.Draw(),
            ["客户端：可用任务"] = () => CS_Missions.Draw(),
            // ["Test Buttons"] = () => Ui_TestButtons.Draw(),
            ["测试按钮"] = () => Ui_TestButtons.Draw(),
            // ["IPC Testing"] = () => Ui_IPCTesting.Draw(),
            ["IPC 测试"] = () => Ui_IPCTesting.Draw(),
            // ["IPC: Artisan"] = () => Ipc_Artisan.Draw(),
            ["IPC：Artisan"] = () => Ipc_Artisan.Draw(),
            // ["Map Test"] = () => Ui_MapTesting.Draw(),
            ["地图测试"] = () => Ui_MapTesting.Draw(),
            // ["Navmesh Testing"] = () => Ui_NavmeshTesting.Draw(),
            ["寻路测试"] = () => Ui_NavmeshTesting.Draw(),
            // ["TaskManager Testing"] = () => Ui_TaskManagerInfo.Draw(),
            ["任务管理器测试"] = () => Ui_TaskManagerInfo.Draw(),
            // ["ImGui Testing"] = () => UI_Test.Draw(),
            ["ImGui 测试"] = () => UI_Test.Draw(),
            // ["Sheet: Mission Rewards"] = () => Sheet_MissionRewards.Draw(),
            ["数据表：任务奖励"] = () => Sheet_MissionRewards.Draw(),
        },
    };

    private string _selectedGroup = "Hud";
    // private string _selectedView = "Moon Main";
    private string _selectedView = "HUD：星球主界面";

    public override void Draw()
    {
        float spacing = 10f;
        float leftPanelWidth = 250f;
        float childHeight = ImGui.GetContentRegionAvail().Y;

        if (_showSidebar)
        {
            DrawSidebar(leftPanelWidth, childHeight);
            ImGui.SameLine(0, spacing);
        }

        float rightPanelWidth = ImGui.GetContentRegionAvail().X;
        using var content = ImRaii.Child("DebugContent", new Vector2(rightPanelWidth, childHeight), true);
        if (!content || !content.Success)
            return;

        if (DebugViewGroups.TryGetValue(_selectedGroup, out var views)
            && views.TryGetValue(_selectedView, out var drawAction))
        {
            drawAction();
        }
    }

    private void DrawSidebar(float width, float height)
    {
        var lineHeight = ImGui.GetTextLineHeightWithSpacing();
        var halfLineHeight = (int)MathF.Round(lineHeight / 2f);

        using var child = ImRaii.Child("DebugGroupSelector", new Vector2(width, height), true, ImGuiWindowFlags.NoSavedSettings);
        if (!child || !child.Success)
            return;

        using var table = ImRaii.Table("DebugGroupTable"u8, 1, ImGuiTableFlags.NoSavedSettings);
        if (!table || !table.Success)
            return;

        ImGui.TableSetupColumn("Group"u8, ImGuiTableColumnFlags.WidthStretch);

        foreach (var (groupName, views) in DebugViewGroups)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();

            bool groupSelected = _selectedGroup == groupName;
            if (ImGui.Selectable($"{groupName}###Selectable_{groupName}", groupSelected))
            {
                _selectedGroup = groupName;
                _selectedView = views.Keys.First();
            }

            // Always expanded now — no gating on _selectedGroup.
            var viewNames = views.Keys.ToList();
            for (var i = 0; i < viewNames.Count; i++)
            {
                var viewName = viewNames[i];
                var pos = ImGui.GetCursorPos();

                ImGui.Indent(lineHeight);

                bool viewSelected = _selectedGroup == groupName && _selectedView == viewName;
                if (ImGui.Selectable($"{viewName}###Selectable_{groupName}_{viewName}", viewSelected))
                {
                    _selectedGroup = groupName;
                    _selectedView = viewName;
                }

                ImGui.Unindent(lineHeight);

                // Tree-connector line, drawn at the indent gutter.
                var linePos = ImGui.GetWindowPos() + pos
                    - new Vector2(ImGui.GetScrollX(), ImGui.GetScrollY())
                    + new Vector2(MathF.Round(halfLineHeight - ImGui.GetStyle().ItemSpacing.Y / 2f), -MathF.Round(ImGui.GetStyle().ItemSpacing.Y / 2f));

                ImGui.GetWindowDrawList().AddLine(
                    linePos,
                    linePos + new Vector2(0, i == viewNames.Count - 1 ? halfLineHeight : lineHeight),
                    ImGui.GetColorU32(ImGuiCol.TextDisabled));

                ImGui.GetWindowDrawList().AddLine(
                    linePos + new Vector2(0, halfLineHeight),
                    linePos + new Vector2(halfLineHeight, halfLineHeight),
                    ImGui.GetColorU32(ImGuiCol.TextDisabled));
            }
        }
    }
}
