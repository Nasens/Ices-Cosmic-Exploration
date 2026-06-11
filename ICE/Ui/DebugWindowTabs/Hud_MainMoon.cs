using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Hud_MainMoon
    {
        public static void Draw()
        {
            if (GenericHelpers.TryGetAddonMaster<WKSHud>("WKSHud", out var HudAddon))
            {
                if (ImGui.Button("任务"))
                {
                    HudAddon.Mission();
                }

                ImGui.SameLine();

                if (ImGui.Button("机械"))
                {
                    HudAddon.Mech();
                }

                ImGui.SameLine();

                if (ImGui.Button("星象"))
                {
                    HudAddon.Steller();
                }

                ImGui.SameLine();

                if (ImGui.Button("基建"))
                {
                    HudAddon.Infrastructor();
                }

                ImGui.SameLine();

                if (ImGui.Button("研究"))
                {
                    HudAddon.Research();
                }

                ImGui.SameLine();

                if (ImGui.Button("职业追踪"))
                {
                    HudAddon.ClassTracker();
                }
            }
            else
            {
                ImGui.Text("等待 \"WKSHud\" 可见");
            }
        }
    }
}
