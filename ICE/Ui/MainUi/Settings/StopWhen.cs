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
        public static bool AnyStop => C.StopOnceHitCosmicScore
                                   || C.StopWhenLevel
                                   || C.StopOnceHitCosmoCredits
                                   || C.StopOnceHitLunarCredits
                                   || C.StopOnceRelicFinished;

        public static void Draw()
        {
            ImGui.Checkbox("完成当前任务后停止", ref Mission_Settings.StopAfterCurrent);

            #region CosmoCredits

            bool stopCosmic = C.StopOnceHitCosmoCredits;
            if (ImGui.Checkbox($"达到 Cosmic Credits 时停止", ref stopCosmic))
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
            if (ImGui.Checkbox($"达到星球点数数量时停止", ref stopLunar))
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
            if (ImGui.Checkbox($"达到 Cosmic 分数时停止", ref stopScore))
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
            if (ImGui.Checkbox($"Relic 完成时停止", ref relicStop))
            {
                C.StopOnceRelicFinished = relicStop;
                C.Save();
            }

            #endregion

            #region Relic Level

            bool stopWhen = C.StopAtRelicLv;
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

            #region Sound Alert

            bool playSoundAlert = C.PlaySoundAlert;
            if (ImGui.Checkbox("停止时播放提示音", ref playSoundAlert))
            {
                C.PlaySoundAlert = playSoundAlert;
                C.Save();
            }
            if (playSoundAlert)
            {
                var soundVolume = C.SoundVolume;
                ImGui.Text("音量");
                ImGui.SetNextItemWidth(200);
                if (ImGui.SliderFloat("##Sound Volume", ref soundVolume, 0f, 1f, "%.2f"))
                {
                    C.SoundVolume = soundVolume;
                    C.SaveDebounced();
                }
                if (ImGui.Button("测试提示音"))
                {
                    _ = SoundPlayer.PlaySoundAsync();
                }
            }

            #endregion
        }
    }
}
