using AncientTools.EntityBehaviors;
using AncientTools.Utility;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace AncientTools.CollectibleBehaviors
{
    class CollectibleBehaviorMobileStorageDestruction : CollectibleBehavior
    {
        private int DestructionAmount { get; set; } = 10;
        private float DestructionInterval { get; set; } = 2;
        private int DurabilityLoss { get; set; } = 1;

        private float PreviousTickedTime { get; set;} = 0;

        //private ILoadedSound sawSound;

        //private Random rand;

        private EntityBehaviorHealthNoRecover EntityHealth { get; set; }

        private DamageSource deconstructionDamageSource = new DamageSource() { Type = EnumDamageType.SlashingAttack };

        public CollectibleBehaviorMobileStorageDestruction(CollectibleObject collObj) : base(collObj)
        {

        }
        ~CollectibleBehaviorMobileStorageDestruction()
        {
            //if (sawSound != null) sawSound.Dispose();
        }
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            if(api.Side == EnumAppSide.Client)
            {
                /*
                sawSound = ((IClientWorldAccessor)api.World).LoadSound(new SoundParams()
                {
                    Location = new AssetLocation("ancienttools", "sounds/item/sawingwood.ogg"),
                    ShouldLoop = false,
                    DisposeOnFinish = false,
                    Volume = 1.0f
                });
                
                rand = new Random();
                */
            }
        }
        public override void Initialize(JsonObject properties)
        {
            if(properties["destructionAmount"].Exists)
                DestructionAmount = properties["destructionAmount"].AsInt();

            if(properties["destructionInterval"].Exists)
                DestructionInterval = properties["destructionInterval"].AsFloat();

            if(properties["durabilityLoss"].Exists)
                DurabilityLoss = properties["durabilityLoss"].AsInt();
        }
        public override void OnUnloaded(ICoreAPI api)
        {
            base.OnUnloaded(api);
            /*
            if (sawSound != null)
            {
                sawSound.Stop();
                sawSound.Dispose();
            }
            */
        }
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
        {
            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handHandling, ref handling);

            if(entitySel == null || 
                CanAccessInClaim(byEntity as EntityPlayer, entitySel.Position.AsBlockPos, EnumBlockAccessFlags.BuildOrBreak) == false ||
                CanAccessInClaim(byEntity as EntityPlayer, entitySel.Position.AsBlockPos, EnumBlockAccessFlags.Use) == false)
                    return;

            if(entitySel.Entity is EntityMobileStorage && byEntity.Controls.Sneak)
            {
                if(byEntity.Api.Side == EnumAppSide.Server)
                    EntityHealth = entitySel.Entity.GetBehavior<EntityBehaviorHealthNoRecover>();

                deconstructionDamageSource.SourceEntity = byEntity;

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
                if(byEntity.Api.Side == EnumAppSide.Client)
                {
                    /*
                   sawSound.SetPosition(entitySel.Position.ToVec3f());


                   if (!sawSound.IsPlaying)
                   {
                       sawSound.SetPitchOffset(rand.Next(-2, 2) / 10f);
                       sawSound.Start();
                   }
                   */
                }

                if (secondsUsed - PreviousTickedTime > DestructionInterval)
                {
                    if (byEntity.Api.Side == EnumAppSide.Server)
                    {
                        EntityHealth.Health -= DestructionAmount;

                        entitySel.Entity.OnHurt(new DamageSource(), DestructionAmount);
                        entitySel.Entity.PlayEntitySound("hurt");

                        slot.Itemstack.Collectible.DamageItem(byEntity.World, byEntity, slot, DurabilityLoss);

                        if (EntityHealth.Health <= 0)
                            entitySel.Entity.Die();

                        PreviousTickedTime = secondsUsed;
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

            /*
            if(byEntity.Api.Side == EnumAppSide.Client)
            {
                sawSound.FadeOutAndStop(0.2f);
            }
            */
        }
        private bool CanAccessInClaim(EntityPlayer player, BlockPos pos, EnumBlockAccessFlags accessType)
        {
            LandClaim[] claims = player.Api.World.Claims.Get(pos);

            if (claims == null || claims.Length == 0)
                return true;

            foreach (LandClaim claim in claims)
            {
                if (claim.AllowUseEveryone || claim.TestPlayerAccess(player.Player, accessType) != EnumPlayerAccessResult.Denied)
                    return true;
            }

            return false;
        }
    }
}
