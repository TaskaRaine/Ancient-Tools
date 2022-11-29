using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace AncientTools.Utility
{
    abstract class EntityMobileStorage: EntityAgent
    {
        public ICoreClientAPI Capi { get; set; }
        public MeshData[] Meshes { get; set; }

        public EntityAgent AttachedEntity { get; set; }
        public EntityPos EntityTransform { get; set; }
        public Size2i AtlasSize => Capi.EntityTextureAtlas.Size;

        public override void Initialize(EntityProperties properties, ICoreAPI api, long InChunkIndex3d)
        {
            base.Initialize(properties, api, InChunkIndex3d);

            EntityTransform = this.SidedPos;

            if (api.Side == EnumAppSide.Client)
                Capi = (ICoreClientAPI)api;
        }
        public void SetEntityPosition(Vec3d position)
        {
            EntityTransform.SetPos(position);

            SyncPosition();
        }
        public void SyncPosition()
        {
            ServerPos.SetFrom(EntityTransform);
            Pos.SetFrom(ServerPos);
        }

    }
}
