using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Hud_MoonRecipe
    {
        public static void Draw()
        {
            if (GenericHelpers.TryGetAddonMaster<WKSRecipeNotebook>("WKSRecipeNotebook", out var x) && x.IsAddonReady)
            {
                ImGui.Text(x.SelectedCraftingItem);

                if (ImGui.Button("填充 NQ"))
                {
                    x.NQItemInput();
                }
                ImGui.SameLine();

                if (ImGui.Button("填充 HQ"))
                {
                    x.HQItemInput();
                }
                ImGui.SameLine();

                if (ImGui.Button("填充全部"))
                {
                    x.NQItemInput();
                    x.HQItemInput();
                }
                ImGui.SameLine();

                if (ImGui.Button("制作"))
                {
                    x.Synthesize();
                }

                foreach (var m in x.CraftingItems)
                {
                    if (ImGui.Button($"选择###Select + {m.Name}"))
                    {
                        m.Select();
                    }
                    ImGui.SameLine();
                    ImGui.Text($"{m.Name}");
                }
            }
            else
            {
                ImGui.Text("等待 \"WKSRecipeNotebook\" 显示");
            }
        }
    }
}
