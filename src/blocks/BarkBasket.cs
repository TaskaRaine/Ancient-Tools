using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace AncientTools.Blocks
{
    class BlockBarkBasket: BlockGenericTypedContainer
    {
        //-- Copied from Block. The BlockContainer version was causing 'unknown texture' particles --//
        public override int GetRandomColor(ICoreClientAPI capi, BlockPos pos, BlockFacing facing, int rndIndex)
        {
            if (Textures == null || Textures.Count == 0) return 0;
            CompositeTexture tex;
            if (!Textures.TryGetValue(facing.Code, out tex))
            {
                tex = Textures.First().Value;
            }
            if (tex?.Baked == null) return 0;

            int color = capi.BlockTextureAtlas.GetRandomColor(tex.Baked.TextureSubId);

            if (ClimateColorMapResolved != null || SeasonColorMapResolved != null)
            {
                color = capi.World.ApplyColorMapOnRgba(ClimateColorMapResolved, SeasonColorMapResolved, color, pos.X, pos.Y, pos.Z);
            }

            return color;
        }
        //-- Copied from Block. The BlockContainer version was causing a crash. Probably from AssetLocation domain being 'game'. --//
        public override void GetDecal(IWorldAccessor world, BlockPos pos, ITexPositionSource decalTexSource, ref MeshData decalModelData, ref MeshData blockModelData)
        {
            BlockEntityGenericTypedContainer be = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityGenericTypedContainer;
            if (be != null)
            {
                ICoreClientAPI capi = api as ICoreClientAPI;
                string shapename = this.Attributes["shape"][be.type].AsString();
                if (shapename == null)
                {
                    base.GetDecal(world, pos, decalTexSource, ref decalModelData, ref blockModelData);
                    return;
                }

                blockModelData = GenMesh(capi, be.type, shapename);

                AssetLocation shapeloc = new AssetLocation("ancienttools", shapename).WithPathPrefix("shapes/");
                Shape shape = capi.Assets.TryGet(shapeloc + ".json")?.ToObject<Shape>();
                if (shape == null)
                {
                    shape = capi.Assets.TryGet(shapeloc + "1.json").ToObject<Shape>();
                }

                MeshData md;
                capi.Tesselator.TesselateShape("typedcontainer-decal", shape, out md, decalTexSource);
                decalModelData = md;

                decalModelData.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, be.MeshAngle, 0);

                return;
            }
        }
    }
}
