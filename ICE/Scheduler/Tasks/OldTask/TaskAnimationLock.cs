using Dalamud.Game.ClientState.Conditions;

namespace ICE.Scheduler.Tasks.OldTask
{
    internal static class TaskAnimationLock
    {
        public static void Enqueue()
        {   
            /*
            if (Svc.Condition[ConditionFlag.NormalConditions] || Svc.Condition[ConditionFlag.ExecutingCraftingAction] || AddonHelper.IsAddonActive("RecipeNote") || AddonHelper.IsAddonActive("WKSRecipeNotebook"))
            {
                // IceLogging.Info("We were in Animation Lock fix state and seem to be fixed. Reseting.");
                IceLogging.Info("此前处于动画锁定修复状态，现已修复，正在重置。");
                SchedulerMain.State = IceState.GrabMission;
                SchedulerMain.PossiblyStuck = 0;
                SchedulerMain.AnimationLockAbandonState = false;
            }
            else
            {
                if (EzThrottler.Throttle("Open Recipe", 1000))
                    AddonHelper.OpenRecipeNote();
            }
            */
        }
    }
}