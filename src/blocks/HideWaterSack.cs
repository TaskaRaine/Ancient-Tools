using AncientTools.BlockEntity;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace AncientTools.Blocks
{
    class BlockHideWaterSack : Block
    {
        WorldInteraction[] pickupInteraction = null;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            pickupInteraction = ObjectCacheUtil.GetOrCreate(api, "sackPickUp", () =>
            {
                return new WorldInteraction[] {
                    new WorldInteraction()
                    {
                        ActionLangCode = "ancienttools:blockhelp-take-hide-sack",
                        MouseButton = EnumMouseButton.Right
                    }
                };
            });
        }
        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            return pickupInteraction;
        }
        public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
        {
            if (world.BlockAccessor.GetBlockEntity(pos) is BEHideWaterSack waterSackEntity)
                if(this.FirstCodePart(1) == "raw")
                    return base.GetPlacedBlockInfo(world, pos, forPlayer) + "\n" + waterSackEntity.GetTimeRemainingInfo();

            return base.GetPlacedBlockInfo(world, pos, forPlayer);
        }
        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            if (this.FirstCodePart(1) == "raw")
            {
                if (inSlot.Itemstack.Attributes.HasAttribute("timeremaining"))
                    dsc.Append("\n" + Lang.Get("ancienttools:blockdesc-hidewatersack-soak-x-hours-when-placed", (int)(inSlot.Itemstack.Attributes.GetDouble("timeremaining") + 0.5)));
                else
                    dsc.Append("\n" + Lang.Get("ancienttools:blockdesc-hidewatersack-soak-x-hours-when-placed", this.Attributes["conversiontime"]));
                }
            }
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if(this.FirstCodePart(1) == "soaked")
            {
                ItemStack soakedHide = new ItemStack(world.GetItem(new AssetLocation("game", "hide-soaked-" + this.LastCodePart())));

                if (!byPlayer.Entity.TryGiveItemStack(soakedHide))
                {
                    world.SpawnItemEntity(soakedHide, blockSel.Position.ToVec3d());
                }

                ItemStack waterItemstack = new ItemStack(world.GetItem(new AssetLocation("game", "waterportion")));
                
                world.PlaySoundAt(new AssetLocation("game", "sounds/effect/water-fill1"), byPlayer, byPlayer, true, 12);
                api.World.SpawnCubeParticles(byPlayer.Entity.Pos.AheadCopy(0.25).XYZ.Add(0, byPlayer.Entity.CollisionBox.Y2 / 2, 0), waterItemstack, 0.75f, 10 * 2, 0.45f);

                world.BlockAccessor.SetBlock(0, blockSel.Position);
                world.BlockAccessor.RemoveBlockEntity(blockSel.Position);

                return true;
            }
            else if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEHideWaterSack waterSackEntity)
            {
                ItemStack rawHide = new ItemStack(world.GetBlock(new AssetLocation("ancienttools", "hidewatersack-raw-" + this.LastCodePart())));
                rawHide.Attributes.SetDouble("timeremaining", waterSackEntity.GetTimeRemaining());

                if (!byPlayer.Entity.TryGiveItemStack(rawHide))
                {
                    world.SpawnItemEntity(rawHide, blockSel.Position.ToVec3d());
                }

                world.BlockAccessor.SetBlock(0, blockSel.Position);
                world.BlockAccessor.RemoveBlockEntity(blockSel.Position);

                return true;
            }

            return false;
        }
    }
}
