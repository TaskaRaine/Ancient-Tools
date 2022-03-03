using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace AncientTools.Blocks
{
    class BlockPitchTorch: BlockWaterproofTorch
    {
        public WorldInteraction[] placePitchInteractions = null;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            ItemStack[] pitch =
            {
                new ItemStack(api.World.GetItem(new AssetLocation("ancienttools", "pitch-stick")))
            };

            WorldInteraction pitchInteraction = new WorldInteraction()
            {
                ActionLangCode = "ancienttools:blockhelp-place-pitch",
                MouseButton = EnumMouseButton.Right,
                Itemstacks = pitch
            };

            placePitchInteractions = ObjectCacheUtil.GetOrCreate(api, "placePitchInteractions", () =>
            {
                return new WorldInteraction[]
                {
                    pitchInteraction
                };
            });
        }
        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            if(CodeWithVariant("state", "melted").Equals(Code))
            {
                return placePitchInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
            }
            return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer);
        }
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (byPlayer.InventoryManager.ActiveHotbarSlot == null || byPlayer.InventoryManager.ActiveHotbarSlot.Empty)
                return false;

            AssetLocation interactedItemCode = byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Collectible.Code;

            if (interactedItemCode.Equals(new AssetLocation("ancienttools", "pitch-stick")))
            {
                if (CodeWithVariant("state", "melted").Equals(Code))
                {
                    world.BlockAccessor.SetBlock(world.GetBlock(CodeWithVariant("state", "extinct")).Id, blockSel.Position);
                    world.BlockAccessor.MarkBlockDirty(blockSel.Position);
                    world.BlockAccessor.MarkBlockEntityDirty(blockSel.Position);

                    byPlayer.InventoryManager.ActiveHotbarSlot.TakeOut(1);

                    if (!byPlayer.InventoryManager.TryGiveItemstack(new ItemStack(api.World.GetItem(new AssetLocation("game", "stick")))))
                        api.World.SpawnItemEntity(new ItemStack(api.World.GetItem(new AssetLocation("game", "stick"))), byPlayer.Entity.Pos.AsBlockPos.ToVec3d());

                    return true;
                }
            }

            return false;
        }
    }
}
