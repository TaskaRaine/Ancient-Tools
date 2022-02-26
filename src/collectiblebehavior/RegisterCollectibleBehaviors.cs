using Vintagestory.API.Common;

namespace AncientTools.CollectibleBehaviors
{
    class RegisterCollectibleBehaviors: ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterCollectibleBehaviorClass("SalveIngredient", typeof(CollectibleBehaviorSalveIngredient));
            api.RegisterCollectibleBehaviorClass("ConvertHide", typeof(CollectibleBehaviorConvertHide));
        }
    }
}
