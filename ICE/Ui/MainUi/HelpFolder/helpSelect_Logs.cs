using Dalamud.Interface.Utility.Raii;
using ICE.Utilities.Cosmic_Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ICE.Utilities.Cosmic_Helper.IceLogging;

namespace ICE.Ui.MainUi.HelpFolder
{
    internal class helpSelect_Logs
    {
        private static string searchFilter = string.Empty;

        public static void Draw_Helper()
        {
            using (var headerChild = ImRaii.Child("##helpSelect_Logs", new Vector2(0, 0), true, ImGuiWindowFlags.NoScrollbar))
            {
                if (!headerChild.Success) return; // Ensures that it was loaded properly before continuing.
                if (ImGui.BeginTabBar("Ice Log Tabs"))
                {
                    // if (ImGui.BeginTabItem("Main Logs"))
                    if (ImGui.BeginTabItem("主日志"))
                    {
                        LogHelperViewer();
                        ImGui.EndTabItem();
                    }
                    // if (ImGui.BeginTabItem("Destination Logs"))
                    if (ImGui.BeginTabItem("目的地日志"))
                    {
                        DestinationLogViewer();
                        ImGui.EndTabItem();
                    }

                    ImGui.EndTabBar();
                }
            }
        }

        public static void Draw_Debug()
        {
            // if (ImGui.Button("Copy logs to clipboard"))
            if (ImGui.Button("复制日志到剪贴板"))
            {
                LogSystem.CopyToClipboard();
            }
            LogHelperViewer();
        }

        private static void LogHelperViewer()
        {
            // Search input
            ImGui.SetNextItemWidth(300);
            // ImGui.InputTextWithHint("##LogSearch", "Search logs...", ref searchFilter, 256);
            ImGui.InputTextWithHint("##LogSearch", "搜索日志...", ref searchFilter, 256);

            ImGui.SameLine();
            // if (ImGui.Button("Copy Logs"))
            if (ImGui.Button("复制日志"))
            {
                LogSystem.CopyToClipboard();
            }

            ImGui.Spacing();

            ImGuiTableFlags flags = ImGuiTableFlags.RowBg |
                ImGuiTableFlags.Borders |
                ImGuiTableFlags.ScrollY |
                ImGuiTableFlags.SizingFixedFit;

            if (ImGui.BeginTable("LogTable", 5, flags))
            {
                // ImGui.TableSetupColumn("Time");
                ImGui.TableSetupColumn("时间");
                // ImGui.TableSetupColumn("Count");
                ImGui.TableSetupColumn("次数");
                // ImGui.TableSetupColumn("Level");
                ImGui.TableSetupColumn("级别");
                // ImGui.TableSetupColumn("Category");
                ImGui.TableSetupColumn("分类");
                // ImGui.TableSetupColumn("Message", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("消息", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableHeadersRow();

                // Filter logs based on search input
                var filteredLogs = LogSystem.Logs.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(searchFilter))
                {
                    filteredLogs = filteredLogs.Where(log =>
                        log.Message.Contains(searchFilter, StringComparison.OrdinalIgnoreCase) ||
                        (log.Category?.Contains(searchFilter, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        log.Level.ToString().Contains(searchFilter, StringComparison.OrdinalIgnoreCase)
                    );
                }

                foreach (var log in filteredLogs.OrderByDescending(l => l.LastOccurrence))  // Changed from Timestamp
                {
                    ImGui.TableNextRow();

                    ImGui.TableNextColumn();
                    // Show time range if repeated
                    if (log.Count > 1)
                    {
                        ImGui.Text($"{log.Timestamp:HH:mm:ss}");
                        ImGui.SameLine();
                        ImGui.TextDisabled("-");
                        ImGui.SameLine();
                        ImGui.Text($"{log.LastOccurrence:HH:mm:ss}");
                    }
                    else
                    {
                        ImGui.Text(log.Timestamp.ToString("HH:mm:ss"));
                    }

                    ImGui.TableNextColumn();
                    // Show count if > 1
                    if (log.Count > 1)
                    {
                        ImGui.TextColored(new Vector4(1, 0.5f, 0, 1), $"x{log.Count}");  // Orange for visibility
                    }
                    else
                    {
                        ImGui.TextDisabled("1");
                    }

                    ImGui.TableNextColumn();
                    // Color-code by level
                    var color = log.Level switch
                    {
                        LogLevel.Error => new Vector4(1, 0, 0, 1),
                        LogLevel.Warning => new Vector4(1, 1, 0, 1),
                        LogLevel.Info => new Vector4(0, 1, 1, 1),
                        _ => new Vector4(0.7f, 0.7f, 0.7f, 1)
                    };
                    ImGui.TextColored(color, log.Level.ToString());

                    ImGui.TableNextColumn();
                    ImGui.Text(log.Category ?? "");

                    ImGui.TableNextColumn();
                    ImGui.Text(log.Message);
                }

                ImGui.EndTable();
            }
        }

        private static void DestinationLogViewer()
        {
            ImGuiTableFlags flags = ImGuiTableFlags.RowBg |
                                    ImGuiTableFlags.Borders |
                                    ImGuiTableFlags.ScrollY |
                                    ImGuiTableFlags.SizingFixedFit;

            if (ImGui.BeginTable("Destination Log Viewer", 5, flags))
            {
                // ImGui.TableSetupColumn("Timestamp");
                ImGui.TableSetupColumn("时间戳");
                // ImGui.TableSetupColumn("Start");
                ImGui.TableSetupColumn("起点");
                // ImGui.TableSetupColumn("Destination");
                ImGui.TableSetupColumn("目的地");
                // ImGui.TableSetupColumn("Distance");
                ImGui.TableSetupColumn("距离");

                ImGui.TableHeadersRow();

                var filteredLogs = DestinationLogs.Logs.AsEnumerable();
                var entryNumber = 0;

                foreach (var log in filteredLogs.OrderByDescending(l => l.Timestamp))
                {
                    ImGui.TableNextRow();

                    ImGui.PushID($"{log.PlayerDestination}_{entryNumber}");

                    ImGui.TableSetColumnIndex(0);
                    Table_VertCenterText(log.Timestamp.ToString("HH:mm:ss"));

                    ImGui.TableNextColumn();
                    Table_VertCenterText($"X: {log.PlayerStart.X:N2}, Y: {log.PlayerStart.Y:N2}, Z: {log.PlayerStart.Z:N2}");

                    ImGui.TableNextColumn();
                    Table_VertCenterText($"X: {log.PlayerDestination.X:N2}, Y: {log.PlayerDestination.Y:N2}, Z: {log.PlayerDestination.Z:N2}");

                    ImGui.TableNextColumn();
                    Table_VertCenterText($"{log.Distance}");

                    ImGui.TableNextColumn();
                    // if (ImGui.Button("Copy Info"))
                    if (ImGui.Button("复制信息"))
                    {
                        var clipboardText = new StringBuilder();
                        clipboardText.AppendLine($"Start: X: {log.PlayerStart.X:N2}, Y: {log.PlayerStart.Y:N2}, Z: {log.PlayerStart.Z:N2}");
                        clipboardText.Append($"End: X: {log.PlayerDestination.X:N2}, Y: {log.PlayerDestination.Y:N2}, Z: {log.PlayerDestination.Z:N2}");
                        ImGui.SetClipboardText($"{clipboardText}");
                        // Notify.Success("Log copied to clipbard");
                        Notify.Success("日志已复制到剪贴板");
                    }
                    ImGui.PopID();

                    entryNumber += 1;
                }

                ImGui.EndTable();
            }
        }
        private static void Table_VertCenterText(string text)
        {
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted(text);
        }
    }
}
