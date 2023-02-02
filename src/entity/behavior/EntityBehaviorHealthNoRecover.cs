using AncientTools.CollectibleBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace AncientTools.EntityBehaviors
{
    class EntityBehaviorHealthNoRecover : EntityBehaviorHealth
    {
        private WorldInteraction[] deconstructInteraction = null;
        private WorldInteraction[] repairInteraction = null;

        public EntityBehaviorHealthNoRecover(Entity entity) : base(entity)
        {

        }
        public override void Initialize(EntityProperties properties, JsonObject typeAttributes)
        {
            base.Initialize(properties, typeAttributes);

            deconstructInteraction = ObjectCacheUtil.GetOrCreate<WorldInteraction[]>(entity.Api, "cartDeconstructInventoryInteraction", () =>
            {
                List<ItemStack> deconstructItems = new List<ItemStack>();

                foreach (Item item in entity.Api.World.Items)
                {
                    if (item.HasBehavior<CollectibleBehaviorMobileStorageDestruction>())
                    {
                        deconstructItems.Add(new ItemStack(item));
                    }
                }
                return new WorldInteraction[]
                {
                    new WorldInteraction()
                    {
                        ActionLangCode = "ancienttools:entityhelp-cart-deconstruct",
                        MouseButton = EnumMouseButton.Right,
                        HotKeyCode = "shift",
                        Itemstacks = deconstructItems.ToArray()
                    }
                };
            });
            repairInteraction = ObjectCacheUtil.GetOrCreate<WorldInteraction[]>(entity.Api, "cartRepairInventoryInteraction", () =>
            {
                List<ItemStack> repairItems = new List<ItemStack>();

                foreach (Item item in entity.Api.World.Items)
                {
                    if (item.HasBehavior<CollectibleBehaviorMobileStorageRepair>())
                    {
                        repairItems.Add(new ItemStack(item));
                    }
                }
                return new WorldInteraction[]
                {
                    new WorldInteraction()
                    {
                        ActionLangCode = "ancienttools:entityhelp-cart-repair",
                        MouseButton = EnumMouseButton.Right,
                        HotKeyCode = "shift",
                        Itemstacks = repairItems.ToArray(),
                        RequireFreeHand = false
                    }
                };
            });
        }
        public override void GetInfoText(StringBuilder infotext)
        {
            infotext.Append(Lang.Get("ancienttools:entityinfo-cart-health", Health, MaxHealth ));
        }
        public override void OnGameTick(float deltaTime)
        {
            entity.World.FrameProfiler.Mark("not-bhhealth");
            if (entity.Pos.Y < -30)
            {
                entity.ReceiveDamage(new DamageSource()
                {
                    Source = EnumDamageSource.Void,
                    Type = EnumDamageType.Gravity
                }, 4);
            }
        }
        public override void OnEntityReceiveDamage(DamageSource damageSource, ref float damage)
        {
            if (!entity.Alive) return;
            if (damage <= 0) return;

            Health -= damage;
            entity.OnHurt(damageSource, damage);
            UpdateMaxHealth();

            if (Health <= 0)
            {
                Health = 0;

                entity.Die(
                    EnumDespawnReason.Death,
                    damageSource
                );
            }
            else
            {
                if (entity.Api.Side == EnumAppSide.Server)
                {
                    entity.PlayEntitySound("hurt");
                }
            }
        }
        public override string PropertyName()
        {
            return "HealthNoRecover";
        }
        public override WorldInteraction[] GetInteractionHelp(IClientWorldAccessor world, EntitySelection es, IClientPlayer player, ref EnumHandling handled)
        {
            handled = EnumHandling.Handled;

            if (Health < MaxHealth)
                return repairInteraction.Append(deconstructInteraction);
            else
                return deconstructInteraction;
        }
    }
}
