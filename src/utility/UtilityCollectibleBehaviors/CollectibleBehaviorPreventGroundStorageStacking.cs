using System.Security.Cryptography.X509Certificates;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace AncientTools.Utility
{
    public class CollectibleBehaviorPreventGroundStorageStacking : CollectibleBehavior
    {
        public CollectibleBehaviorPreventGroundStorageStacking(CollectibleObject collObj) : base(collObj)
        {

        }
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
        {
            if (byEntity.Api.World.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityGroundStorage groundStorage)
            {
                if(groundStorage == null || groundStorage.Inventory.Empty || groundStorage.StorageProps.Layout != EnumGroundStorageLayout.Stacking)
                {
                    base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handHandling, ref handling);
                    return;
                }

                if(byEntity.Controls.Sneak && groundStorage.Inventory.FirstNonEmptySlot.StackSize == groundStorage.StorageProps.StackingCapacity)
                {
                    handling = EnumHandling.PreventSubsequent;
                    handHandling = EnumHandHandling.Handled;
                }
            }
        }
    }
}
