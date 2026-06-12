using Dalamud.Interface.Utility.Raii;
using ICE.Utilities.Cosmic_Helper;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Ui_ClassInfo
    {
        public static void Draw()
        {
            var classInfo = CosmicHelper.Cosmic_ClassInfo();

            using var tabBar = ImRaii.TabBar("Cosmic Class Info");
            if (!tabBar) return;

            foreach (var item in classInfo)
            {
                using var tabItem = ImRaii.TabItem($"Job {item.Key}");
                if (!tabItem) continue;

                // ImGui.Text($"Score: {item.Value.Score}");
                ImGui.Text($"分数: {item.Value.Score}");
                ImGui.Separator();

                using var table = ImRaii.Table($"ClassInfo_{item.Key}", 2,
                    ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit);
                if (!table) continue;

                // Set up columns
                // ImGui.TableSetupColumn("Property", ImGuiTableColumnFlags.WidthFixed, 150f);
                ImGui.TableSetupColumn("属性", ImGuiTableColumnFlags.WidthFixed, 150f);
                // ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("数值", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableHeadersRow();

                // Current Stage
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                // ImGui.Text("Current Stage");
                ImGui.Text("当前阶段");
                ImGui.TableNextColumn();
                ImGui.Text($"{item.Value.Stage_Current}");

                // Next Stage
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                // ImGui.Text("Next Stage");
                ImGui.Text("下一阶段");
                ImGui.TableNextColumn();
                ImGui.Text($"{item.Value.Stage_Next}");

                // Experience entries
                foreach (var exp in item.Value.CurrentExp)
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.Text(exp.Value.Name);
                    ImGui.TableNextColumn();
                    ImGui.Text($"{exp.Value.Current} / {exp.Value.Needed} (Max: {exp.Value.Max})");
                }
            }
        }
    }
}
