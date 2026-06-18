using ECommons.GameHelpers;
using ICE.Utilities.Cosmic_Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICE.Scheduler
{
    internal static class Task_HubActivities
    {
        public static bool RepairNpc = false;
        public static bool RelicTurnin = false;
        public static bool CosmoBuy = false;
        public static bool CanGamba = false;
        public static bool CanBuyDrones = false;
        private static Vector3 craftingSpot = Vector3.Zero;

        public static void Enqueue()
        {
            P.TaskManager.Enqueue(RegisterCraftingPosition, "Registering crafting position for later");
            P.TaskManager.Enqueue(Task_Repair.HubCheck, "Checking to see if we're in hub area");
            if (RepairNpc)
            {
                P.TaskManager.EnqueueMulti
                (
                    // new(() => IceLogging.Info("Starting repair task at the npc", "Task_HubActivities")),
                    new(() => IceLogging.Info("在 NPC 处开始修理任务", "Task_HubActivities")),
                    new(Task_Repair.Repair_PathTo, "Pathing to the repair NPC"),
                    new(Task_Repair.RepairAtNpc, "Repairing at the NPC Vendor"),
                    new(Task_Repair.CloseRepair, "Closing the repair window")
                );
            }
            if (RelicTurnin)
            {
                // P.TaskManager.Enqueue(() => IceLogging.Info("Starting Relic Turnin task at the npc", "Task_HubActivities"));
                P.TaskManager.Enqueue(() => IceLogging.Info("在 NPC 处开始神器交付任务", "Task_HubActivities"));
                Task_RelicTurnin.Enqueue();
                // P.TaskManager.Enqueue(() => IceLogging.Info("Task_Relic turnin is Complete"));
                P.TaskManager.Enqueue(() => IceLogging.Info("Task_Relic 交付完成"));
            }
            if (CosmoBuy)
            {
                // P.TaskManager.Enqueue(() => IceLogging.Info("Starting Relic Turnin task at the npc", "Task_HubActivities"));;
                P.TaskManager.Enqueue(() => IceLogging.Info("在 NPC 处开始神器交付任务", "Task_HubActivities"));;
                Task_BuyCosmoItems.Enqueue();
            }
            if (CanGamba)
            {
                // P.TaskManager.Enqueue(() => IceLogging.Info("Starting Gamba task at the npc", "Task_HubActivities"));
                P.TaskManager.Enqueue(() => IceLogging.Info("在 NPC 处开始赌博任务", "Task_HubActivities"));
                Task_Gamba.Enqueue();
            }
            if (CanBuyDrones)
            {
                // P.TaskManager.Enqueue(() => IceLogging.Info("Starting the drone buying", "Task_HubActivities"));
                P.TaskManager.Enqueue(() => IceLogging.Info("开始购买无人机", "Task_HubActivities"));
                Task_ArtifactSearch.EnqueueBuy();
            }
            P.TaskManager.EnqueueMulti
            (
                new(() => ResetAll(), "Setting all task to false"),
                // new(() => IceLogging.Info("Checking to see if we need to path back to the spot")),
                new(() => IceLogging.Info("检查是否需要寻路返回原位置")),
                new(PathBackToCraftingSpot, "Pathing back to our crafting spot", Utils.TaskConfig),
                new(() => SchedulerMain.State = IceState.Start, "Swapping back to start")
            );
        }

        public static bool? RegisterCraftingPosition()
        {
            if (CosmicHelper.CrafterJobList.Contains((uint)Player.Job))
                craftingSpot = Player.Position;

            return true;
        }

        public static bool? PathBackToCraftingSpot()
        {
            if (CosmicHelper.CrafterJobList.Contains((uint)Player.Job))
            {
                if (!Task_NavmeshMove.Task_NavTo(craftingSpot, true, 1, false).Value)
                {
                    return false;
                }
                else
                {
                    // IceLogging.Debug("We're back at our spot, so continuing on");
                    IceLogging.Debug("已回到原位置，继续执行");
                    return true;
                }
            }
            else
            {
                // IceLogging.Info($"We're not on a crafting job. (Allegedly) which means that we don't need to path back | Player Job: {(uint)Player.Job}");
                IceLogging.Info($"当前不是制作职业。（理论上）意味着无需寻路返回 | Player Job：{(uint)Player.Job}");
                return true;
            }
        }

        private static bool? ResetAll()
        {
            // IceLogging.Info("Resetting all hub task to false");
            IceLogging.Info("将所有 hub 任务重置为 false");

            RepairNpc = false;
            RelicTurnin = false;
            CosmoBuy = false;
            CanGamba = false;
            CanBuyDrones = false;
            CosmicHelper.Task_UpdateRelicMissionInfo();

            return true;
        }
    }
}
