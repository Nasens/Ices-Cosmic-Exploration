using ICE.Utilities.Cosmic_Helper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ICE.Ui.Debug_Tabs.Debug_Ui
{
    internal class Ui_TaskManagerInfo
    {
        private static uint mission = 0;
        private static List<Vector3> pathTo = new List<Vector3>();
        private static Vector3 pathToArea = new Vector3();

        public static void Draw()
        {
            // ImGui.Text($"Running task: {P.TaskManager.NumQueuedTasks != 0} | Amount of queue'd task: {P.TaskManager.NumQueuedTasks}");
            ImGui.Text($"任务运行中: {P.TaskManager.NumQueuedTasks != 0} | 队列任务数: {P.TaskManager.NumQueuedTasks}");
            string currentTask = P.TaskManager.CurrentTask?.Name ?? "";
            // ImGui.Text($"Current task running: {currentTask}");
            ImGui.Text($"当前运行任务: {currentTask}");
            // ImGui.Text($"Current State: {SchedulerMain.State}");
            ImGui.Text($"当前状态: {SchedulerMain.State}");
            // ImGui.Text($"Task Count: {P.TaskManager.Tasks.Count}");
            ImGui.Text($"任务总数: {P.TaskManager.Tasks.Count}");
            // if (ImGui.Button("Set State to Idle"))
            if (ImGui.Button("设为空闲状态"))
            {
                SchedulerMain.State = IceState.Idle; 
            }

            // if (ImGui.Button("Stop Task"))
            if (ImGui.Button("停止任务"))
            {
                P.TaskManager.Tasks.Clear();
                P.TaskManager.Abort();
            }

            ImGui.SetNextItemWidth(100);
            // ImGui.InputUInt("Mission", ref mission);
            ImGui.InputUInt("任务", ref mission);

            // if (ImGui.Button("Abandon Mission"))
            if (ImGui.Button("放弃任务"))
            {
                Task_AbandonMission.Enqueue();
            }
            // if (ImGui.Button("Path to repair NPC"))
            if (ImGui.Button("寻路至修理 NPC"))
            {
                P.TaskManager.Enqueue(() => Task_Repair.Repair_PathTo(), "Pathing to repair NPC");
            }
            // if (ImGui.Button("Test Repair Function"))
            if (ImGui.Button("测试修理功能"))
            {
                Task_Repair.Enqueue();
            }
            // ImGui.Text($"Current waypoint list count: {pathTo.Count}");
            ImGui.Text($"当前路径点数量: {pathTo.Count}");

            ImGui.SetNextItemWidth(250);
            // ImGui.InputFloat3("Destination", ref pathToArea);
            ImGui.InputFloat3("目的地", ref pathToArea);
            // if (ImGui.Button("Set Area"))
            if (ImGui.Button("设为当前位置"))
            {
                pathToArea = ECommons.GameHelpers.Player.Position;
            }
            // if (ImGui.Button("Create waypoint list"))
            if (ImGui.Button("创建路径点列表"))
            {
                Vector3 currentPos = ECommons.GameHelpers.Player.Position;

                // Fire and forget - this will update pathTo when complete
                _ = Task.Run(async () =>
                {
                    pathTo = await FindTask(currentPos);
                });
            }
            // if (ImGui.Button("Test Crafting"))
            if (ImGui.Button("测试制作"))
            {
                Task_Craft.Enqueue();
            }
            // if (ImGui.Button("Test Gather Targeting"))
            if (ImGui.Button("测试采集目标"))
            {
                Task_Gather.Enqueue();
            }
            // if (ImGui.Button("Buy Items from shop"))
            if (ImGui.Button("从商店购买物品"))
            {
                Task_BuyCosmoItems.Enqueue();
            }

            // if (ImGui.Button("Test Drone Buy Item"))
            if (ImGui.Button("测试无人机购买"))
            {
                Task_ArtifactSearch.EnqueueBuy();
            }
            // if (ImGui.Button("Test Drone Pathing"))
            if (ImGui.Button("测试无人机寻路"))
            {
                P.TaskManager.Enqueue(() => Task_ArtifactSearch.CheckBoxStatus());
            }
        }

        private static async Task<List<Vector3>> FindTask(Vector3 currentPos)
        {
            IceLogging.DestinationLogs.Log(pathToArea);
            return await P.Navmesh.Pathfind(currentPos, pathToArea, false);
        }
    }
}
