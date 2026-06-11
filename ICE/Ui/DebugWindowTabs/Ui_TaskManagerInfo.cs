using ICE.Utilities.Cosmic_Helper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Ui_TaskManagerInfo
    {
        private static uint mission = 0;
        private static List<Vector3> pathTo = new List<Vector3>();
        private static Vector3 pathToArea = new Vector3();

        public static void Draw()
        {
            ImGui.Text($"正在运行任务: {P.TaskManager.NumQueuedTasks != 0} | 队列中的任务数量: {P.TaskManager.NumQueuedTasks}");
            string currentTask = P.TaskManager.CurrentTask?.Name ?? "";
            ImGui.Text($"当前运行的任务: {currentTask}");
            ImGui.Text($"当前状态: {SchedulerMain.State}");
            ImGui.Text($"任务数量: {P.TaskManager.Tasks.Count}");
            if (ImGui.Button("将状态设为空闲"))
            {
                SchedulerMain.State = IceState.Idle; 
            }

            if (ImGui.Button("停止任务"))
            {
                P.TaskManager.Tasks.Clear();
                P.TaskManager.Abort();
            }

            ImGui.SetNextItemWidth(100);
            ImGui.InputUInt("任务", ref mission);

            if (ImGui.Button("放弃任务"))
            {
                Task_AbandonMission.Enqueue();
            }
            if (ImGui.Button("寻路到修理 NPC"))
            {
                P.TaskManager.Enqueue(() => Task_Repair.Repair_PathTo(), "Pathing to repair NPC");
            }
            if (ImGui.Button("测试修理功能"))
            {
                Task_Repair.Enqueue();
            }
            ImGui.Text($"当前路点列表数量: {pathTo.Count}");

            ImGui.SetNextItemWidth(250);
            ImGui.InputFloat3("目的地", ref pathToArea);
            if (ImGui.Button("设置区域"))
            {
                pathToArea = ECommons.GameHelpers.Player.Position;
            }
            if (ImGui.Button("创建路点列表"))
            {
                Vector3 currentPos = ECommons.GameHelpers.Player.Position;

                // Fire and forget - this will update pathTo when complete
                _ = Task.Run(async () =>
                {
                    pathTo = await FindTask(currentPos);
                });
            }
            if (ImGui.Button("测试制作"))
            {
                Task_Craft.Enqueue();
            }
            if (ImGui.Button("测试采集目标选择"))
            {
                Task_Gather.Enqueue();
            }
            if (ImGui.Button("从商店购买物品"))
            {
                Task_BuyCosmoItems.Enqueue();
            }

            if (ImGui.Button("测试无人机购买物品"))
            {
                Task_ArtifactSearch.EnqueueBuy();
            }
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
