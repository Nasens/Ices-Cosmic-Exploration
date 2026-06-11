using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Hud_WheelofFortune
    {
        public static void Draw()
        {
            if (ImGui.Button($"自动转盘"))
            {
                Task_Gamba.Enqueue();
            }

            if (GenericHelpers.TryGetAddonMaster<WKSLottery>("WKSLottery", out var lotto) && lotto.IsAddonReady)
            {
                ImGui.Text($"抽奖插件可见!");

                if (ImGui.Button($"选择左转盘"))
                {
                    Task_Gamba.SelectWheelLeft(lotto);
                }
                ImGui.SameLine();

                if (ImGui.Button($"选择右转盘"))
                {
                    Task_Gamba.SelectWheelRight(lotto);
                }

                ImGui.SameLine();
                if (ImGui.Button($"确认"))
                {
                    lotto.ConfirmButton();
                }

                if (ImGui.Button($"自动转盘"))
                {
                    Task_Gamba.Enqueue();
                }

                ImGui.Text($"左转盘物品");
                foreach (var l in lotto.LeftWheelItems)
                {
                    ImGui.Text($"名称: {l.itemName} | ID: {l.itemId} | 数量: {l.itemAmount}");
                }

                ImGui.Spacing();
                foreach (var m in lotto.RightWheelItems)
                {
                    ImGui.Text($"名称: {m.itemName} | ID: {m.itemId} | 数量: {m.itemAmount}");
                }
            }
            else
            {
                ImGui.Text("等待 \"WKSLottery\" 可见");
            }
        }
    }
}
