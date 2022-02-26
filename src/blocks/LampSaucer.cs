using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace AncientTools.Blocks
{
    class BlockLampSaucer: BlockGroundAndSideAttachable
    {
        WorldInteraction[] saucerInteractions = null;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            ItemStack[] lightTypes = new ItemStack[]
            {
                new ItemStack(api.World.GetItem(new AssetLocation("ancienttools", "pitch-ball"))),
                new ItemStack(api.World.GetItem(new AssetLocation("ancienttools", "pitch-stick"))),
                new ItemStack(api.World.GetItem(new AssetLocation("game", "candle")))
            };

            WorldInteraction emptyInteraction = new WorldInteraction()
            {
                ActionLangCode = "ancienttools:blockhelp-saucer-insert-object",
                MouseButton = EnumMouseButton.Right,
                Itemstacks = lightTypes
            };

            saucerInteractions = ObjectCacheUtil.GetOrCreate(api, "saucerInteractions", () =>
            {
                return new WorldInteraction[]
                {
                    emptyInteraction
                };
            });
        }
        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            if (CodeWithVariant("type", "empty").Equals(Code))
                return saucerInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));

            return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer);
        }
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (byPlayer.InventoryManager.ActiveHotbarSlot == null || byPlayer.InventoryManager.ActiveHotbarSlot.Empty)
                return base.OnBlockInteractStart(world, byPlayer, blockSel);

            AssetLocation interactedItemCode = byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Collectible.Code;

            if(interactedItemCode.Equals(new AssetLocation("game", "candle")))
            {
                if(FirstCodePart(2) == "empty")
                {
                    world.BlockAccessor.SetBlock(world.GetBlock(CodeWithVariant("type", "candle")).Id, blockSel.Position);
                    world.BlockAccessor.MarkBlockDirty(blockSel.Position);
                    world.BlockAccessor.MarkBlockEntityDirty(blockSel.Position);

                    byPlayer.InventoryManager.ActiveHotbarSlot.TakeOut(1);

                    return true;
                }
            }
            else if(interactedItemCode.BeginsWith("ancienttools", "pitch-"))
            {
                if (FirstCodePart(2) == "empty")
                {
                    world.BlockAccessor.SetBlock(world.GetBlock(CodeWithVariant("type", "pitch")).Id, blockSel.Position);
                    world.BlockAccessor.MarkBlockDirty(blockSel.Position);
                    world.BlockAccessor.MarkBlockEntityDirty(blockSel.Position);

                    byPlayer.InventoryManager.ActiveHotbarSlot.TakeOut(1);

                    return true;
                }
            }

            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }
    }
}
