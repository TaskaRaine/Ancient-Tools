using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace AncientTools.EntityBehaviors
{
    class EntityBehaviorMobileStorageDebuffRemoval: EntityBehavior
    {
        public EntityBehaviorMobileStorageDebuffRemoval(Entity entity) : base(entity)
        {

        }

        public override void OnEntityDeath(DamageSource damageSourceForDeath)
        {
            RemoveDebuff();

            base.OnEntityDeath(damageSourceForDeath);
        }
        public override void OnEntityDespawn(EntityDespawnReason despawn)
        {
            RemoveDebuff();

            base.OnEntityDespawn(despawn);
        }
        public override string PropertyName()
        {
            return "mobilestoragedebuffremoval";
        }
        private void RemoveDebuff()
        {
            if(entity.Stats["walkspeed"].ValuesByKey.ContainsKey("cartspeedmodifier"))
                entity.Stats.Remove("walkspeed", "cartspeedmodifier");
        }
    }
}
