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
                AssetLocation texturePath = null;

                //-- If the shape does not have any textures hardcoded into it, the DisplayInventory system uses the texture of the held object. --//
                if(currentShape.Textures.Count == 0)
                {
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
                }
                else
                {
                    if (currentShape != null)
                    {
                        currentShape.Textures.TryGetValue(textureCode, out texturePath);
                    }
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
        protected Shape currentShape;
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
        /// <summary>
        /// A client-side method that must be used to update meshes every time the block is changed. You should use GenMesh here to store the mesh data in your block entity.
        /// </summary>
        public abstract void UpdateMeshes();
        /// <summary>
        /// Return mesh data of the shape from the provided filepath.
        /// </summary>
        /// <param name="capi">The client API needed to access the tesselator.</param>
        /// <param name="shapePath">The file path pointing to where the base mesh for the object will be.</param>
        /// <returns>The generated mesh data of the object.</returns>
        protected MeshData GenMesh(ICoreClientAPI capi, string shapePath)
        {
            currentShape = capi.Assets.TryGet(shapePath + ".json").ToObject<Shape>();

            capi.Tesselator.TesselateShape("container", currentShape, out MeshData wholeMesh, this);
            return wholeMesh;
        }
        /// <summary>
        /// Add a mesh to the object to be rendered, likely in OnTesselation.
        /// </summary>
        /// <param name="mesher">The mesher provided by OnTesselation.</param>
        /// <param name="mesh">The mesh to be added to the current mesh.</param>
        protected void AddMesh(ITerrainMeshPool mesher, MeshData mesh)
        {
            mesher.AddMeshData(mesh);
        }
        /// <summary>
        /// Add a mesh to the object to be rendered, likely in OnTesselation. Rotation controls the orientation of the new mesh.
        /// </summary>
        /// <param name="mesher">The mesher provided by OnTesselation.</param>
        /// <param name="mesh">The mesh to be added to the current mesh.</param>
        /// <param name="rotation">The rotation to apply to the new mesh.</param>
        protected void AddMesh(ITerrainMeshPool mesher, MeshData mesh, Vec3f rotation)
        {
            mesher.AddMeshData(mesh.Clone()
                .Rotate(new Vec3f(0.5f, 0.5f, 0.5f), rotation.X, rotation.Y, rotation.Z));
        }
    }
}
