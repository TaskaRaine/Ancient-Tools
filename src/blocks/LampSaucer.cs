using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace AncientTools.Blocks
{
    class BlockLampSaucer: BlockGroundAndSideAttachable, IIgnitable
    {
        WorldInteraction[] saucerInteractions = null;
        WorldInteraction[] extinctInteractions = null;

        Block ExtinctVariant;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            ItemStack[] lightTypes = new ItemStack[]
            {
                new ItemStack(api.World.GetItem(new AssetLocation("ancienttools", "pitch-stick"))),
                new ItemStack(api.World.GetItem(new AssetLocation("game", "candle")))
            };

            WorldInteraction emptyInteraction = new WorldInteraction()
            {
                ActionLangCode = "ancienttools:blockhelp-saucer-insert-object",
                MouseButton = EnumMouseButton.Right,
                Itemstacks = lightTypes
            };

            ItemStack[] canIgnite = BlockBehaviorCanIgnite.CanIgniteStacks(api, true).ToArray();

            WorldInteraction extinctInteraction = new WorldInteraction()
            {
                ActionLangCode = "game:blockhelp-firepit-ignite",
                MouseButton = EnumMouseButton.Right,
                HotKeyCode = "shift",
                Itemstacks = canIgnite,
                GetMatchingStacks = (wi, bs, es) =>
                {
                    return wi.Itemstacks;
                }
            };

            saucerInteractions = ObjectCacheUtil.GetOrCreate(api, "saucerInteractions", () =>
            {
                return new WorldInteraction[]
                {
                    emptyInteraction
                };
            });

            extinctInteractions = ObjectCacheUtil.GetOrCreate(api, "extinctInteractions", () =>
            {
                return new WorldInteraction[]
                {
                    extinctInteraction
                };
            });

            if (Variant.ContainsKey("state"))
            {
                AssetLocation loc = CodeWithVariant("state", "extinct");
                ExtinctVariant = api.World.GetBlock(loc);
            }
        }
        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            if (CodeWithVariant("type", "empty").Equals(Code))
                return saucerInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
            else if (CodeWithVariant("state", "extinct").Equals(Code))
                return extinctInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));

            return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer);
        }
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if(CodeWithVariant("type", "empty").Equals(Code))
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
                else if(interactedItemCode.Equals(new AssetLocation("ancienttools", "pitch-stick")))
                {
                    if (FirstCodePart(2) == "empty")
                    {
                        world.BlockAccessor.SetBlock(world.GetBlock(CodeWithVariant("type", "pitch")).Id, blockSel.Position);
                        world.BlockAccessor.MarkBlockDirty(blockSel.Position);
                        world.BlockAccessor.MarkBlockEntityDirty(blockSel.Position);

                        byPlayer.InventoryManager.ActiveHotbarSlot.TakeOut(1);

                        if (!byPlayer.InventoryManager.TryGiveItemstack(new ItemStack(api.World.GetItem(new AssetLocation("game", "stick")))))
                            api.World.SpawnItemEntity(new ItemStack(api.World.GetItem(new AssetLocation("game", "stick"))), byPlayer.Entity.Pos.AsBlockPos.ToVec3d());

                        return true;
                    }
                }
            }

            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }
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
        public EnumIgniteState OnTryIgniteBlock(EntityAgent byEntity, BlockPos pos, float secondsIgniting)
        {
            return EnumIgniteState.NotIgnitable;
        }

        public void OnTryIgniteBlockOver(EntityAgent byEntity, BlockPos pos, float secondsIgniting, ref EnumHandling handling)
        {
            if (CodeWithVariant("state", "extinct").Equals(Code))
            {
                handling = EnumHandling.PreventDefault;
                var block = api.World.GetBlock(CodeWithVariant("state", "lit"));
                if (block != null)
                {
                    api.World.BlockAccessor.SetBlock(block.Id, pos);
                }

                byEntity.World.BlockAccessor.MarkBlockDirty(pos);
                byEntity.World.BlockAccessor.MarkBlockEntityDirty(pos);

                return;
            }

            handling = EnumHandling.PassThrough;
            return;
        }
    }
}
