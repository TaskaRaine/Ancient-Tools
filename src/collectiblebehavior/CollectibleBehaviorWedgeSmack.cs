using AncientTools.BlockEntities;
using AncientTools.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace AncientTools.CollectibleBehaviors
{
    class CollectibleBehaviorWedgeSmack: CollectibleBehavior
    {
        public CollectibleBehaviorWedgeSmack(CollectibleObject collObj) : base(collObj)
        {

        }
        //-- Unfortunately, OnHeldAttack behavior functions never get called by the CollectibleObject and cannot be used. Conclusion: Hammers can't be used in place of mallets. --//
        //-- Perhaps bring this issue up with VS devs? --//
        public override void OnHeldAttackStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandHandling handHandling, ref EnumHandling handling)
        {
            if (blockSel.SelectionBoxIndex == 0 || byEntity.World.BlockAccessor.GetBlock(blockSel.Position).GetType() != typeof(BlockSplitLog))
            {
                base.OnHeldAttackStart(slot, byEntity, blockSel, entitySel, ref handHandling, ref handling);

                return;
            }

            if (byEntity.World.BlockAccessor.GetBlockEntity(blockSel.Position) is BESplitLog splitLogEntity)
            {
                handling = EnumHandling.PreventDefault;

                splitLogEntity.SmackWedge(blockSel.SelectionBoxIndex - 1, byEntity as IPlayer);
            }
        }
    }
}
