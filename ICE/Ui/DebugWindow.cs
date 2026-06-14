using Dalamud.Interface;
using ICE.Ui.Debug_Tabs.Debug_CS;
using ICE.Ui.Debug_Tabs.Debug_Hud;
using ICE.Ui.Debug_Tabs.Debug_Tables;
using ICE.Ui.Debug_Tabs.Debug_Ui;
using ICE.Ui.DebugWindowTabs;
using ICE.Ui.MainUi.HelpFolder;
using System.Collections.Generic;

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

    private readonly Dictionary<string, Action> DebugViews = new()
    {
        // ["Ui: Table V3"] = () => Table_MissionsV3.Draw(),
        ["界面：任务表 V3"] = () => Table_MissionsV3.Draw(),

        // HUD Elements
        // ["Hud: Moon Main"] = () => Hud_MainMoon.Draw(),
        ["HUD：星球主界面"] = () => Hud_MainMoon.Draw(),
        // ["Hud: Mission"] = () => Hud_Mission.Draw(),
        ["HUD：任务"] = () => Hud_Mission.Draw(),
        // ["Hud: Mission Info"] = () => Hud_MissionInfo.Draw(),
        ["HUD：任务信息"] = () => Hud_MissionInfo.Draw(),
        // ["Hud: Wheel of fortune!"] = () => Hud_WheelofFortune.Draw(),
        ["HUD：幸运转盘"] = () => Hud_WheelofFortune.Draw(),
        // ["Hud: Moon Recipe"] = () => Hud_MoonRecipe.Draw(),
        ["HUD：星球配方"] = () => Hud_MoonRecipe.Draw(),
        // ["Hud: Gather Collectable"] = () => Hud_CollectableGathering.Draw(),
        ["HUD：采集收藏品"] = () => Hud_CollectableGathering.Draw(),
        // ["Hud: Item Exchange"] = () => Hud_ItemExchange.Draw(),
        ["HUD：物品兑换"] = () => Hud_ItemExchange.Draw(),

        // Table Elements
        // ["Table: Mission Info"] = () => Table_MissionInfo.Draw(),
        ["表格：任务信息"] = () => Table_MissionInfo.Draw(),
        // ["Table: Gathering Missions"] = () => Table_GatheringInfo.Draw(),
        ["表格：采集任务"] = () => Table_GatheringInfo.Draw(),
        // ["Table: Special Missions"] = () => Table_TimeWeather.Draw(),
        ["表格：特殊任务"] = () => Table_TimeWeather.Draw(),
        // ["Table: Mission Text"] = () => Table_MissionText.Draw(),
        ["表格：任务文本"] = () => Table_MissionText.Draw(),
        // ["Table: Recipies"] = () => Table_MoonRecipies.Draw(),
        ["表格：配方"] = () => Table_MoonRecipies.Draw(),
        // ["Table: Fish Info"] = () => Table_FishInfo.Draw(),
        ["表格：捕鱼信息"] = () => Table_FishInfo.Draw(),

        // UI Elements
        // ["Ui: Select String"] = () => Ui_RedAlertString.Draw(),
        ["界面：选项字符串"] = () => Ui_RedAlertString.Draw(),
        // ["Ui: Fishing Hole Editor"] = () => Ui_Fish_HoleEditor.Draw(),
        ["界面：钓点编辑器"] = () => Ui_Fish_HoleEditor.Draw(),
        // ["Ui: Fishing Preset Editor"] = () => Ui_FishPresets.Draw(),
        ["界面：捕鱼预设编辑器"] = () => Ui_FishPresets.Draw(),
        // ["Ui: Gather Editor"] = () => Ui_GatherEditor.Draw(),
        ["界面：采集编辑器"] = () => Ui_GatherEditor.Draw(),
        // ["Ui: Log Viewer"] = () => helpSelect_Logs.Draw_Debug(),
        ["界面：日志查看器"] = () => helpSelect_Logs.Draw_Debug(),
        // ["Ui: Player Gearsets"] = () => Ui_Gearsets.Draw(),
        ["界面：玩家套装"] = () => Ui_Gearsets.Draw(),

        // Non-labeled Elements
        // ["CS: Tiemr Info"] = () => CS_TimerInfo.Draw(),
        ["客户端：计时信息"] = () => CS_TimerInfo.Draw(),
        // ["CS: Available Missions"] = () => CS_Missions.Draw(),
        ["客户端：可用任务"] = () => CS_Missions.Draw(),
        // ["Player Info"] = () => Ui_PlayerInfo.Draw(),
        ["玩家信息"] = () => Ui_PlayerInfo.Draw(),
        // ["Test Buttons"] = () => Ui_TestButtons.Draw(),
        ["测试按钮"] = () => Ui_TestButtons.Draw(),
        // ["IPC Testing"] = () => Ui_IPCTesting.Draw(),
        ["IPC 测试"] = () => Ui_IPCTesting.Draw(),
        // ["Map Test"] = () => Ui_MapTesting.Draw(),
        ["地图测试"] = () => Ui_MapTesting.Draw(),
        // ["Navmesh Testing"] = () => Ui_NavmeshTesting.Draw(),
        ["寻路测试"] = () => Ui_NavmeshTesting.Draw(),
        // ["Relic Info"] = () => Ui_RelicInfo.Draw(),
        ["Relic 信息"] = () => Ui_RelicInfo.Draw(),
        // ["TaskManager Testing"] = () => Ui_TaskManagerInfo.Draw(),
        ["任务管理器测试"] = () => Ui_TaskManagerInfo.Draw(),
        // ["NPC Box Viewer"] = () => Ui_NpcViewer.Draw(),
        ["NPC 框查看器"] = () => Ui_NpcViewer.Draw(),
        // ["ImGui Testing"] = () => UI_Test.Draw(),
        ["ImGui 测试"] = () => UI_Test.Draw(),
        // ["Relic Info V2"] = () => Ui_ClassInfo.Draw(),
        ["Relic 信息 V2"] = () => Ui_ClassInfo.Draw(),

        // Sheet Viewer Info
        // ["Sheet: Mission Rewards"] = () => Sheet_MissionRewards.Draw(),
        ["数据表：任务奖励"] = () => Sheet_MissionRewards.Draw(),
        // ["Table: Leveling Missions"] = () => Table_LevelingMissions.Draw(),
        ["表格：练级任务"] = () => Table_LevelingMissions.Draw(),
        // ["Table: Mission Select"] = () => Table_MissionSelect.Draw(),
        ["表格：任务选择"] = () => Table_MissionSelect.Draw(),
        // ["Oizyr Map Stuff"] = () => Ui_OyzinMap.Draw(),
        ["Oizys 地图"] = () => Ui_OyzinMap.Draw(),
        // ["Aethernet Test"] = () => Ui_Aethernet.Draw(),
        ["以太之光测试"] = () => Ui_Aethernet.Draw(),

        // ["IPC: Artisan"] = () => Ipc_Artisan.Draw()
        ["IPC：Artisan"] = () => Ipc_Artisan.Draw()
    };

    // private string selectedDebugView = "Hud: Moon Main"; // Store the name instead of index
    private string selectedDebugView = "HUD：星球主界面"; // Store the name instead of index

    public override unsafe void Draw()
    {
        float spacing = 10f;
        float leftPanelWidth = 200f;
        float childHeight = ImGui.GetContentRegionAvail().Y;

        if (_showSidebar)
        {
            if (ImGui.BeginChild("DebugSelector", new Vector2(leftPanelWidth, childHeight), true))
            {
                foreach (var viewName in DebugViews.Keys)
                {
                    bool isSelected = (selectedDebugView == viewName);
                    string label = isSelected ? $"→ {viewName}" : $"   {viewName}";

                    if (ImGui.Selectable(label, isSelected))
                    {
                        selectedDebugView = viewName;
                    }
                }
            }
            ImGui.EndChild();

            ImGui.SameLine(0, spacing);
        }

        float rightPanelWidth = ImGui.GetContentRegionAvail().X;

        if (ImGui.BeginChild("DebugContent", new Vector2(rightPanelWidth, childHeight), true))
        {
            if (DebugViews.TryGetValue(selectedDebugView, out var drawAction))
            {
                drawAction();
            }
            else
            {
                // ImGui.Text("Unknown Debug View");
                ImGui.Text("未知调试视图");
            }
        }
        ImGui.EndChild();
    }
}
