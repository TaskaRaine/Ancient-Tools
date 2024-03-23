using AncientTools.BlockEntities;
using AncientTools.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace AncientTools.CollectibleBehaviors
{
    class CollectibleBehaviorWedgeSmack: CollectibleBehavior
    {
        public CollectibleBehaviorWedgeSmack(CollectibleObject collObj) : base(collObj)
        {

        }
        //-- Collectible held attack should work now! A patch is applied to AuthorativeAnimation behavior to allow it to go through--//
        public override void OnHeldAttackStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandHandling handHandling, ref EnumHandling handling)
        {
            IPlayer byPlayer = (byEntity as EntityPlayer)?.Player;
            if (byPlayer == null) return;

            if (blockSel == null)
            {
                if (entitySel == null)
                    return;
                else if (entitySel.Entity is EntityPlayer || entitySel.Entity is EntityTrader)
                {
                    if (byEntity.Api.Side == EnumAppSide.Server)
                        byEntity.Api.World.PlaySoundAt(new AssetLocation("ancienttools:sounds/item/bonk"), entitySel.Entity, byPlayer, false, 2, 0.8f);

                    handHandling = EnumHandHandling.PreventDefaultAction;
                }
                return;
            }

            handHandling = EnumHandHandling.NotHandled;

            if (blockSel.SelectionBoxIndex == 0 || byEntity.World.BlockAccessor.GetBlock(blockSel.Position).GetType() != typeof(BlockSplitLog))
            {
                base.OnHeldAttackStart(slot, byEntity, blockSel, entitySel, ref handHandling, ref handling);

                return;
            }

            if (byEntity.World.BlockAccessor.GetBlockEntity(blockSel.Position) is BESplitLog splitLogEntity)
            {
                handHandling = EnumHandHandling.PreventDefaultAction;   

                splitLogEntity.SmackWedge(blockSel.SelectionBoxIndex - 1, byPlayer);
                collObj.DamageItem(byEntity.Api.World, byEntity, slot, 1);
            }
        }
    }
}
