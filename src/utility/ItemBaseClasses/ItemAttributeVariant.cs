using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace AncientTools.Utility
{
    class ItemAttributeVariant: Item, ITexPositionSource, IAttributeVariant
    {
        public TextureAtlasPosition this[string textureCode] 
        {
            get
            {
                string material = CurrentType.Split('-')[0];
                string type = CurrentType.Split('-')[1];

                if (Textures.TryGetValue(textureCode, out CompositeTexture compositeTex))
                {
                    if (Attributes["dynamicTextures"][textureCode].Exists)
                    {
                        compositeTex = new CompositeTexture(new AssetLocation(Attributes["dynamicTextures"][textureCode].AsString().Replace("{type}", type).Replace("{material}", material)));
                    }
                    else
                    {
                        if (!Textures.TryGetValue(CurrentType + "-" + textureCode, out compositeTex))
                            compositeTex = FirstTexture;
                    }
                }

                TextureAtlasPosition texpos = null;

                if(compositeTex != null)
                {
                    if (compositeTex.Base != null)
                        texpos = Capi.ItemTextureAtlas[compositeTex.Base];
                }
                if (texpos == null)
                {
                    IAsset texAsset = Capi.Assets.TryGet(compositeTex.Base.Clone().WithPathPrefixOnce("textures/").WithPathAppendixOnce(".png"));
                    if (texAsset != null)
                    {
                        Capi.ItemTextureAtlas.GetOrInsertTexture(compositeTex.Base, out _, out texpos);
                    }
                }

                return texpos;
            }
        }   

        public Size2i AtlasSize => Capi.ItemTextureAtlas.Size;
        public ICoreClientAPI Capi { get; set; }

        public Shape CurrentShape { get; set; }
        public string CurrentType { get; set; }

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            if (api.Side == EnumAppSide.Client)
                Capi = (ICoreClientAPI)api;
        }
        public override string GetHeldItemName(ItemStack itemStack)
        {
            string type = itemStack.Attributes.GetString("type", "unknown");

            return Lang.GetMatching(Code?.Domain + AssetLocation.LocationSeparator + "item-" + type + "-" + Code?.Path);
        }
        public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
        {
            CurrentType = itemstack.Attributes.GetString("type", "unknown");

            string cacheKey = "itemAttributeMeshRefs" + Code.Domain + FirstCodePart();
            Dictionary<string, MeshRef> meshrefs = ObjectCacheUtil.GetOrCreate(capi, cacheKey, () => new Dictionary<string, MeshRef>());

            if (!meshrefs.TryGetValue(CurrentType, out renderinfo.ModelRef))
            {
                MeshData mesh = GenMesh();
                meshrefs[CurrentType] = renderinfo.ModelRef = capi.Render.UploadMesh(mesh);
            }

            base.OnBeforeRender(capi, itemstack, target, ref renderinfo);
        }
        public override void OnUnloaded(ICoreAPI api)
        {
            base.OnUnloaded(api);

            if(api.Side == EnumAppSide.Client)
            {
                string cacheKey = "itemAttributeMeshRefs" + Code.Domain + FirstCodePart();
                Dictionary<string, MeshRef> meshrefs = ObjectCacheUtil.TryGet<Dictionary<string, MeshRef>>(api, cacheKey);

                if(meshrefs != null)
                {
                    foreach(KeyValuePair<string, MeshRef> reference in meshrefs)
                    {
                        reference.Value.Dispose();
                    }

                    api.ObjectCache.Remove(cacheKey);
                }
            }
        }
        public MeshData GenMesh()
        {
            MeshData mesh;

            if (Shape != null)
            {
                CurrentShape = Capi.TesselatorManager.GetCachedShape(Shape.Base);
            }

            Capi.Tesselator.TesselateItem(this, out mesh, this);

            return mesh;
        }
    }
}
