using Vintagestory.API.Common;

namespace AncientTools.EntityBehaviors
{
    class RegisterEntityBehaviors: ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            api.RegisterEntityBehaviorClass("HealthNoRecover", typeof(EntityBehaviorHealthNoRecover));
        }
    }
}
