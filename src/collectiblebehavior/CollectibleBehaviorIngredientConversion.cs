using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace AncientTools.CollectibleBehaviors
{
    class CollectibleBehaviorConvertBlockUsingIngredient : CollectibleBehavior
    {
        private AssetLocation convertFromBlockCode;
        private AssetLocation convertToBlockCode;

        public CollectibleBehaviorConvertBlockUsingIngredient(CollectibleObject collObj) : base(collObj)
        {

        }

        public override void Initialize(JsonObject properties)
        {
            base.Initialize(properties);

            string[] fromCode = properties["convertFromBlockCode"].AsString().Split(':');
            string[] toCode = properties["convertToBlockCode"].AsString().Split(':');

            convertFromBlockCode = new AssetLocation(fromCode[0], fromCode[1]);
            convertToBlockCode = new AssetLocation(toCode[0], toCode[1]);
        }
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
        {
            if (blockSel == null)
                return;

            ICoreAPI api = byEntity.Api;

            if (byEntity is EntityPlayer player)
            {
                Block interactedBlock = api.World.BlockAccessor.GetBlock(blockSel.Position);

                if (interactedBlock is BlockGroundStorage)
                {
                    if (api.World.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityGroundStorage groundStorageEntity)
                    {
                        if (groundStorageEntity.TotalStackSize == 1 && groundStorageEntity.Inventory.FirstNonEmptySlot.Itemstack.Collectible.Code.Equals(convertFromBlockCode))
                        {
                            api.World.BlockAccessor.RemoveBlockEntity(blockSel.Position);
                            api.World.BlockAccessor.SetBlock(api.World.BlockAccessor.GetBlock(convertToBlockCode).Id, blockSel.Position);
                            api.World.BlockAccessor.MarkBlockDirty(blockSel.Position);

                            handling = EnumHandling.PreventDefault;
                            handHandling = EnumHandHandling.PreventDefault;
                        }
                    }
                    return;
                }
            }

            handling = EnumHandling.PassThrough;
        }
    }
}
