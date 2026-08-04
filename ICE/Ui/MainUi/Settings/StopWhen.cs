using ICE.Sounds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICE.Ui.MainUi.Settings
{
    internal class StopWhen
    {
        public static bool AnyStop => 
           C.StopOnceHitCosmicScore
        || C.StopWhenLevel
        || C.StopOnceHitCosmoCredits
        || C.StopOnceHitLunarCredits
        || C.StopOnceRelicFinished
        || C.StopOnceStandardMissionsGolded
        || C.StopWhenMasteryComplete;

        public static void Draw()
        {
            // ImGui.Checkbox("Stop after current mission", ref Mission_Settings.StopAfterCurrent);
            ImGui.Checkbox("当前任务完成后停止", ref Mission_Settings.StopAfterCurrent);

            #region CosmoCredits

            bool stopCosmic = C.StopOnceHitCosmoCredits;
            // if (ImGui.Checkbox($"Stop at Cosmic Credits", ref stopCosmic))
            if (ImGui.Checkbox($"达到宇宙点数时停止", ref stopCosmic))
            {
                C.StopOnceHitCosmoCredits = stopCosmic;
                C.Save();
            }

            ImGui.SameLine();
            int cosmicCap = C.CosmoCreditsCap;
            ImGui.SetNextItemWidth(200);
            if (ImGui.SliderInt("##CosmicStop", ref cosmicCap, 0, 30_000))
            {
                if (cosmicCap > 30000)
                    cosmicCap = 30000;
                else if (cosmicCap < 0)
                    cosmicCap = 0;

                C.CosmoCreditsCap = cosmicCap;
                C.SaveDebounced();
            }

            #endregion

            #region Planet Credits

            bool stopLunar = C.StopOnceHitLunarCredits;
            // if (ImGui.Checkbox($"Stop at Planetary Credit Amount", ref stopLunar))
            if (ImGui.Checkbox($"达到行星点数时停止", ref stopLunar))
            {
                C.StopOnceHitLunarCredits = stopLunar;
                C.Save();
            }

            ImGui.SameLine();

            int lunarCap = C.LunarCreditsCap;
            ImGui.SetNextItemWidth(200);
            if (ImGui.SliderInt("##LunarStop", ref lunarCap, 0, 10_000))
            {
                C.LunarCreditsCap = lunarCap;
                C.SaveDebounced();
            }

            #endregion

            #region Cosmic Score

            bool stopScore = C.StopOnceHitCosmicScore;
            // if (ImGui.Checkbox($"Stop at Cosmic Score", ref stopScore))
            if (ImGui.Checkbox($"达到宇宙分数时停止", ref stopScore))
            {
                C.StopOnceHitCosmicScore = stopScore;
                C.BuyItems = false;
                C.Save();
            }

            ImGui.SameLine();

            int scoreCap = C.CosmicScoreCap;
            ImGui.SetNextItemWidth(200);
            if (ImGui.InputInt("###ScoreStop", ref scoreCap, 10_000, 500_000))
            {
                C.CosmicScoreCap = scoreCap >= 0 ? scoreCap : 0;
                C.Save();
            }

            #endregion

            #region Level

            bool stopWhenLevel = C.StopWhenLevel;
            // if (ImGui.Checkbox($"Stop at Level", ref stopWhenLevel))
            if (ImGui.Checkbox($"达到等级时停止", ref stopWhenLevel))
            {
                C.StopWhenLevel = stopWhenLevel;
                C.Save();
            }

            ImGui.SameLine();

            int targetLevel = C.TargetLevel;
            ImGui.SetNextItemWidth(200);
            if (ImGui.SliderInt("##Level", ref targetLevel, 10, 100))
            {
                C.TargetLevel = targetLevel;
                C.SaveDebounced();
            }

            #endregion

            #region Relic Completed

            bool relicStop = C.StopOnceRelicFinished;
            // if (ImGui.Checkbox($"Stop @ Relic Complete", ref relicStop))
            if (ImGui.Checkbox($"Relic 完成时停止", ref relicStop))
            {
                C.StopOnceRelicFinished = relicStop;
                C.Save();
            }

            #endregion

            #region Relic Level

            bool stopWhen = C.StopAtRelicLv;
            // if (ImGui.Checkbox("Stop At Relic Lv.", ref stopWhen))
            if (ImGui.Checkbox("达到 Relic 等级时停止", ref stopWhen))
            {
                C.StopAtRelicLv = stopWhen;
                C.Save();
            }
            ImGui.SameLine();
            int relicLv = C.RelicLv;
            ImGui.SetNextItemWidth(150);
            if (ImGui.SliderInt("##RelicLvSlider", ref relicLv, 1, 20))
            {
                C.RelicLv = relicLv;
                C.SaveDebounced();
            }

            #endregion

            #region Mastery Score

            bool stopMastery = C.StopWhenMasteryComplete;
            // if (ImGui.Checkbox("Stop When Mastery Complete", ref stopMastery))
            if (ImGui.Checkbox("大师分数完成时停止", ref stopMastery))
            {
                C.StopWhenMasteryComplete = stopMastery;
                C.SaveDebounced();
            }
            ImGui.SameLine();
            int masteryScore = C.MasteryCap;
            ImGui.SetNextItemWidth(150);
            if (ImGui.SliderInt("##MasteryCapSlider", ref masteryScore, 0, 500_000))
            {
                C.MasteryCap = masteryScore;
                C.SaveDebounced();
            }


            #endregion

            #region Standard Missions Golded

            bool standardGoldStop = C.StopOnceStandardMissionsGolded;
            // if (ImGui.Checkbox("Stop when all standard missions are golded", ref standardGoldStop))
            if (ImGui.Checkbox("所有标准任务金牌后停止", ref standardGoldStop))
            {
                C.StopOnceStandardMissionsGolded = standardGoldStop;
                C.Save();
            }
            // ImGuiEx.HelpMarker(
            //     "Stops when every non-provisional, non-critical mission for your selected job on the current moon is gold.\n" +
            //     "Timed, weather, sequence, and red alert missions are not counted.");
            ImGuiEx.HelpMarker(
                "当前星球上所选职业的所有非临时、非紧急任务均为金牌时停止。\n" +
                "限时、天气、序列和紧急通告任务不计入。");

            #endregion

            #region Sound Alert

            bool playSoundAlert = C.PlaySoundAlert;
            // if (ImGui.Checkbox("Play Sound Alert on Stop", ref playSoundAlert))
            if (ImGui.Checkbox("停止时播放提示音", ref playSoundAlert))
            {
                C.PlaySoundAlert = playSoundAlert;
                C.Save();
            }
            if (playSoundAlert)
            {
                var soundVolume = C.SoundVolume;
                // ImGui.Text("Sound Volume");
                ImGui.Text("音量");
                ImGui.SetNextItemWidth(200);
                if (ImGui.SliderFloat("##Sound Volume", ref soundVolume, 0f, 1f, "%.2f"))
                {
                    C.SoundVolume = soundVolume;
                    C.SaveDebounced();
                }
                // if (ImGui.Button("Test Sound Alert"))
                if (ImGui.Button("测试提示音"))
                {
                    _ = SoundPlayer.PlaySoundAsync();
                }
            }

            #endregion
        }
    }
}
