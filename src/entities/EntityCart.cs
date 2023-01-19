using AncientTools.EntityRenderers;
using AncientTools.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace AncientTools.Entities
{
    class EntityCart : EntityMobileStorage
    {
        public CartRenderer Renderer { get; set; }
        public override int StorageBlocksCount { get; protected set; } = 1;

        ItemSlot CartInventorySlot { 
            get { return MobileStorageInventory[0]; } 
        }

        private WorldInteraction[] placeInventoryInteraction = null;
        private WorldInteraction[] takeInventoryInteraction = null;
        private WorldInteraction[] liftCartInteraction = null;
        private WorldInteraction[] dropCartInteraction = null;
        private WorldInteraction[] accessInventoryInteraction = null;

        public override void Initialize(EntityProperties properties, ICoreAPI api, long InChunkIndex3d)
        {
            base.Initialize(properties, api, InChunkIndex3d);

            placeInventoryInteraction = ObjectCacheUtil.GetOrCreate<WorldInteraction[]>(Api, "cartPlaceInventoryInteraction", () =>
            {
                List<ItemStack> cartStorageBlocks = new List<ItemStack>();

                foreach (Block block in Api.World.Blocks)
                {
                    if (block.Code == null)
                        continue;

                    if (block.Attributes?["cartPlacable"].Exists == true)
                    {
                        cartStorageBlocks.Add(new ItemStack(block));
                    }
                }

                return new WorldInteraction[]
                {
                    new WorldInteraction()
                    {
                        ActionLangCode = "ancienttools:entityhelp-cart-place-inventory",
                        MouseButton = EnumMouseButton.Right,
                        Itemstacks = cartStorageBlocks.ToArray()
                    }
                };
            });
            takeInventoryInteraction = ObjectCacheUtil.GetOrCreate<WorldInteraction[]>(Api, "cartTakeInventoryInteraction", () => {
                return new WorldInteraction[]
                {
                    new WorldInteraction()
                    {
                        ActionLangCode = "ancienttools:entityhelp-cart-take-inventory",
                        MouseButton = EnumMouseButton.Right,
                        HotKeyCode = "ctrl",
                        RequireFreeHand = true
                    }
                };
            });
            liftCartInteraction = ObjectCacheUtil.GetOrCreate<WorldInteraction[]>(Api, "cartLiftInteraction", () =>
            {
                return new WorldInteraction[]
                {
                    new WorldInteraction()
                    {
                        ActionLangCode = "ancienttools:entityhelp-cart-pick-up",
                        MouseButton = EnumMouseButton.Right,
                        HotKeyCode = "shift"
                    }
                };
            });
            dropCartInteraction = ObjectCacheUtil.GetOrCreate<WorldInteraction[]>(Api, "cartDropInteraction", () => {
                return new WorldInteraction[]
                {
                    new WorldInteraction()
                    {
                        ActionLangCode = "ancienttools:entityhelp-cart-drop",
                        MouseButton = EnumMouseButton.Right,
                        HotKeyCode = "shift"
                    }
                };
            });
            accessInventoryInteraction = ObjectCacheUtil.GetOrCreate<WorldInteraction[]>(Api, "cartAccessInventoryInteraction", () =>
            {
                return new WorldInteraction[]
                {
                    new WorldInteraction()
                    {
                        ActionLangCode = "ancienttools:entityhelp-cart-open-inventory",
                        MouseButton = EnumMouseButton.Right
                    }
                };
            });
        }
        public override WorldInteraction[] GetInteractionHelp(IClientWorldAccessor world, EntitySelection es, IClientPlayer player)
        {
            if(CartInventorySlot.Empty)
            {
                if(AttachedEntity == null)
                {
                    return placeInventoryInteraction.Append(liftCartInteraction);
                }
                else
                {
                    if(player == AttachedEntity)
                    {
                        return placeInventoryInteraction.Append(dropCartInteraction);
                    }

                    return placeInventoryInteraction.Append(dropCartInteraction);
                }    
            }
            else
            {
                if (AttachedEntity == null)
                {
                    if(!MobileStorageInventory.InventoryContainsItems(0))
                    {
                        return takeInventoryInteraction.Append(liftCartInteraction.Append(accessInventoryInteraction));
                    }

                    return liftCartInteraction.Append(accessInventoryInteraction);
                }
                else
                {
                    if (!MobileStorageInventory.InventoryContainsItems(0))
                    {
                        return takeInventoryInteraction.Append(dropCartInteraction.Append(accessInventoryInteraction));
                    }

                    return dropCartInteraction.Append(accessInventoryInteraction);
                }
            }
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

            if(AttachedEntity != null)
            {
                AttachedEntity.Stats.Set("walkspeed", "cartspeedmodifier", (float)-0.2, false);
            }
        }
        public override void OnInteract(EntityAgent byEntity, ItemSlot itemslot, Vec3d hitPosition, EnumInteractMode mode)
        {
            if (mode == EnumInteractMode.Interact)
            {
                if(byEntity.ServerControls.ShiftKey)
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
                        if (AttachedEntity != null)
                        {
                            AttachedEntity.Stats.Remove("walkspeed", "cartspeedmodifier");
                        }

                        AttachedEntity = null;

                        if(Api.Side == EnumAppSide.Client)
                            DropCart();
                    }
                }
                else if(itemslot.Empty && !CartInventorySlot.Empty && byEntity.ServerControls.CtrlKey)
                {
                    if (CartInventorySlot.Itemstack?.Collectible?.Attributes?.KeyExists("cartPlacable") == true)
                    {
                        if (!MobileStorageInventory.InventoryContainsItems(0))
                        {
                            byEntity.TryGiveItemStack(CartInventorySlot.TakeOutWhole());
                            CartInventorySlot.MarkDirty();

                            MobileStorageInventory.RemoveEmptyStorage(0);
                        }
                    }
                }
                else if(CartInventorySlot.Empty)
                {
                    bool collectibleCartPlacable = false;
                    string type = string.Empty;

                    if (itemslot.Itemstack?.Collectible?.Attributes?.KeyExists("cartPlacable") == true)
                        collectibleCartPlacable = true;

                    if(collectibleCartPlacable)
                    {
                        if(itemslot.Itemstack.Collectible is BlockGenericTypedContainer)
                        {
                            type = itemslot.Itemstack.Attributes?.GetString("type");

                            if (!itemslot.Itemstack.Collectible.Attributes["mobileStorageProps"][type].Exists
                                || !itemslot.Itemstack.Collectible.Attributes["cartPlacable"][type].Exists)
                                return;
                        }

                        CartInventorySlot.Itemstack = itemslot.TakeOut(1);
                        CartInventorySlot.MarkDirty();
                        itemslot.MarkDirty();

                        //-- Allows for both typed and non-typed containers to use the mobile storage system --//
                        if (type != string.Empty)
                        {
                            MobileStorageInventory.GenerateEmptyStorageInventory(CartInventorySlot.Itemstack.Collectible.Attributes["mobileStorageProps"][type]["quantitySlots"].AsInt());
                            
                            if (Api.Side == EnumAppSide.Client)
                                Renderer.AssignStoragePlacementProperties(CartInventorySlot.Itemstack?.Collectible?.Attributes["cartPlacable"][type]);
                        }
                        else
                        {
                            MobileStorageInventory.GenerateEmptyStorageInventory(CartInventorySlot.Itemstack.Collectible.Attributes["mobileStorageProps"]["quantitySlots"].AsInt());

                            if (Api.Side == EnumAppSide.Client)
                                Renderer.AssignStoragePlacementProperties(CartInventorySlot.Itemstack?.Collectible?.Attributes["cartPlacable"]);
                        }
                    }
                }
                else
                {
                    SendOpenInventoryPacket((EntityPlayer)byEntity, 0);

                    AnimateOpen();
                }
            }
            else if(mode == EnumInteractMode.Attack)
            {
                float damage = itemslot.Itemstack == null ? 0.5f : itemslot.Itemstack.Collectible.GetAttackPower(itemslot.Itemstack);
                int damagetier = itemslot.Itemstack == null ? 0 : itemslot.Itemstack.Collectible.ToolTier;

                damage *= byEntity.Stats.GetBlended("meleeWeaponsDamage");

                if(!IsActivityRunning("invulnerable"))
                    itemslot?.Itemstack?.Collectible.OnAttackingWith(byEntity.World, byEntity, this, itemslot);

                DamageSource dmgSource = new DamageSource()
                {
                    Source = (byEntity as EntityPlayer).Player == null ? EnumDamageSource.Entity : EnumDamageSource.Player,
                    SourceEntity = byEntity,
                    Type = EnumDamageType.BluntAttack,
                    HitPosition = hitPosition,
                    DamageTier = damagetier,
                    KnockbackStrength = 0
                };

                if (ReceiveDamage(dmgSource, damage))
                {
                    byEntity.DidAttack(dmgSource, this);
                }
            }
        }
        public override bool ReceiveDamage(DamageSource damageSource, float damage)
        {
            return base.ReceiveDamage(damageSource, damage);
        }
        public override void OnEntityDespawn(EntityDespawnReason despawn)
        {
            if (AttachedEntity != null)
            {
                AttachedEntity.Stats.Set("walkspeed", "cartspeedmodifier", (float)-0.2, false);
            }

            if (despawn.reason == EnumDespawnReason.Combusted || despawn.reason == EnumDespawnReason.Death)
            {
                MobileStorageInventory.DropAll(this.Pos.XYZ);
            }

            base.OnEntityDespawn(despawn);
        }
        public override string GetName()
        {
            string type = this.WatchedAttributes.GetString("type", "unknown");

            return Lang.GetMatching(Code?.Domain + AssetLocation.LocationSeparator + "entity-" + type + "-" + Code?.Path);
        }
        public void SetRendererReference(CartRenderer renderer)
        {
            Renderer = renderer;
        }
        private void AnimateOpen()
        {
            AnimManager.StartAnimation("lidopen");
        }
    }
}
