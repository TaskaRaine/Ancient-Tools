using AncientTools.Utility;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace AncientTools.EntityRenderers
{
    class MobileStorageRenderer : EntityRenderer, ITexPositionSource
    {
        protected struct PlacementProperties
        {
            public double[] Translation;
            public double[] Rotation;
            public double Scale;
        }
        public TextureAtlasPosition this[string textureCode]
        {
            get
            {
                CompositeTexture compositeTex;
                AssetLocation texturePath = null;

                if (!entity.Properties.Client.Textures.TryGetValue(CurrentType + "-" + textureCode, out compositeTex))
                {
                    entity.Properties.Client.Textures.TryGetValue(textureCode, out compositeTex);
                }

                if(compositeTex != null)
                {
                    texturePath = compositeTex.Base;
                }
                else
                {
                    bool texFound = false;

                    for(int i = 0; i < InventoryShapes.Length; i++)
                    {
                        if (InventoryShapes[i].Textures.ContainsKey(textureCode))
                        {
                            texturePath = InventoryShapes[i].Textures[textureCode];

                            texFound = true;
                        }
                    }
                    
                    if(texFound == false)
                        texturePath = MobileStorageEntity.Properties.Client.FirstTexture.Base;
                }
                TextureAtlasPosition texpos = null;

                if(texturePath != null)
                    texpos = capi.EntityTextureAtlas[texturePath];

                if (texpos == null)
                {
                    IAsset texAsset = Capi.Assets.TryGet(texturePath.Clone().WithPathPrefixOnce("textures/").WithPathAppendixOnce(".png"));
                    if (texAsset != null)
                    {
                        Capi.EntityTextureAtlas.GetOrInsertTexture(texturePath, out _, out texpos);
                    }
                }

                return texpos;
            }
        }
        public ICoreClientAPI Capi { get; set; }
        public string CurrentType { get; set; }
        public Shape CurrentShape { get; set; }
        public Shape[] InventoryShapes { get; set; }
        public EntityMobileStorage MobileStorageEntity { get; set; }
        public Size2i AtlasSize => Capi.EntityTextureAtlas.Size;


        protected PlacementProperties StoragePlacementProperties;

        public MobileStorageRenderer(EntityMobileStorage entity, ICoreClientAPI api) : base(entity, api)
        {
            Capi = api;

            MobileStorageEntity = entity;

            CurrentType = entity.WatchedAttributes.GetString("type");
            InventoryShapes = new Shape[entity.StorageBlocksCount];
        }
        public void AssignStoragePlacementProperties(JsonObject props)
        {
            if (props == null)
            {
                StoragePlacementProperties.Translation = new double[] { 0.0, 0.0, 0.0 };
                StoragePlacementProperties.Rotation = new double[] { 0.0, 0.0, 0.0 };
                StoragePlacementProperties.Scale = 1.0;

                return;
            }

            if (props.KeyExists("translation"))
                StoragePlacementProperties.Translation = props["translation"].AsArray<double>();
            else
                StoragePlacementProperties.Translation = new double[] { 0.0, 0.0, 0.0 };

            if (props.KeyExists("rotation"))
                StoragePlacementProperties.Rotation = props["rotation"].AsArray<double>();
            else
                StoragePlacementProperties.Rotation = new double[] { 0.0, 0.0, 0.0 };

            if (props.KeyExists("scale"))
                StoragePlacementProperties.Scale = props["scale"].AsDouble();
            else
                StoragePlacementProperties.Scale = 1.0;
        }
        public override void Dispose()
        {

        }
    }
}
