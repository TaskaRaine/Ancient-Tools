using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;

namespace AncientTools.CollectibleBehaviors
{
    class CollectibleBehaviorConvertHide: CollectibleBehavior
    {
        public CollectibleBehaviorConvertHide(CollectibleObject collObj) : base(collObj)
        {

        }
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
        {
            if (blockSel == null)
                return;

            if (byEntity is EntityPlayer)
            {
                Block interactedBlock = byEntity.Api.World.BlockAccessor.GetBlock(blockSel.Position.Copy().Offset(blockSel.Face));

                if (interactedBlock.BlockMaterial == EnumBlockMaterial.Liquid)
                {
                    if (interactedBlock.Code.Domain == "game" && interactedBlock.FirstCodePart() == "water")
                    {
                        ItemStack hideWaterSack = new ItemStack(byEntity.Api.World.GetBlock(new AssetLocation("ancienttools", "hidewatersack-raw-" + slot.Itemstack.Item.LastCodePart())));

                        slot.TakeOut(1);
                        slot.MarkDirty();

                        if (!byEntity.TryGiveItemStack(hideWaterSack))
                            byEntity.Api.World.SpawnItemEntity(hideWaterSack, byEntity.Pos.AsBlockPos.ToVec3d(), null);

                        handHandling = EnumHandHandling.PreventDefault;
                        return;
                    }
                }
            }

            handHandling = EnumHandHandling.NotHandled;
        }
    }
}
