using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace AncientTools.EntityBehaviors
{
    class EntityBehaviorHealthNoRecover : EntityBehaviorHealth
    {
        public EntityBehaviorHealthNoRecover(Entity entity) : base(entity)
        {

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
            base.OnEntityReceiveDamage(damageSource, ref damage);
        }
        public override string PropertyName()
        {
            return "HealthNoRecover";
        }
    }
}
