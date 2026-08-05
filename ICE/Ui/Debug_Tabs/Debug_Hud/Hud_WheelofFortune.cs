using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;

namespace ICE.Ui.Debug_Tabs.Debug_Hud
{
    internal class Hud_WheelofFortune
    {
        public static void Draw()
        {
            // if (ImGui.Button($"Auto Gamba"))
            if (ImGui.Button($"自动好运道"))
            {
                Task_Gamba.Enqueue();
            }

            if (GenericHelpers.TryGetAddonMaster<WKSLottery>("WKSLottery", out var lotto) && lotto.IsAddonReady)
            {
                // ImGui.Text($"Lottery addon is visible!");
                ImGui.Text($"抽奖界面已可见！");

                // if (ImGui.Button($"Left wheel select"))
                if (ImGui.Button($"选择左轮"))
                {
                    Task_Gamba.SelectWheelLeft(lotto);
                }
                ImGui.SameLine();

                // if (ImGui.Button($"Right wheel select"))
                if (ImGui.Button($"选择右轮"))
                {
                    Task_Gamba.SelectWheelRight(lotto);
                }

                ImGui.SameLine();
                // if (ImGui.Button($"Confirm"))
                if (ImGui.Button($"确认"))
                {
                    lotto.ConfirmButton();
                }

                // if (ImGui.Button($"Auto Gamba"))
                if (ImGui.Button($"自动好运道"))
                {
                    Task_Gamba.Enqueue();
                }

                // ImGui.Text($"Items in left wheel");
                ImGui.Text($"左轮物品");
                foreach (var l in lotto.LeftWheelItems)
                {
                    // ImGui.Text($"Name: {l.itemName} | Id: {l.itemId} | Amount: {l.itemAmount}");
                    ImGui.Text($"名称: {l.itemName} | Id: {l.itemId} | 数量: {l.itemAmount}");
                }

                ImGui.Spacing();
                foreach (var m in lotto.RightWheelItems)
                {
                    // ImGui.Text($"Name: {m.itemName} | Id: {m.itemId} | Amount: {m.itemAmount}");
                    ImGui.Text($"名称: {m.itemName} | Id: {m.itemId} | 数量: {m.itemAmount}");
                }
            }
            else
            {
                // ImGui.Text("Waiting for \"WKSLottery\" to be visible");
                ImGui.Text("等待 \"WKSLottery\" 界面可见");
            }
        }
    }
}
