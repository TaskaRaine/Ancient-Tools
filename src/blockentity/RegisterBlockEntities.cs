using Vintagestory.API.Common;

namespace AncientTools.BlockEntity
{
    class RegisterBlockEntities: ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterBlockEntityClass("BEMortar", typeof(BEMortar));
            api.RegisterBlockEntityClass("BECuringRack", typeof(BECuringRack));
        }
    }
}
