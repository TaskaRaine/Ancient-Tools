using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace AncientTools.Blocks
{
    class BlockWaterproofTorch: BlockTorch
    {
        public override void OnGroundIdle(EntityItem entityItem)
        {
            if (CodeWithVariant("state", "lit").Equals(Code) && entityItem.Swimming)
            {
                api.World.PlaySoundAt(new AssetLocation("game:sounds/effect/extinguish"), entityItem.Pos.X + 0.5, entityItem.Pos.Y + 0.75, entityItem.Pos.Z + 0.5, null, false, 16);

                int torchCount = entityItem.Itemstack.StackSize;
                entityItem.Itemstack = new ItemStack(ExtinctVariant);
                entityItem.Itemstack.StackSize = torchCount;
            }
        }
        public override void OnHeldIdle(ItemSlot slot, EntityAgent byEntity)
        {
            if (api.World.Side == EnumAppSide.Server && CodeWithVariant("state", "lit").Equals(Code) && byEntity.Swimming && ExtinctVariant != null)
            {
                api.World.PlaySoundAt(new AssetLocation("game:sounds/effect/extinguish"), byEntity.Pos.X + 0.5, byEntity.Pos.Y + 0.75, byEntity.Pos.Z + 0.5, null, false, 16);

                int torchCount = slot.Itemstack.StackSize;
                slot.Itemstack = new ItemStack(ExtinctVariant);
                slot.Itemstack.StackSize = torchCount;
                slot.MarkDirty();
            }
        }
        public new void OnTryIgniteBlockOver(EntityAgent byEntity, BlockPos pos, float secondsIgniting, ref EnumHandling handling)
        {
            if (CodeWithVariant("state", "extinct").Equals(Code))
            {
                base.OnTryIgniteBlockOver(byEntity, pos, secondsIgniting, ref handling);

                byEntity.World.BlockAccessor.MarkBlockDirty(pos);
                byEntity.World.BlockAccessor.MarkBlockEntityDirty(pos);

                return;
            }

            handling = EnumHandling.PassThrough;
        }
    }
}
