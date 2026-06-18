using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using ICE.Utilities.Cosmic_Helper;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;

namespace ICE.Scheduler.Handlers
{
    internal static class GearsetHandler // Borrowed from Artisan
    {
        internal unsafe static void TaskClassChange(Job job)
        {
            if (job == Player.Job || !EzThrottler.Throttle("Gearset", 250) || Player.IsBusy)
                return;
            var gearsets = RaptureGearsetModule.Instance();
            foreach (ref var gs in gearsets->Entries)
            {
                if (!RaptureGearsetModule.Instance()->IsValidGearset(gs.Id)) continue;
                if ((Job)gs.ClassJob == job)
                {
                    if (gs.Flags.HasFlag(RaptureGearsetModule.GearsetFlag.MainHandMissing))
                    {
                        if (GenericHelpers.TryGetAddonMaster<SelectYesno>("SelectYesno", out var select) && select.IsAddonReady)
                        {
                            select.Yes();
                        }
                        else
                        {
                            gearsets->EquipGearset(gs.Id);
                        }
                    }

                    var result = gearsets->EquipGearset(gs.Id);
                    // IceLogging.Debug($"Tried to equip gearset {gs.Id} for {job}, result={result}, flags={gs.Flags}");
                    IceLogging.Debug($"已尝试为 {job} 装备配装 {gs.Id}，result={result}，flags={gs.Flags}");
                    return;
                }
            }

            if (EzThrottler.Throttle("No gearsets"))
                // IceLogging.Verbose($"Hewwo. We have gotten thiws faw, which means thawt the geawset fow {job.ToString()} doesn't exist. Pwease make owne", "Task: Equip Gearset");
                IceLogging.Verbose($"你好呀。我们都走到这一步了，这意味着 {job.ToString()} 的配装并不存在。请创建一个吧", "Task: Equip Gearset");
            return;
        }
    }
}