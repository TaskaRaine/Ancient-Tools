using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;

namespace AncientTools.EntityBehaviors
{
    class EntityBehaviorSalveHeal : EntityBehavior
    {
        public float TotalHealing = 10;
        public float HealingTime = 1;

        private long healingTickListener;
        private long healingStartTime;
        private long currentTime;

        private readonly DamageSource healing = new DamageSource() { Type = EnumDamageType.Heal };

        public EntityBehaviorSalveHeal(Entity entity) : base(entity)
        {
            if (entity.Api.Side == EnumAppSide.Server)
            {
                //-- Run the HealPlayer function every second --//
                healingTickListener = entity.Api.World.RegisterGameTickListener(HealPlayer, 1000);
                healingStartTime = entity.Api.World.ElapsedMilliseconds;
            }
        }
        public override void OnEntityDeath(DamageSource damageSourceForDeath)
        {
            RemoveSalveBehavior();

            base.OnEntityDeath(damageSourceForDeath);
        }
        public override void OnEntityDespawn(EntityDespawnReason despawn)
        {
            RemoveSalveBehavior();

            base.OnEntityDespawn(despawn);
        }
        private void HealPlayer(float dt)
        {
            currentTime = entity.Api.World.ElapsedMilliseconds;

            if(healingStartTime + (HealingTime * 1000) < currentTime)
            {
                RemoveSalveBehavior();
            }

            entity.ReceiveDamage(healing, TotalHealing / HealingTime);
        }
        //-- This behavior is removed whenever 'time is up'. --//
        private void RemoveSalveBehavior()
        {
            if (entity.Api.Side == EnumAppSide.Server)
            {
                entity.Api.World.UnregisterGameTickListener(healingTickListener);
                entity.RemoveBehavior(this);
            }
        }
        public override string PropertyName()
        {
            return "salveheal";
        }
    }
}
