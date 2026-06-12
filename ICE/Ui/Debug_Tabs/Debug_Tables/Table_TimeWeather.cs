using Lumina.Excel.Sheets;

namespace ICE.Ui.Debug_Tabs.Debug_Tables
{
    internal class Table_TimeWeather
    {
        public static unsafe void Draw()
        {
            var timeSheet = Svc.Data.GetExcelSheet<WKSMissionLotterySpecialCond>();

            if (ImGui.BeginTable($"WKSMission Time Sheet", 4, ImGuiTableFlags.SizingFixedFit))
            {
                // ImGui.TableSetupColumn("Key");
                ImGui.TableSetupColumn("键");
                // ImGui.TableSetupColumn("Weather Required");
                ImGui.TableSetupColumn("所需天气");
                // ImGui.TableSetupColumn("Start Hour");
                ImGui.TableSetupColumn("开始小时");
                // ImGui.TableSetupColumn("End Hour");
                ImGui.TableSetupColumn("结束小时");

                ImGui.TableHeadersRow();

                foreach (var entry in timeSheet)
                {
                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text($"{entry.RowId}");

                    ImGui.TableNextColumn();
                    ImGui.Text($"{entry.WeatherRequired.Value.Name}"); // Unknown 0

                    ImGui.TableNextColumn();
                    ImGui.Text($"{entry.StartTimeHour}"); // Unknown 1

                    ImGui.TableNextColumn();
                    ImGui.Text($"{entry.EndTimeHour}"); // Unknown 2

                }

                ImGui.EndTable();
            }
        }
    }
}
