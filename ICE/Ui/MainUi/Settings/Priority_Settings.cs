using Dalamud.Interface;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.ImGuiTools;

namespace ICE.Ui.MainUi.Settings
{
    internal class Priority_Settings
    {
        public static void Draw()
        {
            if (ImGui.BeginTabBar("Mission Priority Settings"))
            {
                // if (ImGui.BeginTabItem("Mission Priority Order"))
                if (ImGui.BeginTabItem("任务优先级顺序"))
                {
                    MissionTypeOrderUi();

                    ImGui.EndTabItem();
                }

                // if (ImGui.BeginTabItem("Provisional: Type Order"))
                if (ImGui.BeginTabItem("临时任务：类型顺序"))
                {
                    TypePriorityUi();

                    ImGui.EndTabItem();
                }

                // if (ImGui.BeginTabItem("Provisional: Job Order"))
                if (ImGui.BeginTabItem("临时任务：职业顺序"))
                {
                    JobPriorityUi();

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }

        private static ImGuiEx.RealtimeDragDrop<ProvisionalTypes>? _dragDrop_ProvisionalType;

        private static void TypePriorityUi()
        {
            _dragDrop_ProvisionalType ??= new ImGuiEx.RealtimeDragDrop<ProvisionalTypes>(
                "ProvisionalTypeDragDrop",
                (info) => $"{info}_{info.GetHashCode()}",
                smallButton: false
            );

            _dragDrop_ProvisionalType.Begin();

            if (ImGui.BeginTable("Type Priority Table", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders))
            {
                // ImGui.TableSetupColumn("ReOrder");
                // ImGui.TableSetupColumn("Icon");
                // ImGui.TableSetupColumn("Type");
                ImGui.TableSetupColumn("排序");
                ImGui.TableSetupColumn("图标");
                ImGui.TableSetupColumn("类型");

                ImGui.TableHeadersRow();

                for (int i = 0; i < C.MissionPrio.Count; i++)
                {
                    ImGui.PushID(i);
                    var entry = C.MissionPrio[i];

                    ImGui.TableNextRow();
                    _dragDrop_ProvisionalType.NextRow();
                    _dragDrop_ProvisionalType.SetRowColor(entry);

                    ImGui.TableSetColumnIndex(0);
                    _dragDrop_ProvisionalType.DrawButtonDummy(entry, C.MissionPrio, i);

                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    FontAwesomeIcon icon = entry switch
                    {
                        ProvisionalTypes.ProvisionalTimed => FontAwesomeIcon.Clock,
                        ProvisionalTypes.ProvisionalSequential => FontAwesomeIcon.ListOl,
                        ProvisionalTypes.ProvisionalWeather => FontAwesomeIcon.Cloud,
                        _ => FontAwesomeIcon.Question,
                    };

                    ImGuiEx.Icon(icon);

                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    string type = entry switch
                    {
                        // ProvisionalTypes.ProvisionalTimed => "Timed",
                        // ProvisionalTypes.ProvisionalSequential => "Sequence",
                        // ProvisionalTypes.ProvisionalWeather => "Weather",
                        ProvisionalTypes.ProvisionalTimed => "限时",
                        ProvisionalTypes.ProvisionalSequential => "序列",
                        ProvisionalTypes.ProvisionalWeather => "天气",
                        _ => entry.ToString()
                    };
                    ImGui.Text($"{type}");

                    ImGui.PopID();
                }

                ImGui.EndTable();
            }

            _dragDrop_ProvisionalType.End();
        }

        private static ImGuiEx.RealtimeDragDrop<MissionTypes>? _dragDrop_MissionType;

        private static void MissionTypeOrderUi()
        {
            // ImGui.Text("Mission Search Priority");
            ImGui.Text("任务搜索优先级");
            // ImGui_Ice.IconWithTooltip(
            //     FontAwesomeIcon.InfoCircle, 
            //     "Order you would like to do the actions. It will work from the top down.\n" +
            //     "So if you Have Red Arert -> Drone Search, if a red alert isn't available, it will proceed to use a drone box if it can");
            ImGui_Ice.IconWithTooltip(
                FontAwesomeIcon.InfoCircle,
                "按从上到下的顺序执行操作。\n" +
                "例如设为 紧急通告 → 无人机搜索 时，若无可用紧急通告，则会尝试使用无人机箱。");

            _dragDrop_MissionType ??= new ImGuiEx.RealtimeDragDrop<MissionTypes>(
                "MissionTypeDragDrop",
                (info) => $"{info}_{info.GetHashCode()}",
                smallButton: false
            );

            _dragDrop_MissionType.Begin();

            if (ImGui.BeginTable("Mission Type Table", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders))
            {
                // ImGui.TableSetupColumn("ReOrder");
                // ImGui.TableSetupColumn("Icon");
                // ImGui.TableSetupColumn("Type");
                ImGui.TableSetupColumn("排序");
                ImGui.TableSetupColumn("图标");
                ImGui.TableSetupColumn("类型");

                ImGui.TableHeadersRow();

                for (int i = 0; i < C.MissionTypePrio.Count; i++)
                {
                    ImGui.PushID(i);
                    var entry = C.MissionTypePrio[i];

                    ImGui.TableNextRow();
                    _dragDrop_MissionType.NextRow();
                    _dragDrop_MissionType.SetRowColor(entry);

                    ImGui.TableSetColumnIndex(0);
                    _dragDrop_MissionType.DrawButtonDummy(entry, C.MissionTypePrio, i);

                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    FontAwesomeIcon icon = entry switch
                    {
                        MissionTypes.DroneSearch => FontAwesomeIcon.Satellite,
                        MissionTypes.Critical => FontAwesomeIcon.Bell,
                        MissionTypes.Provisional => FontAwesomeIcon.HourglassHalf,
                        MissionTypes.Standard => FontAwesomeIcon.Star,
                        MissionTypes.ToolMastery => FontAwesomeIcon.Meteor,
                        _ => FontAwesomeIcon.Question
                    };
                    ImGuiEx.Icon(icon);

                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    string name = entry switch
                    {
                        // MissionTypes.DroneSearch => "Drone Search",
                        // MissionTypes.Critical => "Red Alert",
                        // MissionTypes.Provisional => "Provisional Missions [Weather/Timed/Sequence]",
                        // MissionTypes.Standard => "Standard Missions [A->D]",
                        MissionTypes.DroneSearch => "无人机搜索",
                        MissionTypes.Critical => "紧急通告",
                        MissionTypes.Provisional => "临时任务 [天气/限时/序列]",
                        MissionTypes.Standard => "标准任务 [A->D]",
                        _ => $"{entry}"
                    };
                    ImGui.Text($"{name}");
                    if (entry == MissionTypes.DroneSearch && !C.Cosmodrone_Run)
                    {
                        ImGui.SameLine();
                        // ImGui_Ice.IconWithTooltip(FontAwesomeIcon.ExclamationTriangle,
                        //     "Finding drone locations is turned off, so we're just going to ignore this. If you want to run this, please enable it");
                        ImGui_Ice.IconWithTooltip(FontAwesomeIcon.ExclamationTriangle,
                            "无人机定位已关闭，此项将被忽略。如需启用，请打开相应设置");
                    }

                    ImGui.PopID();
                }

                ImGui.EndTable();
            }

            _dragDrop_MissionType.End();
        }

        private static ImGuiEx.RealtimeDragDrop<uint>? _dragDrop_JobPrio;

        private static void JobPriorityUi()
        {
            // ImGui.Text("Provisional Job Priority");
            ImGui.Text("临时任务职业优先级");
            // ImGui_Ice.IconWithTooltip(FontAwesomeIcon.InfoCircle,
            //     "Order you would like to do the provisional mission in, if multiple are selected and the option to do multiple classes is enabled");
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.InfoCircle,
                "当启用多职业选项且选中多个临时任务时，按此顺序执行。");

            bool provisionalAllJobs = C.GrindAllProvisionals;
            // if (ImGui_Ice.SliderButton("##Provisional_AllJobsToggle", "Allow for all Provisional Jobs", ref provisionalAllJobs))
            if (ImGui_Ice.SliderButton("##Provisional_AllJobsToggle", "允许所有临时任务职业", ref provisionalAllJobs))
            {
                C.GrindAllProvisionals = provisionalAllJobs;
                C.Save();
            }

            _dragDrop_JobPrio ??= new ImGuiEx.RealtimeDragDrop<uint>(
                "JobPrioDragDrop",
                (info) => ($"{info}_{info.GetHashCode()}"),
                smallButton: false
            );

            _dragDrop_JobPrio.Begin();

            if (ImGui.BeginTable("Job Priority Order", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders))
            {
                // ImGui.TableSetupColumn("ReOrder");
                // ImGui.TableSetupColumn("Icon");
                // ImGui.TableSetupColumn("Type");
                ImGui.TableSetupColumn("排序");
                ImGui.TableSetupColumn("图标");
                ImGui.TableSetupColumn("类型");

                ImGui.TableHeadersRow();

                for (int i = 0; i < C.JobPrio.Count(); i++)
                {
                    ImGui.PushID(i);

                    var entry = C.JobPrio[i];
                    ImGui.TableNextRow();
                    _dragDrop_JobPrio.NextRow();
                    _dragDrop_JobPrio.SetRowColor(entry);

                    ImGui.TableSetColumnIndex(0);
                    _dragDrop_JobPrio.DrawButtonDummy(entry, C.JobPrio, i);

                    ImGui.TableNextColumn();
                    if (CosmicHelper.ClassInfoDict.TryGetValue(entry, out var icon))
                    {
                        ImGui.Image(icon.JobIcon.GetWrapOrEmpty().Handle, new(24, 24));
                    }

                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text($"{GetJobName(entry)}");

                    ImGui.PopID();
                }

                ImGui.EndTable();
            }

            _dragDrop_JobPrio.End();
        }

        // Job name helper
        private static string GetJobName(uint jobId)
        {
            return jobId switch
            {
                // 8 => "Carpenter",
                // 9 => "Blacksmith",
                // 10 => "Armorer",
                // 11 => "Goldsmith",
                // 12 => "Leatherworker",
                // 13 => "Weaver",
                // 14 => "Alchemist",
                // 15 => "Culinarian",
                // 16 => "Miner",
                // 17 => "Botanist",
                // 18 => "Fisher",
                // _ => "Unknown Job"
                8 => "刻木师",
                9 => "锻铁匠",
                10 => "铸甲匠",
                11 => "雕金匠",
                12 => "制革匠",
                13 => "裁衣匠",
                14 => "炼金术士",
                15 => "烹调师",
                16 => "采矿工",
                17 => "园艺工",
                18 => "捕鱼人",
                _ => "未知职业"
            };
        }
    }
}
