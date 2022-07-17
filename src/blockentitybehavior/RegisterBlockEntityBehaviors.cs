using Vintagestory.API.Common;

namespace AncientTools.BlockEntityBehaviors
{
    class RegisterBlockEntityBehaviors: ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            api.RegisterBlockEntityBehaviorClass("FireproofFuel", typeof(BlockEntityBehaviorFireproofFuel));
            api.RegisterBlockEntityBehaviorClass("SplitLog", typeof(BlockEntityBehaviorSplitLog));
        }
    }
}
