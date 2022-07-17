using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace AncientTools.Utility
{
    interface IDisplayInventory
    {
        Size2i AtlasSize { get; }
        InventoryGeneric GenericDisplayInventory { get; }
        string InventoryClassName { get; }
        TextureAtlasPosition this[string textureCode] { get; }

        ICoreClientAPI Capi { get; set; }
        CollectibleObject CurrentObject { get; set; }
        Shape CurrentShape { get; set; }
        MeshData[] Meshes { get; set; }
        int InventorySize { get; set; }

        void InitializeInventory();
        void Initialize(ICoreAPI api);
        /// <summary>
        /// A client-side method that must be used to update meshes every time the block is changed. You should use GenMesh here to store the mesh data in your block entity.
        /// </summary>
        void UpdateMeshes();
        /// <summary>
        /// Return mesh data of the shape from the provided filepath.
        /// </summary>
        /// <param name="capi">The client API needed to access the tesselator.</param>
        /// <param name="shapePath">The file path pointing to where the base mesh for the object will be.</param>
        /// <returns>The generated mesh data of the object.</returns>
        MeshData GenMesh(ICoreClientAPI capi, string shapePath);
        MeshData GenMesh(ItemStack stack);
        /// <summary>
        /// Add a mesh to the object to be rendered, likely in OnTesselation.
        /// </summary>
        /// <param name="mesher">The mesher provided by OnTesselation.</param>
        /// <param name="mesh">The mesh to be added to the current mesh.</param>
        void AddMesh(ITerrainMeshPool mesher, MeshData mesh);
        /// <summary>
        /// Add a mesh to the object to be rendered, likely in OnTesselation. Rotation controls the orientation of the new mesh.
        /// </summary>
        /// <param name="mesher">The mesher provided by OnTesselation.</param>
        /// <param name="mesh">The mesh to be added to the current mesh.</param>
        /// <param name="rotation">The rotation to apply to the new mesh.</param>
        void AddMesh(ITerrainMeshPool mesher, MeshData mesh, Vec3f rotation);
    }
}
