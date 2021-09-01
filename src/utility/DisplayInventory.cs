using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace AncientTools.Utility
{
    abstract class DisplayInventory: BlockEntityContainer, ITexPositionSource
    {
        public Size2i AtlasSize => capi.BlockTextureAtlas.Size;

        public override InventoryBase Inventory => inventory;
        public override string InventoryClassName => "displayinventory";

        public TextureAtlasPosition this[string textureCode]
        {
            get
            {
                AssetLocation texturePath;

                if (currentObject.ItemClass == EnumItemClass.Item)
                {
                    Item currentItem = currentObject as Item;
                    texturePath = currentItem.FirstTexture.Baked.BakedName;
                }
                else
                {
                    Block currentBlock = currentObject as Block;
                    texturePath = currentBlock.Textures["all"].Base;
                }

                TextureAtlasPosition texpos = capi.BlockTextureAtlas[texturePath];

                if (texpos == null)
                {
                    IAsset texAsset = capi.Assets.TryGet(texturePath.Clone().WithPathPrefixOnce("textures/").WithPathAppendixOnce(".png"));
                    if (texAsset != null)
                    {
                        BitmapRef bmp = texAsset.ToBitmap(capi);
                        capi.BlockTextureAtlas.InsertTextureCached(texturePath, bmp, out _, out texpos);
                    }
                }

                return texpos;
            }
        }

        protected ICoreClientAPI capi;
        protected InventoryGeneric inventory;
        protected CollectibleObject currentObject;
        protected MeshData[] meshes;

        protected int InventorySize = 1;

        public void InitializeInventory()
        {
            inventory = new InventoryGeneric(InventorySize, null, null);
        }
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            capi = api as ICoreClientAPI;
            meshes = new MeshData[InventorySize];
        }
        public abstract void UpdateMeshes();
        protected MeshData GenMesh(ICoreClientAPI capi, string shapePath)
        {
            Shape shape = capi.Assets.TryGet(shapePath + ".json").ToObject<Shape>();
            MeshData wholeMesh;

            capi.Tesselator.TesselateShape("salvecontainer", shape, out wholeMesh, this);
            return wholeMesh;
        }
        protected void AddMesh(ITerrainMeshPool mesher, MeshData mesh)
        {
            mesher.AddMeshData(mesh);
        }
    }
}
