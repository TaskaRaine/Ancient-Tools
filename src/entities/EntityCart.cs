using AncientTools.EntityRenderers;
using AncientTools.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace AncientTools.Entities
{
    class EntityCart : EntityMobileStorage
    {
        public CartRenderer Renderer { get; set; }
        protected override int StorageBlocksCount { get; set; } = 1;
        ItemSlot CartInventorySlot { 
            get { return MobileStorageInventory[0]; } 
        }
        public override void Initialize(EntityProperties properties, ICoreAPI api, long InChunkIndex3d)
        {
            base.Initialize(properties, api, InChunkIndex3d);
        }
        public override void OnGameTick(float dt)
        {
            base.OnGameTick(dt);

            if (Api.Side == EnumAppSide.Client)
            {
                if (AttachedEntity != null)
                {
                    SetLookAtVector(EntityTransform.XYZFloat, AttachedEntity.SidedPos.XYZFloat, AttachedEntity.LocalEyePos.ToVec3f());
                }

                Renderer.TesselateShape();
            }
        }
        public override void OnInteract(EntityAgent byEntity, ItemSlot itemslot, Vec3d hitPosition, EnumInteractMode mode)
        {
            base.OnInteract(byEntity, itemslot, hitPosition, mode);

            if (mode == EnumInteractMode.Interact)
            {
                if(byEntity.Controls.ShiftKey)
                {
                    if(AttachedEntity == null)
                    {
                        double entityDistance = EntityTransform.DistanceTo(byEntity.SidedPos);

                        if (entityDistance <= 2.0)
                        {
                            AttachedEntity = byEntity;

                            if(Api.Side == EnumAppSide.Client)
                                SetLookAtVector(EntityTransform.XYZFloat, AttachedEntity.SidedPos.XYZFloat, AttachedEntity.LocalEyePos.ToVec3f());
                        }
                    }
                    else if(byEntity.EntityId == AttachedEntity.EntityId)
                    {
                        AttachedEntity = null;

                        if(Api.Side == EnumAppSide.Client)
                            DropCart();
                    }
                }
                else if(itemslot.Empty && !CartInventorySlot.Empty && byEntity.Controls.CtrlKey)
                {
                    if (CartInventorySlot.Itemstack?.Collectible?.Attributes?.KeyExists("cartPlacable") == true)
                    {
                        byEntity.TryGiveItemStack(CartInventorySlot.TakeOutWhole());
                        CartInventorySlot.MarkDirty();
                    }
                }
                else if(CartInventorySlot.Empty)
                {
                    if(itemslot.Itemstack?.Collectible?.Attributes?.KeyExists("cartPlacable") == true)
                    {
                        CartInventorySlot.Itemstack = itemslot.TakeOut(1);
                        CartInventorySlot.MarkDirty();
                        itemslot.MarkDirty();

                        CreateNewStorageBlockInventory(0);

                        if (Api.Side == EnumAppSide.Client)
                            Renderer.AssignStoragePlacementProperties(CartInventorySlot.Itemstack?.Collectible?.Attributes["cartPlacable"]);
                    }
                }
                else
                {
                    SendOpenInventoryPacket((EntityPlayer)byEntity, 0);
                }
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
