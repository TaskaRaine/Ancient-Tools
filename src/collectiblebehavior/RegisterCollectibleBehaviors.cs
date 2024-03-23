using Vintagestory.API.Common;

namespace AncientTools.CollectibleBehaviors
{
    class RegisterCollectibleBehaviors: ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterCollectibleBehaviorClass("ConvertBlockUsingIngredient", typeof(CollectibleBehaviorConvertBlockUsingIngredient));
            api.RegisterCollectibleBehaviorClass("ConvertHide", typeof(CollectibleBehaviorConvertHide));
            api.RegisterCollectibleBehaviorClass("WedgeSmack", typeof(CollectibleBehaviorWedgeSmack));
            api.RegisterCollectibleBehaviorClass("MobileStorageDestruction", typeof(CollectibleBehaviorMobileStorageDestruction));
            api.RegisterCollectibleBehaviorClass("MobileStorageRepair", typeof(CollectibleBehaviorMobileStorageRepair));
            api.RegisterCollectibleBehaviorClass("AttemptRepairClutter", typeof(CollectibleBehaviorAttemptRepairClutter));
        }
    }
}
