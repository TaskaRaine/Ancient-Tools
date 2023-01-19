using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace AncientTools.Utility
{
    interface IAttributeVariant
    {
        Size2i AtlasSize { get; }
        TextureAtlasPosition this[string textureCode] { get; }
        ICoreClientAPI Capi { get; set; }

        Shape CurrentShape { get; set; }
        string CurrentType { get; set; }

        void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo);
        MeshData GenMesh();
    }
}
