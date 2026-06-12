using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.Game.WKS;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ICE.Scheduler.Handlers.PictoStuff;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.ImGuiTools;
using System.Collections.Generic;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Ui_PlayerInfo
    {
        private static uint best_LevelMission = 0;
        private static uint playerLevel = 90;

        private static int currentXp = 0;
        private static int neededXp = 100;
        private static int maxXp = 200;

        public static unsafe void Draw()
        {
            ImGui.SetNextItemWidth(200);
            // ImGui.InputInt("Current XP", ref currentXp);
            ImGui.InputInt("当前经验", ref currentXp);
            ImGui.SetNextItemWidth(200);
            // ImGui.InputInt("Needed XP", ref neededXp);
            ImGui.InputInt("所需经验", ref neededXp);
            ImGui.SetNextItemWidth(200);
            // ImGui.InputInt("Max XP", ref maxXp);
            ImGui.InputInt("最大经验", ref maxXp);
            ImGui_Ice.Draw_XPBar(currentXp, neededXp, maxXp, size: new Vector2(200, 10));

            ImGui.Separator();
            var currentProgress = WorldProgress();
            // ImGui.Text($"World Stage: {currentProgress}");
            ImGui.Text($"世界阶段: {currentProgress}");

            // ImGui.Text("Need to actually put the player info here. It got lost");
            ImGui.Text("需要在此处添加玩家信息，内容已丢失");
            ImGui.Spacing();
            ImGui.AlignTextToFramePadding();
            // ImGui.Text($"Player Position: X:{Player.Position.X:N2}, Y:{Player.Position.Y:N2}, Z:{Player.Position.Z:N2}");
            ImGui.Text($"玩家位置: X:{Player.Position.X:N2}, Y:{Player.Position.Y:N2}, Z:{Player.Position.Z:N2}");
            ImGui.SameLine();
            // if (ImGui.Button("Copy Vector2"))
            if (ImGui.Button("复制 Vector2"))
            {
                ImGui.SetClipboardText($"{Player.Position.X:N2}f, {Player.Position.Z:N2}f");
            }
            ImGui.SameLine();
            // if (ImGui.Button("Copy Vector3"))
            if (ImGui.Button("复制 Vector3"))
            {
                ImGui.SetClipboardText($"{Player.Position.X:N2}f, {Player.Position.Y:N2}f, {Player.Position.Z:N2}f");
            }
            // ImGui.Text($"Job: {Player.Job}");
            ImGui.Text($"职业: {Player.Job}");
            // ImGui.Text($"JobId: {(uint)Player.Job}");
            ImGui.Text($"职业 ID: {(uint)Player.Job}");
            // ImGui.Text($"Current Territory/ZoneId: {Player.Territory.RowId}");
            ImGui.Text($"当前区域/ZoneId: {Player.Territory.RowId}");
            if (PlayerHelper.IsInCosmicZone())
            {
                var manager = WKSManager.Instance();
                var currentMission = manager->State.CurrentMission.MissionUnitRowId;

                // ImGui.Text($"Current Mission: {currentMission}");
                ImGui.Text($"当前任务: {currentMission}");
            }
            if (Svc.Targets.Target != null)
            {
                var currentTarget = Svc.Targets.Target;
                // if (ImGui.Button($"Name: {currentTarget.Name}"))
                if (ImGui.Button($"名称: {currentTarget.Name}"))
                {
                    ImGui.SetClipboardText(currentTarget.Name.ToString());
                }
                // if (ImGui.Button($"Id: {currentTarget.BaseId}"))
                if (ImGui.Button($"Id: {currentTarget.BaseId}"))
                {
                    ImGui.SetClipboardText(currentTarget.BaseId.ToString());
                }
                // if (ImGui.Button($"Position: X: {currentTarget.Position.X:N2}, Y: {currentTarget.Position.Y:N2}, Z: {currentTarget.Position.Z:N2}"))
                if (ImGui.Button($"位置: X: {currentTarget.Position.X:N2}, Y: {currentTarget.Position.Y:N2}, Z: {currentTarget.Position.Z:N2}"))
                {
                    ImGui.SetClipboardText($"{currentTarget.Position.X:N2}f, {currentTarget.Position.Y:N2}f, {currentTarget.Position.Z:N2}f");
                }
                // ImGui.Text($"Distance: {Player.DistanceTo(currentTarget):N2}");
                ImGui.Text($"距离: {Player.DistanceTo(currentTarget):N2}");
            }

            // ImGui.Text($"Items on person: ");
            ImGui.Text($"身上物品: ");
            foreach (var item in ConsumableInfo.GatherFood)
            {
                if (PlayerHelper.GetItemCount(item.Id, out var count) && count > 0)
                    ImGui.Text($"{item.Name} | {item.Id}");
            }
            // if (ImGui.Button("Use Gathering Food"))
            if (ImGui.Button("使用采集食物"))
            {
                P.TaskManager.Enqueue(() => Task_Gather.UseFood());
            }
            // if (ImGui.Button("Set all leveling missions"))
            if (ImGui.Button("设置所有升级任务"))
            {
                foreach (var mission in C.MissionConfig)
                {
                    if (!CosmicHelper.QuickLevelList.Contains(mission.Key))
                        mission.Value.Enabled = false;
                    else
                        mission.Value.Enabled = true;
                }
                C.SaveDebounced();
            }

            // ImGui.SliderUInt("Player Level", ref playerLevel, 10, 100);
            ImGui.SliderUInt("玩家等级", ref playerLevel, 10, 100);
            // if (ImGui.Button("Update best mission"))
            if (ImGui.Button("更新最佳任务"))
            {
                best_LevelMission = LevelTest();
            }
            // ImGui.Text($"Best Mission for leveling: [{best_LevelMission}]");
            ImGui.Text($"最佳升级任务: [{best_LevelMission}]");


            ClassInfo();

            DroidCheck();

            // if (ImGui.CollapsingHeader("Test Picto"))
            if (ImGui.CollapsingHeader("测试 Picto"))
            {
                PictoManager.DrawPicto();
            }

            // ImGui.Text($"Drone Ready: {DroneReady()}");
            ImGui.Text($"无人机就绪: {DroneReady()}");

            // if (ImGui.Button("Use Drone"))
            if (ImGui.Button("使用无人机"))
            {
                UseDrone();
            }
            // if (ImGui.Button("Test Pathing to position"))
            if (ImGui.Button("测试寻路至位置"))
            {
                P.TaskManager.Enqueue(() => MovetoFlag());
            }
            ImGui.SameLine();
            // if (ImGui.Button($"Set position: {customDestination:N2}"))
            if (ImGui.Button($"设置位置: {customDestination:N2}"))
            {
                customDestination = Player.Position;
            }

            // ImGui.Text($"Any need repaired: {PlayerHelper.AnyNeedsRepair(99)}");
            ImGui.Text($"是否需要修理: {PlayerHelper.AnyNeedsRepair(99)}");
        }

        private static unsafe void ClassInfo()
        {
            // ImGui.Text("Manipulation Check");
            ImGui.Text("操控检查");
            Dictionary<uint, uint> ManipClassInfo = new()
            {
                [8] = 4574,
                [9] = 4575,
                [10] = 4576,
                [11] = 4577,
                [12] = 4578,
                [13] = 4579,
                [14] = 4580,
                [15] = 4581,
            };

            foreach (var job in ManipClassInfo)
            {
                var isUnlocked = ActionManager.Instance()->GetActionStatus(ActionType.Action, job.Value, checkRecastActive: false, checkCastingActive: false) is 574 or 586;
                // ImGui.Text($"JobId: {job.Key} | Unlocked: {isUnlocked}");
                ImGui.Text($"JobId: {job.Key} | 已解锁: {isUnlocked}");
                // 573 | Not unlocked??? 
                // 574 | Is unlocked
                // 586 | Is unlocked for current class/ready to use
            }

            var canUseSkill = ActionManager.Instance()->GetActionStatus(ActionType.Action, 272, checkRecastActive: false, checkCastingActive: false);
            // ImGui.Text($"Skill Status [272]: {canUseSkill}");
            ImGui.Text($"技能状态 [272]: {canUseSkill}");

            ImGui.Separator();
            PlayerHelper.UpdateHasManip();

            // ImGui.Text($"Custom Is Busy: {PlayerHelper.CustomIsBusy}");
            ImGui.Text($"自定义忙碌: {PlayerHelper.CustomIsBusy}");
            foreach (var job in PlayerHelper.ManipClassInfo)
            {
                // ImGui.Text($"JobID: {job.Key} | HasUnlocked: {job.Value.HasUnlocked}");
                ImGui.Text($"JobID: {job.Key} | 已解锁: {job.Value.HasUnlocked}");
            }
        }

        private static uint LevelTest()
        {
            uint bestMission = 0;

            foreach (var mission in C.MissionConfig)
            {
                var id = mission.Key;

                if (!CosmicHelper.QuickLevelList.Contains(id))
                    continue;

                if (CosmicHelper.SheetMissionDict.TryGetValue(id, out var missionInfo))
                {
                    var attribute = missionInfo.Attributes;
                    var missionLevel = missionInfo.Level;

                    // if (!missionInfo.Jobs.Contains((uint)Player.Job))
                        // continue;

                    int playerTier = playerLevel >= 90 ? 90 : playerLevel >= 50 ? 50 : 10;

                    if (missionLevel != playerTier)
                        continue;

                    bestMission = id;
                }
            }

            return bestMission;
        }

        private static void DroidCheck()
        {
            // if (ImGui.CollapsingHeader("Object info"))
            if (ImGui.CollapsingHeader("对象信息"))
            {
                foreach (var obect in Svc.Objects.OrderBy(x => Player.DistanceTo(x.Position)))
                {
                    // ImGui.Text($"Name: {obect.Name} : {obect.BaseId}");
                    ImGui.Text($"名称: {obect.Name} : {obect.BaseId}");
                }
            }
        }

        private static unsafe bool DroneReady()
        {
            var actionManager = ActionManager.Instance();

            // For regular items
            uint itemId = 50414; // your item ID
            var actionStatus = actionManager->GetActionStatus(
                ActionType.Item,
                itemId
            );

            // actionStatus == 0 means the item is ready to use
            // any other value indicates it's not ready (on cooldown, requirements not met, etc.)
            if (actionStatus == 0)
            {
                return true;
            }
            else if (Task_ArtifactSearch.IsTreasureDetected())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private static unsafe void UseDrone()
        {
            uint itemId = 50414;
            var inventoryManager = InventoryManager.Instance();

            // Array of inventory types to check
            var inventoryTypes = new[]
            {
                InventoryType.Inventory1,
                InventoryType.Inventory2,
                InventoryType.Inventory3,
                InventoryType.Inventory4
            };

            foreach (var invType in inventoryTypes)
            {
                var container = inventoryManager->GetInventoryContainer(invType);
                if (container == null) continue;

                for (int i = 0; i < container->Size; i++)
                {
                    var item = container->GetInventorySlot(i);
                    if (item != null && item->ItemId == itemId)
                    {
                        // Use the item from inventory
                        AgentInventoryContext.Instance()->UseItem(item->ItemId, invType, (uint)i, 0);
                        return;
                    }
                }
            }

            // If we get here, item wasn't found
            PluginLog.Warning($"Item {itemId} not found in any inventory container");
        }

        private static Vector3 customDestination = Vector3.Zero;

        private static bool? MovetoFlag()
        {
            if (Task_NavmeshMove.Task_NavTo(customDestination, stayMounted: true).Value)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static unsafe uint WorldProgress()
        {
            var wks = WKSManager.Instance();
            if (wks == null)
                return 0;

            return wks->State.DevGrade;
        }
    }
}
