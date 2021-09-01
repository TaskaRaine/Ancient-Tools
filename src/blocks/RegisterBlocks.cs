using Vintagestory.API.Common;

namespace AncientTools.Blocks
{
    class RegisterBlocks: ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            api.RegisterBlockClass("BlockMortar", typeof(BlockMortar));
            api.RegisterBlockClass("BlockCuringRack", typeof(BlockCuringRack));
            api.RegisterBlockClass("BlockSalveContainer", typeof(BlockSalveContainer));
            api.RegisterBlockClass("BlockUnmixedSalve", typeof(BlockUnmixedSalve));
            api.RegisterBlockClass("BlockSalve", typeof(BlockSalve));
            api.RegisterBlockClass("BlockBarkBasket", typeof(BlockBarkBasket));

            base.Start(api);
        }
    }
}
