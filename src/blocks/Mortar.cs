using AncientTools.BlockEntities;
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

                foreach (CollectibleObject item in api.World.Collectibles)
                {
                    if (item.Code == null) continue;

                    if (item.FirstCodePart() == "pestle" && item.Code.Domain == "ancienttools")
                    {
                        pestleList.Add(new ItemStack(item));
                    }
                    else if (item.Attributes != null && item.Attributes["mortarProperties"].Exists)
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
                            HotKeyCode = "shift",
                            MouseButton = EnumMouseButton.Right
                        }
                    };
            });
        }
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (blockSel == null)
                return false;

            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEMortar mortarEntity)
            {
                mortarEntity.OnInteract(byPlayer);

                return true;
            }

            return false;
        }
        public override bool OnBlockInteractStep(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (blockSel == null)
                return false;

            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEMortar mortarEntity)
            {
                return mortarEntity.OnSneakInteract(byPlayer);
            }
            return false;
        }
        public override void OnBlockInteractStop(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (blockSel == null)
                return;

            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEMortar mortarEntity)
            {
                mortarEntity.OnInteractStop();
            }
        }
        public override bool OnBlockInteractCancel(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, EnumItemUseCancelReason cancelReason)
        {
            if (blockSel == null)
                return true;

            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEMortar mortarEntity)
            {
                mortarEntity.OnInteractStop();
            }

            return true;
        }
        public MeshData GenMesh(ICoreClientAPI capi, string shapePath, ITexPositionSource texture)
        {
            Shape shape = capi.Assets.TryGet(shapePath + ".json").ToObject<Shape>();

            MeshData wholeMesh;

            capi.Tesselator.TesselateShape("mortar", shape, out wholeMesh, capi.Tesselator.GetTextureSource(this));

            return wholeMesh;
        }
        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            return interactions;
        }
    }
}
