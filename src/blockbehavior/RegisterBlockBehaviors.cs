using Vintagestory.API.Common;

namespace AncientTools.BlockBehaviors
{
    class RegisterBlockBehaviors: ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            api.RegisterBlockBehaviorClass("ConvertToResinLog", typeof(BlockBehaviorConvertToResinLog));
        }
    }
}
