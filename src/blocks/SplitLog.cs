using AncientTools.BlockEntities;
using AncientTools.Items;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace AncientTools.Blocks
{
    class BlockSplitLog: Block
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (blockSel.SelectionBoxIndex != 0)
            {
                ItemSlot activeSlot = byPlayer.InventoryManager.ActiveHotbarSlot;

                if(activeSlot.Empty || activeSlot.Itemstack?.Collectible is ItemWedge)
                {
                    if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BESplitLog splitLogEntity)
                    {
                        return splitLogEntity.OnInteract(byPlayer, blockSel.SelectionBoxIndex - 1, blockSel.HitPosition);
                    }
                }
            }

            return false;
        }
        public override float OnGettingBroken(IPlayer player, BlockSelection blockSel, ItemSlot itemslot, float remainingResistance, float dt, int counter)
        {
            if (blockSel.SelectionBoxIndex == 0 || itemslot.Itemstack?.Collectible.GetType() != typeof(ItemMallet))
                return base.OnBlockBreaking(player, blockSel, itemslot, remainingResistance, dt, counter);

            return remainingResistance;
        }
    }
}
