using System;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace AncientTools.CollectibleBehaviors
{
    class CollectibleBehaviorConvertBlockUsingIngredient : CollectibleBehavior
    {
        private AssetLocation convertFromBlockCode;
        private AssetLocation convertToBlockCode;
        private AssetLocation resolvedFromBlockCode;
        private AssetLocation resolvedToBlockCode;

        string wildcard;
        int quantityNeeded;

        public CollectibleBehaviorConvertBlockUsingIngredient(CollectibleObject collObj) : base(collObj)
        {

        }

        public override void Initialize(JsonObject properties)
        {
            if (properties == null || !properties["convertFromBlockCode"].Exists || !properties["convertToBlockCode"].Exists)
                return;

            base.Initialize(properties);

            string[] fromCode = properties["convertFromBlockCode"].AsString().Split(':');
            string[] toCode = properties["convertToBlockCode"].AsString().Split(':');

            wildcard = properties["wildcard"].AsString();
            quantityNeeded = properties["quantityNeeded"].AsInt();

            convertFromBlockCode = new AssetLocation(fromCode[0], fromCode[1]);
            convertToBlockCode = new AssetLocation(toCode[0], toCode[1]);
            resolvedFromBlockCode = new AssetLocation(fromCode[0], string.Empty);
            resolvedToBlockCode = new AssetLocation(fromCode[0], string.Empty);
        }
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
        {
            if (blockSel == null)
                return;

            if (convertFromBlockCode == null || convertToBlockCode == null || !convertFromBlockCode.Valid || !convertToBlockCode.Valid)
                return;

            ICoreAPI api = byEntity.Api;

            if (byEntity is EntityPlayer player)
            {
                Block interactedBlock = api.World.BlockAccessor.GetBlock(blockSel.Position, BlockLayersAccess.SolidBlocks);

                if(wildcard is not null && wildcard.Length is not 0)
                {
                    if(interactedBlock.Variant.TryGetValue(wildcard) is string variant)
                    {
                        ResolveWildcard(variant, convertToBlockCode, convertFromBlockCode, ref resolvedToBlockCode, ref resolvedFromBlockCode);
                    }
                }

                if (slot.StackSize >= quantityNeeded)
                {
                    if (interactedBlock is BlockGroundStorage)
                    {
                        if (api.World.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityGroundStorage groundStorageEntity)
                        {
                            if (groundStorageEntity.TotalStackSize == 1 && groundStorageEntity.Inventory.FirstNonEmptySlot.Itemstack.Collectible.Code.Equals(resolvedFromBlockCode))
                            {
                                api.World.BlockAccessor.RemoveBlockEntity(blockSel.Position);
                                api.World.BlockAccessor.SetBlock(api.World.BlockAccessor.GetBlock(resolvedToBlockCode).Id, blockSel.Position);
                                api.World.BlockAccessor.MarkBlockDirty(blockSel.Position);

                                ConsumeResource(quantityNeeded, slot);

                                handling = EnumHandling.PreventDefault;
                                handHandling = EnumHandHandling.PreventDefault;

                                return;
                            }
                        }
                    }
                    else if (interactedBlock.Code.Equals(resolvedFromBlockCode))
                    {
                        api.World.BlockAccessor.SetBlock(api.World.BlockAccessor.GetBlock(resolvedToBlockCode).Id, blockSel.Position);
                        api.World.BlockAccessor.MarkBlockDirty(blockSel.Position);

                        ConsumeResource(quantityNeeded, slot);

                        handling = EnumHandling.PreventDefault;
                        handHandling = EnumHandHandling.PreventDefault;

                        return;
                    }
                }
            }

            handling = EnumHandling.PassThrough;
        }
        private void ResolveWildcard(string variant, AssetLocation toBlockCode, AssetLocation fromBlockCode, ref AssetLocation resolvedToBlockCode, ref AssetLocation resolvedFromBlockCode)
        {
            resolvedToBlockCode.Path = ReplaceWildcard(toBlockCode.Path, variant);
            resolvedFromBlockCode.Path = ReplaceWildcard(fromBlockCode.Path, variant);
        }
        private string ReplaceWildcard(string path, string variant)
        {
            int indexWildcardStart = path.IndexOf('{');
            int indexWildcardEnd = path.IndexOf('}');

            if (indexWildcardStart < 0 || indexWildcardEnd < 0)
                return path;

            string textBefore = path.Substring(0, indexWildcardStart);
            string textAfter = path.Substring(indexWildcardEnd + 1);

            return textBefore + variant + textAfter;
        }
        private void ConsumeResource(int consumptionAmount, ItemSlot slot)
        {
            if (consumptionAmount > 0)
            {
                slot.TakeOut(consumptionAmount);
                slot.MarkDirty();
            }
        }
    }
}
