using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace AncientTools.BlockBehaviors
{
    class BlockBehaviorATHarvestable: BlockBehaviorHarvestable
    {
        public BlockBehaviorATHarvestable(Block block) : base(block)
        {

        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            if (harvestedStack != null)
            {
                if(world.BlockAccessor.BreakDecor(blockSel.Position))
                    world.BlockAccessor.MarkChunkDecorsModified(blockSel.Position);
            }

            return base.OnBlockInteractStart(world, byPlayer, blockSel, ref handling);
        }
    }
}
