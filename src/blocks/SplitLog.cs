using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace AncientTools.Blocks
{
    class BlockSplitLog: Block
    {
        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            if (!CanPlaceBlock(world, byPlayer, blockSel, ref failureCode))
            {
                return false;
            }

            BlockFacing[] horVer = SuggestedHVOrientation(byPlayer, blockSel);

            if (blockSel.Face.IsVertical)
            {
                horVer[1] = blockSel.Face;
            }
            else
            {
                horVer[0] = blockSel.Face;
            }

            AssetLocation blockCode;

            if (horVer[1] == null)
            {
                blockCode = CodeWithVariants(new string[] { "verticalorientation", "horizontalorientation" }, new string[] { "none", horVer[0].Code });
            }
            else
            {
                blockCode = CodeWithVariants(new string[] { "verticalorientation", "horizontalorientation" }, new string[] { horVer[1].Code, horVer[0].Code });
            }

            Block newBlock = world.BlockAccessor.GetBlock(blockCode);
            if (newBlock == null) return false;

            world.BlockAccessor.SetBlock(newBlock.BlockId, blockSel.Position);

            return true;
        }
    }
}
