using ICE.Utilities.Cosmic_Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Ipc_Artisan
    {
        internal static List<ItemInfo> PotentialCrafterFood = new();
        internal static List<ItemInfo> PotentialPots = new();
        internal static List<ItemInfo> PotentialManuals = new();
        internal static List<ItemInfo> PotentialSquadronManuals = new();

        internal static ItemInfo SelectedFood = new();
        internal static ItemInfo SelectedPot = new();
        internal static ItemInfo SelectedManual = new();
        internal static ItemInfo SelectedSquadronManual = new();

        public class ItemInfo
        {
            public uint Id { get; set; } = 0;
            public string Name { get; set; } = "";
        }

        internal static uint RecipeId = 0;
        private static uint MaxSkillUsage = 0;

        public static void Draw()
        {
            // ImGui.Text($"Selected Food: [{SelectedFood.Id}] {SelectedFood.Name}");
            ImGui.Text($"已选食物: [{SelectedFood.Id}] {SelectedFood.Name}");
            ImGui.SameLine();
            // if (ImGui.Button("Select Food"))
            if (ImGui.Button("选择食物"))
            {
                PotentialCrafterFood.Clear();
                foreach (var food in ConsumableInfo.CrafterFood)
                {
                    if (PlayerHelper.GetItemCount(food.Id, out var count) && count > 0)
                    {
                        PotentialCrafterFood.Add(new() { Id = food.Id, Name = food.Name });
                    }
                }
                // ImGui.OpenPopup("Select Crafter Food");
                ImGui.OpenPopup("选择生产食物");
            }

            // if (ImGui.BeginPopup("Select Crafter Food"))
            if (ImGui.BeginPopup("选择生产食物"))
            {
                foreach (var item in PotentialCrafterFood)
                {
                    if (ImGui.Selectable($"{item.Name}"))
                        SelectedFood = item;
                }
                ImGui.EndPopup();
            }

            // ImGui.Text($"Selected Pot: [{SelectedPot.Id}] {SelectedPot.Name}");
            ImGui.Text($"已选药水: [{SelectedPot.Id}] {SelectedPot.Name}");
            ImGui.SameLine();
            // if (ImGui.Button("Select Pot"))
            if (ImGui.Button("选择药水"))
            {
                PotentialPots.Clear();
                foreach (var pot in ConsumableInfo.Pots)
                {
                    if (PlayerHelper.GetItemCount(pot.Id, out var count))
                    {
                        PotentialPots.Add(new() { Id = pot.Id, Name = pot.Name });
                    }
                }
                // ImGui.OpenPopup("Select Pot");
                ImGui.OpenPopup("选择药水");
            }

            // if (ImGui.BeginPopup("Select Pot"))
            if (ImGui.BeginPopup("选择药水"))
            {
                foreach (var item in PotentialPots)
                {
                    if (ImGui.Selectable($"{item.Name}"))
                        SelectedPot = item;
                }
                ImGui.EndPopup();
            }

            // ImGui.Text($"Selected Manual: [{SelectedManual.Id}] {SelectedManual.Name}");
            ImGui.Text($"已选手册: [{SelectedManual.Id}] {SelectedManual.Name}");
            ImGui.SameLine();
            // if (ImGui.Button("Select Manual"))
            if (ImGui.Button("选择手册"))
            {
                PotentialManuals.Clear();
                foreach (var manual in ConsumableInfo.Manuals)
                {
                    if (PlayerHelper.GetItemCount(manual.Id, out var count))
                    {
                        PotentialManuals.Add(new() { Id = manual.Id, Name = manual.Name });
                    }
                }
                // ImGui.OpenPopup("Select Manual");
                ImGui.OpenPopup("选择手册");
            }

            // if (ImGui.BeginPopup("Select Manual"))
            if (ImGui.BeginPopup("选择手册"))
            {
                foreach (var item in PotentialManuals)
                {
                    if (ImGui.Selectable($"{item.Name}"))
                        SelectedManual = item;
                }
                ImGui.EndPopup();
            }

            // ImGui.Text($"Selected Squadron Manual: [{SelectedSquadronManual.Id}] {SelectedSquadronManual.Name}");
            ImGui.Text($"已选小队手册: [{SelectedSquadronManual.Id}] {SelectedSquadronManual.Name}");
            ImGui.SameLine();
            // if (ImGui.Button("Select Squadron Manual"))
            if (ImGui.Button("选择小队手册"))
            {
                PotentialSquadronManuals.Clear();
                foreach (var manual in ConsumableInfo.SquadronManuals)
                {
                    if (PlayerHelper.GetItemCount(manual.Id, out var count))
                    {
                        PotentialSquadronManuals.Add(new() { Id = manual.Id, Name = manual.Name });
                    }
                }
                // ImGui.OpenPopup("Select Squadron Manual");
                ImGui.OpenPopup("选择小队手册");
            }

            // if (ImGui.BeginPopup("Select Squadron Manual"))
            if (ImGui.BeginPopup("选择小队手册"))
            {
                foreach (var item in PotentialSquadronManuals)
                {
                    if (ImGui.Selectable($"{item.Name}"))
                        SelectedSquadronManual = item;
                }
                ImGui.EndPopup();
            }

            ImGui.Separator();
            // ImGui.InputUInt("Recipe Id", ref RecipeId);
            ImGui.InputUInt("配方 Id", ref RecipeId);

            // if (ImGui.Button("Reset Temp"))
            if (ImGui.Button("重置临时"))
            {
                P.Artisan.SetTempFoodBackToNormal(RecipeId);
                P.Artisan.SetTempPotionBackToNormal(RecipeId);
                P.Artisan.SetTempManualBackToNormal(RecipeId);
                P.Artisan.SetTempSquadronManualBackToNormal(RecipeId);
            }

            // if (ImGui.Button("Food [HQ]"))
            if (ImGui.Button("食物 [HQ]"))
            {
                P.Artisan.ChangeFood(RecipeId, SelectedFood.Id, true, true);
            }
            ImGui.SameLine();
            // if (ImGui.Button("Food [NQ]"))
            if (ImGui.Button("食物 [NQ]"))
            {
                P.Artisan.ChangeFood(RecipeId, SelectedFood.Id, false, true);
            }

            // if (ImGui.Button("Potion [HQ]"))
            if (ImGui.Button("药水 [HQ]"))
            {
                P.Artisan.ChangePotion(RecipeId, SelectedPot.Id, true, true);
            }
            ImGui.SameLine();
            // if (ImGui.Button("Potion [NQ]"))
            if (ImGui.Button("药水 [NQ]"))
            {
                P.Artisan.ChangePotion(RecipeId, SelectedPot.Id, false, true);
            }

            // if (ImGui.Button("Manual"))
            if (ImGui.Button("手册"))
            {
                P.Artisan.ChangeManual(RecipeId, SelectedManual.Id, true);
            }
            ImGui.SameLine();
            // if (ImGui.Button("Squad Manual"))
            if (ImGui.Button("小队手册"))
            {
                P.Artisan.ChangeSquadronManual(RecipeId, SelectedSquadronManual.Id, true);
            }

            var mission = CosmicHelper.SheetMissionDict.FirstOrNull(kvp => kvp.Value.Crafts_Main.ContainsKey((ushort)RecipeId)
                                                                    || kvp.Value.Crafts_Pre.ContainsKey((ushort)RecipeId));

            if (mission != null)
            {
                var sheetInfo = mission.Value.Value;

                if (sheetInfo.TemporaryActionCount != 0)
                {
                    var actionInfo = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>().GetRow(sheetInfo.TemporaryActionId);
                    var name = actionInfo.Name;
                    var icon = Svc.Texture.GetFromGameIcon((int)actionInfo.Icon).GetWrapOrEmpty();
                    ImGui.Image(icon.Handle, new(24, 24));
                    ImGui.AlignTextToFramePadding();
                    ImGui.SameLine();
                    ImGui.Text($"{name}");

                    // ImGui.SliderUInt("Max Usage", ref MaxSkillUsage, 0, 2);
                    ImGui.SliderUInt("最大使用次数", ref MaxSkillUsage, 0, 2);
                    // if (ImGui.Button("Apply Temp"))
                    if (ImGui.Button("应用临时"))
                    {
                        if (sheetInfo.TemporaryActionId == 41269)
                        {
                            P.Artisan.ChangeExpertMaxMaterialMiracleUses(RecipeId, MaxSkillUsage, false);
                        }
                    }
                }
            }

        }
    }
}
