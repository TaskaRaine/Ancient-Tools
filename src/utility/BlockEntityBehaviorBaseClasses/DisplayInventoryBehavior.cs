using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace AncientTools.Utility
{
    abstract class DisplayInventoryBehavior : BlockEntityBehavior, ITexPositionSource, IDisplayInventory
    {
        public DisplayInventoryBehavior(BlockEntity blockentity) : base(blockentity)
        {

        }
        public Size2i AtlasSize => Capi.BlockTextureAtlas.Size;

        public string InventoryClassName => "displayinventorybehavior";

        public TextureAtlasPosition this[string textureCode]
        {
            get
            {
                AssetLocation texturePath = null;

                if (CurrentShape != null)
                {
                    if (CurrentShape.Textures == null || !CurrentShape.Textures.ContainsKey(textureCode))
                    {
                        if (CurrentObject.ItemClass == EnumItemClass.Item)
                        {
                            Item currentItem = CurrentObject as Item;

                            if (currentItem.Textures.ContainsKey(textureCode))
                                texturePath = currentItem.Textures[textureCode].Base;
                            else
                                texturePath = currentItem.FirstTexture.Base;
                        }
                        else
                        {
                            Block currentBlock = CurrentObject as Block;
                            texturePath = currentBlock.Textures["all"].Base;
                        }
                    }
                    else
                        CurrentShape.Textures.TryGetValue(textureCode, out texturePath);
                }

                TextureAtlasPosition texpos = null;

                if (texturePath != null)
                    texpos = Capi.BlockTextureAtlas[texturePath];

                if (texpos == null)
                {
                    IAsset texAsset = Capi.Assets.TryGet(texturePath.Clone().WithPathPrefixOnce("textures/").WithPathAppendixOnce(".png"));
                    if (texAsset != null)
                    {
                        Capi.BlockTextureAtlas.GetOrInsertTexture(texturePath, out _, out texpos);
                    }
                }

                return texpos;
            }
        }

        public ICoreClientAPI Capi { get; set; }
        public InventoryGeneric GenericDisplayInventory { get; set; }
        public CollectibleObject CurrentObject { get; set; }
        public Shape CurrentShape { get; set; }
        public MeshData[] Meshes { get; set; }

        public int InventorySize { get; set; } = 1;

        public void InitializeInventory()
        {
            GenericDisplayInventory = new InventoryGeneric(InventorySize, null, null);
        }
        /// <summary>
        /// Initialization is handled by Initialize(ICoreAPI api, JsonObject properties) for behaviors!
        /// </summary>
        /// <param name="api"></param>
        public void Initialize(ICoreAPI api)
        {
            throw new System.NotImplementedException();
        }
        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);

            Capi = api as ICoreClientAPI;
            Meshes = new MeshData[InventorySize];
        }
        public abstract void UpdateMeshes();
        public MeshData GenMesh(ICoreClientAPI capi, string shapePath)
        {
            CurrentShape = capi.Assets.TryGet(shapePath + ".json").ToObject<Shape>();

            capi.Tesselator.TesselateShape("container", CurrentShape, out MeshData wholeMesh, this);
            return wholeMesh;
        }
        public MeshData GenMesh(ItemStack stack)
        {
            MeshData mesh;
            IContainedMeshSource dynBlock = stack.Collectible as IContainedMeshSource;

            if (dynBlock != null)
            {
                mesh = dynBlock.GenMesh(stack, Capi.BlockTextureAtlas, Blockentity.Pos);
                mesh.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, Blockentity.Block.Shape.rotateY * GameMath.DEG2RAD, 0);
            }
            else
            {
                ICoreClientAPI capi = Api as ICoreClientAPI;
                if (stack.Class == EnumItemClass.Block)
                {
                    mesh = capi.TesselatorManager.GetDefaultBlockMesh(stack.Block).Clone();
                }
                else
                {
                    CurrentObject = stack.Collectible;
                    CurrentShape = null;
                    if (stack.Item.Shape != null)
                    {
                        CurrentShape = capi.TesselatorManager.GetCachedShape(stack.Item.Shape.Base);
                    }
                    capi.Tesselator.TesselateItem(stack.Item, out mesh, this);
                }
            }

            return mesh;
        }
        public void AddMesh(ITerrainMeshPool mesher, MeshData mesh)
        {
            mesher.AddMeshData(mesh);
        }
        public void AddMesh(ITerrainMeshPool mesher, MeshData mesh, Vec3f rotation)
        {
            mesher.AddMeshData(mesh.Clone()
                .Rotate(new Vec3f(0.5f, 0.5f, 0.5f), rotation.X, rotation.Y, rotation.Z));
        }
    }
}
