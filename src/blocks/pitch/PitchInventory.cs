using AncientTools.BlockEntities;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace AncientTools.Blocks
{
    class BlockPitchInventory: Block
    {
        public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ItemStack byItemStack = null)
        {
            base.OnBlockPlaced(world, blockPos, byItemStack);

            if(api.Side == EnumAppSide.Server)
            {
                world.BlockAccessor.SetBlock(api.World.GetBlock(new AssetLocation("ancienttools", "pitchpot-" + this.VariantStrict["color"] + "-finishedpitch")).Id, blockPos);

                world.BlockAccessor.MarkBlockDirty(blockPos);

                if (api.World.BlockAccessor.GetBlockEntity(blockPos) is BEFinishedPitch finishedEntity)
                {
                    finishedEntity.InsertPitch();
                }
            }
        }
    }
}
