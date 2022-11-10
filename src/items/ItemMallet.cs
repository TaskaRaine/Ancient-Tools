using AncientTools.BlockEntities;
using AncientTools.Blocks;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace AncientTools.Items
{
    class ItemMallet: Item
    {
        public override void OnHeldAttackStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandHandling handling)
        {
            IPlayer byPlayer = (byEntity as EntityPlayer)?.Player;
            if (byPlayer == null) return;

            if (blockSel == null)
            {
                if (entitySel == null)
                    return;
                else if(entitySel.Entity is EntityPlayer || entitySel.Entity is EntityTrader)
                {
                    if(api.Side == EnumAppSide.Server)
                        api.World.PlaySoundAt(new AssetLocation("ancienttools:sounds/item/bonk"), entitySel.Entity, byEntity as IPlayer, false, 2, 0.8f);
                    
                    handling = EnumHandHandling.PreventDefaultAction;
                }
                return;
            }

            if (blockSel.SelectionBoxIndex == 0 || byEntity.World.BlockAccessor.GetBlock(blockSel.Position).GetType() != typeof(BlockSplitLog))
            {
                base.OnHeldAttackStart(slot, byEntity, blockSel, entitySel, ref handling);

                return;
            }

            if (byEntity.World.BlockAccessor.GetBlockEntity(blockSel.Position) is BESplitLog splitLogEntity)
            {
                handling = EnumHandHandling.PreventDefaultAction;

                splitLogEntity.SmackWedge(blockSel.SelectionBoxIndex - 1, byEntity as IPlayer);
            }
        }
    }
}
