using AncientTools.EntityRenderers;
using AncientTools.Utility;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace AncientTools.Entities
{
    class EntityCart: EntityMobileStorage
    {
        CartRenderer Renderer { get; set; }

        public override void Initialize(EntityProperties properties, ICoreAPI api, long InChunkIndex3d)
        {
            base.Initialize(properties, api, InChunkIndex3d);

            if (api.Side == EnumAppSide.Client)
            {
                //Renderer.UpdateMesh(GenMesh());
            }
        }
        public override void OnGameTick(float dt)
        {
            base.OnGameTick(dt);

            if (AttachedEntity == null)
                return;

            if(Api.Side == EnumAppSide.Client)
            {
                Renderer.SetLookAtVector(EntityTransform.XYZFloat, AttachedEntity.SidedPos.XYZFloat, AttachedEntity.LocalEyePos.ToVec3f());
                Renderer.TesselateShape();
            }

            ServerPos.SetFrom(EntityTransform);
            Pos.SetFrom(ServerPos);
        }
        public override void OnInteract(EntityAgent byEntity, ItemSlot itemslot, Vec3d hitPosition, EnumInteractMode mode)
        {
            base.OnInteract(byEntity, itemslot, hitPosition, mode);

            if(mode == EnumInteractMode.Interact)
            {
                //if(AttachedEntity == null)
                //{
                    double entityDistance = EntityTransform.DistanceTo(byEntity.SidedPos);

                    if (entityDistance >= 1.0 && entityDistance <= 2.0)
                    {
                        AttachedEntity = byEntity;

                        SyncPosition();

                        if(Api.Side == EnumAppSide.Client)
                            Renderer.SetLookAtVector(EntityTransform.XYZFloat, AttachedEntity.SidedPos.XYZFloat, AttachedEntity.LocalEyePos.ToVec3f());
                    }
                //}
            }
            else if(mode == EnumInteractMode.Attack)
            {
                Die();
            }
        }
        public void SetRendererReference(CartRenderer renderer)
        {
            Renderer = renderer;
        }
    }
}
