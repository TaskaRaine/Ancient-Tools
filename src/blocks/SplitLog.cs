using AncientTools.BlockEntities;
using AncientTools.Items;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace AncientTools.Blocks
{
    class BlockSplitLog: Block
    {
        WorldInteraction[] placeInteraction = null;
        WorldInteraction[] smackInteraction = null;
        WorldInteraction[] placeAndSmackInteraction = null;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            List<ItemStack> wedgeList = new List<ItemStack>();
            List<ItemStack> malletList = new List<ItemStack>();

            foreach (Item item in api.World.Items)
            {
                if (item is ItemWedge)
                    wedgeList.Add(new ItemStack(item));

                if (item is ItemMallet)
                    malletList.Add(new ItemStack(item));
            }
            placeInteraction = ObjectCacheUtil.GetOrCreate(api, "splitLogSmackInteraction", () =>
            {
                return new WorldInteraction[]
                {
                    new WorldInteraction()
                    {
                        ActionLangCode = "ancienttools:blockhelp-place-wedge",
                        MouseButton = EnumMouseButton.Right,
                        Itemstacks = wedgeList.ToArray()
                    },
                };
            });
            smackInteraction = ObjectCacheUtil.GetOrCreate(api, "splitLogPlaceInteraction", () =>
            {
                return new WorldInteraction[]
                {
                    new WorldInteraction()
                    {
                        ActionLangCode = "ancienttools:blockhelp-smack-wedge",
                        MouseButton = EnumMouseButton.Left,
                        Itemstacks = malletList.ToArray()
                    }
                };
            });
            placeAndSmackInteraction = ObjectCacheUtil.GetOrCreate(api, "splitLogInteraction", () =>
            {
                return new WorldInteraction[]
                {
                    placeInteraction[0],
                    smackInteraction[0]
                };
            });
        }
        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            if (world.BlockAccessor.GetBlockEntity(selection.Position) is BESplitLog splitLogEntity)
            {
                if (splitLogEntity.IsInventoryEmpty())
                    return placeInteraction;
                else if (splitLogEntity.IsInventoryFull() && splitLogEntity.HasUnsmackedWedges())
                    return smackInteraction;
                else if (!splitLogEntity.IsInventoryFull() && splitLogEntity.HasUnsmackedWedges())
                    return placeAndSmackInteraction;
                else if (!splitLogEntity.IsInventoryFull())
                    return placeInteraction;
            }

            return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer);
        }
        public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
        {
            //return base.GetPlacedBlockInfo(world, pos, forPlayer);
            return Lang.GetMatching("ancienttools:blockdesc-strippedlog-*") + "<br>" + Lang.Get("game:Requires tool tier {0} ({1}) to break", new object[] { 1, "Stone" });
        }
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (blockSel.SelectionBoxIndex != 0)
            {
                ItemSlot activeSlot = byPlayer.InventoryManager.ActiveHotbarSlot;

                if(activeSlot.Empty || activeSlot.Itemstack?.Collectible is ItemWedge)
                {
                    if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BESplitLog splitLogEntity)
                    {
                        return splitLogEntity.OnInteract(byPlayer, blockSel.SelectionBoxIndex - 1, blockSel.HitPosition);
                    }
                }
            }

            return false;
        }
        public override float OnGettingBroken(IPlayer player, BlockSelection blockSel, ItemSlot itemslot, float remainingResistance, float dt, int counter)
        {
            if (blockSel.SelectionBoxIndex == 0 || itemslot.Itemstack?.Collectible.GetType() != typeof(ItemMallet))
                return base.OnGettingBroken(player, blockSel, itemslot, remainingResistance, dt, counter);

            return remainingResistance;
        }
        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            if (world.BlockAccessor.GetBlockEntity(pos) is BESplitLog splitLogEntity)
                if (splitLogEntity.SkipDefaultMesh)
                {
                    base.OnBlockBroken(world, pos, byPlayer, 0);

                    return;
                }
            base.OnBlockBroken(world, pos, byPlayer);
        }
        public override void GetDecal(IWorldAccessor world, BlockPos pos, ITexPositionSource decalTexSource, ref MeshData decalModelData, ref MeshData blockModelData)
        {
            if (world.BlockAccessor.GetBlockEntity(pos) is BESplitLog splitLogEntity)
                if (splitLogEntity.SkipDefaultMesh)
                {
                    ICoreClientAPI capi = api as ICoreClientAPI;

                    Shape shape = capi.Assets.TryGet(splitLogEntity.SplitBlockShapeLocation + ".json")?.ToObject<Shape>();

                    blockModelData = splitLogEntity.NewSplitLogMesh;

                    MeshData md;
                    capi.Tesselator.TesselateShape("typedcontainer-decal", shape, out md, decalTexSource);
                    decalModelData = md;
                }
                else
                    base.GetDecal(world, pos, decalTexSource, ref decalModelData, ref blockModelData);
        }
    }
}
