using AncientTools.EntityBehaviors;
using AncientTools.Utility;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace AncientTools.CollectibleBehaviors
{
    class CollectibleBehaviorMobileStorageRepair: CollectibleBehavior
    {
        private int RepairAmount { get; set; } = 5;
        private float RepairInterval { get; set; } = 1;
        private int DurabilityLoss { get; set; } = 1;

        private float PreviousTickedTime { get; set; } = 0;

        private EntityBehaviorHealthNoRecover EntityHealth { get; set; }

        private string StorageType { get; set; }
        private CollectibleObject LeftHandObject { get; set; }

        private DamageSource repairDamageSource = new DamageSource() { Type = EnumDamageType.Heal };

        public CollectibleBehaviorMobileStorageRepair(CollectibleObject collObj) : base(collObj)
        {

        }
        public override void Initialize(JsonObject properties)
        {
            base.Initialize(properties);

            if (properties["repairAmount"].Exists)
                RepairAmount = properties["repairAmount"].AsInt();

            if (properties["repairInterval"].Exists)
                RepairInterval = properties["repairInterval"].AsFloat();

            if (properties["durabilityLoss"].Exists)
                DurabilityLoss = properties["durabilityLoss"].AsInt();
        }
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
        {
            //base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handHandling, ref handling);

            if (entitySel == null || byEntity.LeftHandItemSlot?.Empty == true)
                return;

            if (entitySel.Entity is EntityMobileStorage && byEntity.Controls.ShiftKey)
            {
                LeftHandObject = byEntity.LeftHandItemSlot.Itemstack.Collectible;

                StorageType = entitySel.Entity.WatchedAttributes.GetAsString("type", string.Empty);
                StorageType = StorageType.Split('-')[1];

                if (LeftHandObject.CodeWithVariant("wood", StorageType).Equals(LeftHandObject.Code))
                {
                    if (byEntity.Api.Side == EnumAppSide.Server)
                        EntityHealth = entitySel.Entity.GetBehavior<EntityBehaviorHealthNoRecover>();

                    repairDamageSource.SourceEntity = byEntity;

                    handling = EnumHandling.PreventDefault;
                    handHandling = EnumHandHandling.PreventDefault;
                }
            }
        }

        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandling handling)
        {
            handling = EnumHandling.PreventSubsequent;

            if (entitySel == null || LeftHandObject == null || !LeftHandObject.CodeWithVariant("wood", StorageType).Equals(LeftHandObject.Code))
                return false;

            if (entitySel.Entity is EntityMobileStorage && byEntity.Controls.ShiftKey)
            {
                if (secondsUsed - PreviousTickedTime > RepairInterval)
                {
                    if (byEntity.Api.Side == EnumAppSide.Server)
                    {
                        if (!byEntity.LeftHandItemSlot.Empty)
                        {
                            CollectibleObject leftHandObject = byEntity.LeftHandItemSlot.Itemstack.Collectible;

                            if (leftHandObject.CodeWithVariant("wood", StorageType).Equals(leftHandObject.Code))
                            {
                                EntityHealth.Health += RepairAmount;

                                entitySel.Entity.OnHurt(repairDamageSource, -RepairAmount);
                                entitySel.Entity.PlayEntitySound("hurt");

                                slot.Itemstack.Collectible.DamageItem(byEntity.World, byEntity, slot, DurabilityLoss);
                                byEntity.LeftHandItemSlot.TakeOut(1);
                                byEntity.LeftHandItemSlot.MarkDirty();

                                PreviousTickedTime = secondsUsed;
                            }
                            else
                                return false;

                            if (EntityHealth.Health > EntityHealth.MaxHealth)
                            {
                                EntityHealth.Health = EntityHealth.MaxHealth;
                                return false;
                            }
                        }
                        else
                            return false;
                    }
                }

                return true;
            }

            return false;
        }
        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandling handling)
        {
            LeftHandObject = null;

            EntityHealth = null;
            PreviousTickedTime = 0;
        }
    }
}
