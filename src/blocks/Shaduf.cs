using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace AncientTools.Blocks
{
    public class BlockShadufBase: Block
    {

        public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ItemStack byItemStack = null)
        {
            BlockPos positionAbove = blockPos.UpCopy(1);
            BlockPos positionTwoAbove = blockPos.UpCopy(2);
            BlockPos positionThreeAbove = blockPos.UpCopy(3);

            if (!world.BlockAccessor.GetBlock(positionAbove).IsReplacableBy(this) || !world.BlockAccessor.GetBlock(positionTwoAbove).IsReplacableBy(this) || !world.BlockAccessor.GetBlock(positionThreeAbove).IsReplacableBy(this))
                return;

            GenerateShadufParts(positionAbove, positionTwoAbove, positionThreeAbove);

            base.OnBlockPlaced(world, blockPos, byItemStack);
        }
        private void GenerateShadufParts(BlockPos positionAbove, BlockPos positionTwoAbove, BlockPos positionThreeAbove)
        {
            api.World.BlockAccessor.SetBlock(api.World.GetBlock(this.CodeWithPart("middle", 2)).Id, positionAbove);
            api.World.BlockAccessor.SetBlock(api.World.GetBlock(this.CodeWithPart("top", 2)).Id, positionTwoAbove);
            api.World.BlockAccessor.SetBlock(api.World.GetBlock(this.CodeWithPart("arm", 2)).Id, positionThreeAbove);
        }
    }
}
