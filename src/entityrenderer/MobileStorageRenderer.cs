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

            public bool IsSet()
            {
                if (Translation == null || Rotation == null || Scale == 0)
                    return false;

                return true;
            }
        }
        public TextureAtlasPosition this[string textureCode]
        {
            get
            {
                string material = CurrentType.Split('-')[0];
                string type = CurrentType.Split('-')[1];

                if (entity.Properties.Client.Textures.TryGetValue(textureCode, out CompositeTexture compositeTex))
                {
                    if (compositeTex.Base.Path.Contains("{type}") || compositeTex.Base.Path.Contains("{material}"))
                    {
                        compositeTex = new CompositeTexture(new AssetLocation(compositeTex.Base.Domain, compositeTex.Base.Path.Replace("{type}", type).Replace("{material}", material)));
                    }
                }
                else
                {
                    for(int i = 0; i < InventoryShapes.Length; i++)
                    {
                        if (InventoryShapes[i].Textures.ContainsKey(textureCode))
                        {
                            compositeTex = new CompositeTexture(InventoryShapes[i].Textures[textureCode]);
                        }
                        else if(this.MobileStorageEntity.MobileStorageInventory[0].Itemstack.Block.Textures.ContainsKey(textureCode))
                        {
                            compositeTex = new CompositeTexture(this.MobileStorageEntity.MobileStorageInventory[0].Itemstack.Block.Textures[textureCode].Base);
                        }
                        else if (this.MobileStorageEntity.MobileStorageInventory[0].Itemstack.Block.Textures.ContainsKey(this.MobileStorageEntity.MobileStorageInventory[0].Itemstack.Attributes?.GetString("type") + "-" + textureCode))
                        {
                            compositeTex = new CompositeTexture(this.MobileStorageEntity.MobileStorageInventory[0].Itemstack.Block.Textures[this.MobileStorageEntity.MobileStorageInventory[0].Itemstack.Attributes?.GetString("type") + "-" + textureCode].Base);
                        }
                    }

                    if(compositeTex == null)
                        if (!entity.Properties.Client.Textures.TryGetValue(CurrentType + "-" + textureCode, out compositeTex))
                            compositeTex = entity.Properties.Client.FirstTexture;
                }

                TextureAtlasPosition texpos = null;

                if (compositeTex.Base != null)
                    texpos = Capi.EntityTextureAtlas[compositeTex.Base];

                if (texpos == null)
                {
                    IAsset texAsset = Capi.Assets.TryGet(compositeTex.Base.Clone().WithPathPrefixOnce("textures/").WithPathAppendixOnce(".png"));
                    if (texAsset != null)
                    {
                        Capi.EntityTextureAtlas.GetOrInsertTexture(compositeTex.Base, out _, out texpos);
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
