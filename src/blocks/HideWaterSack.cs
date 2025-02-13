using AncientTools.BlockEntities;
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

        private float _conversionTime;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            _conversionTime = api.World.Config.GetFloat("ConversionTime");

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
            return pickupInteraction.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
        }
        public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
        {
            if (world.BlockAccessor.GetBlockEntity(pos) is BEHideWaterSack waterSackEntity)
                if(this.FirstCodePart(1) == "raw" || this.FirstCodePart(1) == "salted")
                    return base.GetPlacedBlockInfo(world, pos, forPlayer) + "\n" + waterSackEntity.GetTimeRemainingInfo();

            return base.GetPlacedBlockInfo(world, pos, forPlayer);
        }
        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            if (this.FirstCodePart(1) == "raw"|| this.FirstCodePart(1) == "salted")
            {
                if (inSlot.Itemstack.Attributes.HasAttribute("timeremaining"))
                    dsc.Append("\n" + Lang.Get("ancienttools:blockdesc-hidewatersack-soak-x-hours-when-placed", (int)(inSlot.Itemstack.Attributes.GetDouble("timeremaining") + 0.5)));
                else
                    dsc.Append("\n" + Lang.Get("ancienttools:blockdesc-hidewatersack-soak-x-hours-when-placed", api.World.Config.GetFloat("WaterSackConversionHours", 48.0f)));
                }
            }
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (byPlayer.Entity.Controls.Sneak)
                return false;

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

                return true;
            }
            else if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEHideWaterSack waterSackEntity)
            {
                ItemStack rawHide = new ItemStack(world.GetBlock(new AssetLocation("ancienttools", "hidewatersack-" + this.FirstCodePart(1) + "-" + this.LastCodePart())));
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
        public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ItemStack byItemStack)
        {
            base.OnBlockPlaced(world, blockPos, byItemStack);

            if (byItemStack == null)
                return;

            if (world.BlockAccessor.GetBlockEntity(blockPos) is BEHideWaterSack waterSackEntity)
            {
                waterSackEntity.SetTimeRemaining(byItemStack.Attributes.GetDouble("timeremaining", api.World.Config.GetDouble("WaterSackConversionHours", 48.0)));
            }
        }
    }
}
