using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace AncientTools.Items
{
    class ItemCart : Item
    {
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            if (blockSel == null) return;

            BlockPos spawnPos = GetOffsetSpawnPos(blockSel);

            if (spawnPos == null) return;

            EntityProperties entityType = api.World.GetEntityType(new AssetLocation("ancienttools", "cart"));
            Entity entity = api.World.ClassRegistry.CreateEntity(entityType);

            entity.ServerPos.SetPos(spawnPos);

            entity.Pos.SetFrom(entity.ServerPos);

            api.World.SpawnEntity(entity);

            handling = EnumHandHandling.PreventDefault;
        }
        private BlockPos GetOffsetSpawnPos(BlockSelection interactedBlock)
        {
            if(interactedBlock.Face == BlockFacing.UP)
            {
                return new BlockPos(interactedBlock.Position.X, interactedBlock.Position.UpCopy(1).Y, interactedBlock.Position.Z);
            }

            return null;
        }
    }
}
