using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.GameHelpers;
using ICE.Utilities.Cosmic_Helper;
using Pictomancy;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Ui_NpcViewer
    {
        private static float radius = 0.5f;

        public static void Draw()
        {
            var territoryid = Player.Territory.RowId;
            if (NpcData.MoonNpcs.TryGetValue(territoryid, out var moonNpcs))
            {
                ImGui.Text($"区域 ID: {territoryid}");
                ImGui.Text($"有效月球 NPC 信息: {moonNpcs != null}");
                if (moonNpcs != null)
                {
                    List<Vector3> pictoCircles = new();


                    if (ImGui.BeginTable("NPC Info Debugger", 5, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders))
                    {
                        ImGui.TableSetupColumn("名称");
                        ImGui.TableSetupColumn("位置");
                        ImGui.TableSetupColumn("移动目标点");
                        ImGui.TableSetupColumn("移动至");
                        ImGui.TableSetupColumn("设为当前");

                        foreach (var npcEntry in moonNpcs.Values)
                        {
                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            ImGui.Text($"{npcEntry.Name} [{npcEntry.NpcId}]");

                            ImGui.TableNextColumn();
                            ImGui.Text($"{npcEntry.Location_Npc:N2}");
                            ImGui.Text($"距离: {Player.DistanceTo(npcEntry.Location_Npc):N2}");

                            ImGui.TableNextColumn();
                            ImGui.Text($"{npcEntry.Location_Circle:N2}");
                            pictoCircles.Add(npcEntry.Location_Circle);

                            ImGui.TableNextColumn();
                            if (ImGui.Button($"移动至##MoveTo_{npcEntry.NpcId}"))
                            {
                                Vector3 moveLoc = NpcData.GetRandomPointInCircle(npcEntry.Location_Circle, radius);
                                Task_NavmeshMove.Task_NavTo(moveLoc, distance: 5, npcLoc: npcEntry.Location_Npc);
                            }

                            ImGui.TableNextColumn();
                            if (ImGui.Button($"设为当前##SetCurrent_{npcEntry.NpcId}"))
                            {
                                Vector3 currentPos = Player.Position;
                                npcEntry.Location_Circle = currentPos;
                            }
                            if (ImGui.Button($"复制当前设置##CopyCurrent_{npcEntry.NpcId}"))
                            {
                                ImGui.SetClipboardText($"{npcEntry.Location_Circle.X:N2}f, {npcEntry.Location_Circle.Y:N2}f, {npcEntry.Location_Circle.Z:N2}f");
                            }
                        }

                        ImGui.EndTable();
                    }

                    using (var drawList = PctService.Draw())
                    {
                        foreach (var location in pictoCircles)
                        {
                            drawList.AddCircleFilled(location, radius, C.PictoColor_Circle);
                        }
                    }
                }
            }
        }
    }
}
