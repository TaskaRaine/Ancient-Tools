using Vintagestory.API.Common;

namespace AncientTools.Entities
{
    class RegisterEntities: ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterEntity("EntityCart", typeof(EntityCart));
        }
    }
}
