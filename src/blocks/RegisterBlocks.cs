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
            api.RegisterBlockClass("BlockStretchingFrame", typeof(BlockStretchingFrame));
            api.RegisterBlockClass("BlockHideWaterSack", typeof(BlockHideWaterSack));
            api.RegisterBlockClass("BlockPitchContainer", typeof(BlockPitchContainer));
            api.RegisterBlockClass("BlockPitch", typeof(BlockPitch));
            api.RegisterBlockClass("BlockPitchInventory", typeof(BlockPitchInventory));
            api.RegisterBlockClass("BlockWaterproofTorch", typeof(BlockWaterproofTorch));
            api.RegisterBlockClass("BlockPitchTorch", typeof(BlockPitchTorch));
            api.RegisterBlockClass("BlockLampSaucer", typeof(BlockLampSaucer));

            base.Start(api);
        }
    }
}
