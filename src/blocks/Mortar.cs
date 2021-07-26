using AncientTools.BlockEntity;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;

namespace AncientTools.Blocks
{
    class BlockMortar: Block
    {
        WorldInteraction[] interactions = null;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            interactions = ObjectCacheUtil.GetOrCreate(api, "mortarInteractions", () =>
            {
                List<ItemStack> pestleList = new List<ItemStack>();
                List<ItemStack> grindablesList = new List<ItemStack>();

                foreach (Item item in api.World.Items)
                {
                    if (item.Code == null) continue;

                    if (item.FirstCodePart() == "pestle" && item.Code.Domain == "ancienttools")
                    {
                        pestleList.Add(new ItemStack(item));
                    }
                    else if (item.GrindingProps != null)
                    {
                        grindablesList.Add(new ItemStack(item));
                    }
                }
                return new WorldInteraction[] {
                        new WorldInteraction()
                        {
                            ActionLangCode = "ancienttools:blockhelp-place-pestle",
                            MouseButton = EnumMouseButton.Right,
                            Itemstacks = pestleList.ToArray()
                        },
                        new WorldInteraction()
                        {
                            ActionLangCode = "ancienttools:blockhelp-place-resource",
                            MouseButton = EnumMouseButton.Right,
                            Itemstacks = grindablesList.ToArray()
                        },
                        new WorldInteraction()
                        {
                            ActionLangCode = "ancienttools:blockhelp-grind-mortar",
                            RequireFreeHand = true,
                            HotKeyCode = "sneak",
                            MouseButton = EnumMouseButton.Right
                        }
                    };
            });
        }
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEMortar mortarEntity)
            {
                mortarEntity.OnInteract(byPlayer);

                return true;
            }

            return false;
        }
        public override bool OnBlockInteractStep(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEMortar mortarEntity)
            {
                return mortarEntity.OnSneakInteract(byPlayer); ;
            }
            return false;
        }
        public override void OnBlockInteractStop(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEMortar mortarEntity)
            {
                mortarEntity.OnInteractStop();
            }
        }
        public override bool OnBlockInteractCancel(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, EnumItemUseCancelReason cancelReason)
        {
            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEMortar mortarEntity)
            {
                mortarEntity.OnInteractStop();

                return true;
            }

            return false;
        }
        public MeshData GenMesh(ICoreClientAPI capi, string shapePath, ITexPositionSource texture)
        {
            Shape shape = capi.Assets.TryGet(shapePath + ".json").ToObject<Shape>();

            MeshData wholeMesh;

            capi.Tesselator.TesselateShape("mortar", shape, out wholeMesh, capi.Tesselator.GetTexSource(this));

            return wholeMesh;
        }
        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            return interactions;
        }
    }
}
