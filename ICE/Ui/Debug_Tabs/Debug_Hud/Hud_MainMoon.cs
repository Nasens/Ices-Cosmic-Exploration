using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;

namespace ICE.Ui.Debug_Tabs.Debug_Hud
{
    internal class Hud_MainMoon
    {
        public static void Draw()
        {
            if (GenericHelpers.TryGetAddonMaster<WKSHud>("WKSHud", out var HudAddon))
            {
                // if (ImGui.Button("Mission"))
                if (ImGui.Button("任务"))
                {
                    HudAddon.Mission();
                }

                ImGui.SameLine();

                // if (ImGui.Button("Mech"))
                if (ImGui.Button("机甲"))
                {
                    HudAddon.Mech();
                }

                ImGui.SameLine();

                // if (ImGui.Button("Steller"))
                if (ImGui.Button("Steller"))
                {
                    HudAddon.Steller();
                }

                ImGui.SameLine();

                // if (ImGui.Button("Infrastructor"))
                if (ImGui.Button("基础设施"))
                {
                    HudAddon.Infrastructor();
                }

                ImGui.SameLine();

                // if (ImGui.Button("Research"))
                if (ImGui.Button("研究"))
                {
                    HudAddon.Research();
                }

                ImGui.SameLine();

                // if (ImGui.Button("ClassTracker"))
                if (ImGui.Button("职业追踪"))
                {
                    HudAddon.ClassTracker();
                }
            }
            else
            {
                // ImGui.Text("Waiting for \"WKSHud\" to be visible");
                ImGui.Text("等待 \"WKSHud\" 界面可见");
            }
        }
    }
}
