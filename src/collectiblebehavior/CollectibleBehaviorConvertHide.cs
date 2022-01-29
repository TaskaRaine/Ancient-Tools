using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
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
            if (!(byEntity is EntityPlayer) || blockSel == null)
                return;

            EntityPlayer player = (EntityPlayer)byEntity;

            Block interactedBlock = byEntity.Api.World.BlockAccessor.GetBlock(blockSel.Position);

            if (interactedBlock.BlockMaterial == EnumBlockMaterial.Liquid)
            {
                if (interactedBlock.Code.Domain == "game" && interactedBlock.FirstCodePart() == "water")
                {
                    ItemStack hideWaterSack = new ItemStack(byEntity.Api.World.GetBlock(new AssetLocation("ancienttools", "hidewatersack-raw-" + slot.Itemstack.Item.LastCodePart())));

                    slot.TakeOut(1);
                    slot.MarkDirty();

                    if (!byEntity.TryGiveItemStack(hideWaterSack))
                        byEntity.Api.World.SpawnItemEntity(hideWaterSack, byEntity.Pos.AsBlockPos.ToVec3d(), null);

                    byEntity.World.PlaySoundAt(new AssetLocation("game", "sounds/effect/water-fill2"), byEntity as Entity, player.Player, true, 12.0f, 0.75f);

                    handHandling = EnumHandHandling.PreventDefault;
                    return;
                }
            }

            handHandling = EnumHandHandling.NotHandled;
        }
    }
}
