using AncientTools.EntityBehaviors;
using AncientTools.Utility;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace AncientTools.CollectibleBehaviors
{
    class CollectibleBehaviorMobileStorageDestruction : CollectibleBehavior
    {
        private int DestructionRate { get; set; } = 10;
        private float DestructionInterval { get; set; } = 2;
        private int DurabilityLoss { get; set; } = 1;

        private float PreviousTickedTime { get; set;} = 0;

        private EntityBehaviorHealthNoRecover EntityHealth { get; set; }

        public CollectibleBehaviorMobileStorageDestruction(CollectibleObject collObj) : base(collObj)
        {

        }
        public override void Initialize(JsonObject properties)
        {
            if(properties["destructionRate"].Exists)
                DestructionRate = properties["destructionRate"].AsInt();

            if(properties["destructionInterval"].Exists)
                DestructionInterval = properties["destructionInterval"].AsFloat();

            if(properties["durabilityLoss"].Exists)
                DurabilityLoss = properties["durabilityLoss"].AsInt();
        }
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
        {
            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handHandling, ref handling);

            if(entitySel == null)
                return;

            if(entitySel.Entity is EntityMobileStorage && byEntity.Controls.Sneak)
            {
                if(byEntity.Api.Side == EnumAppSide.Server)
                    EntityHealth = entitySel.Entity.GetBehavior<EntityBehaviorHealthNoRecover>();

                handling = EnumHandling.PreventDefault;
                handHandling = EnumHandHandling.PreventDefault;
            }
        }
        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandling handling)
        {
            if (entitySel == null || slot.Empty)
                return false;

            if (entitySel.Entity is EntityMobileStorage && byEntity.Controls.Sneak)
            {
                if (secondsUsed - PreviousTickedTime > DestructionInterval)
                {
                    PreviousTickedTime = secondsUsed;

                    if (byEntity.Api.Side == EnumAppSide.Server)
                    {
                        EntityHealth.Health -= DestructionRate;

                        slot.Itemstack.Collectible.DamageItem(byEntity.World, byEntity, slot, DurabilityLoss);
                    }
                }

                handling = EnumHandling.Handled;
                return true;
            }

            return false;
        }
        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandling handling)
        {
            EntityHealth = null;
            PreviousTickedTime = 0;
        }
    }
}
