using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;

namespace ICE.Ui.Debug_Tabs.Debug_Hud
{
    internal class Hud_MoonRecipe
    {
        public static void Draw()
        {
            if (GenericHelpers.TryGetAddonMaster<WKSRecipeNotebook>("WKSRecipeNotebook", out var x) && x.IsAddonReady)
            {
                ImGui.Text(x.SelectedCraftingItem);

                // if (ImGui.Button("Fill NQ"))
                if (ImGui.Button("填入 NQ"))
                {
                    x.NQItemInput();
                }
                ImGui.SameLine();

                // if (ImGui.Button("Fill HQ"))
                if (ImGui.Button("填入 HQ"))
                {
                    x.HQItemInput();
                }
                ImGui.SameLine();

                // if (ImGui.Button("Fill Both"))
                if (ImGui.Button("填入两者"))
                {
                    x.NQItemInput();
                    x.HQItemInput();
                }
                ImGui.SameLine();

                // if (ImGui.Button("Synthesize"))
                if (ImGui.Button("合成"))
                {
                    x.Synthesize();
                }

                foreach (var m in x.CraftingItems)
                {
                    // if (ImGui.Button($"Select ###Select + {m.Name}"))
                    if (ImGui.Button($"选择 ###Select + {m.Name}"))
                    {
                        m.Select();
                    }
                    ImGui.SameLine();
                    ImGui.Text($"{m.Name}");
                }
            }
            else
            {
                // ImGui.Text("Waiting for \"WKSRecipeNotebook\" to be visible");
                ImGui.Text("等待 \"WKSRecipeNotebook\" 界面可见");
            }
        }
    }
}
