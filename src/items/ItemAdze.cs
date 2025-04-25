using AncientTools.Utility;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace AncientTools.Items
{
    class ItemAdze: Item
    {
        WorldInteraction[] interactions = null;

        private bool canStripBark = false;
        private int barkAmount;
        private double strippingTime;

        private SimpleParticleProperties strippingWoodParticles;
        private SimpleParticleProperties barrelWoodParticles;

        private bool isSneaking;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            interactions = ObjectCacheUtil.GetOrCreate(api, "adzeInteractions", () =>
            {
                return new WorldInteraction[] {
                    new WorldInteraction()
                        {
                            ActionLangCode = "ancienttools:itemhelp-adze-stripbark",
                            HotKeyCode = "sneak",
                            MouseButton = EnumMouseButton.Right
                        },
                };
            });

            barkAmount = api.World.Config.GetInt("BarkPerLog", 4);
            strippingTime = api.World.Config.GetDouble("BaseBarkStrippingSpeed", 1.0);

            strippingWoodParticles = InitializeStrippingWoodParticles();
        }
        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            return interactions.Append(base.GetHeldInteractionHelp(inSlot));
        }
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);

            //-- Do not process the stripping action if the player is not sneaking, sprinting, or no block is selected --//
            if (blockSel == null)
                return;

            if(byEntity.Controls.Sneak)
            {
                isSneaking = true;

                ProcessLogStrippingStart(byEntity, blockSel, ref handling);
            }
            else
            {
                isSneaking = false;

                ProcessBarrelCreationStart(byEntity, blockSel, ref handling);
            }
        }
        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {

            if (blockSel == null && isSneaking != byEntity.Controls.Sneak)
                return false;

            if(isSneaking)
            {
                ProcessLogStrippingStep(secondsUsed, byEntity, blockSel);
            }
            else
            {
                ProcessBarrelCreationStep(secondsUsed, byEntity, blockSel);
            }

            return true;
        }
        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            base.OnHeldInteractStop(secondsUsed, slot, byEntity, blockSel, entitySel);

            canStripBark = false;
        }
        private void ProcessLogStrippingStart(EntityAgent byEntity, BlockSelection blockSel, ref EnumHandHandling handling)
        {
            if(byEntity.Controls.Sneak)
            {
                Block interactedBlock = api.World.BlockAccessor.GetBlock(blockSel.Position, BlockLayersAccess.SolidBlocks);

                if (interactedBlock.Attributes == null)
                    return;

                if (interactedBlock.Attributes["woodStrippable"].Exists && this.Attributes["strippingTimeModifier"].Exists)
                {
                    canStripBark = true;

                    //-- Stripping time modifier increases the speed at which the wood is stripped. By default, it's based on tool tier --//
                    strippingTime = api.World.Config.GetDouble("BaseBarkStrippingSpeed", 1.0) * this.Attributes["strippingTimeModifier"].AsDouble();

                    byEntity.StartAnimation("adzestrip");

                    if (api.Side == EnumAppSide.Server)
                        api.World.PlaySoundAt(new AssetLocation("ancienttools", "sounds/block/stripwood"), blockSel.Position, 0, null, true, 32f, 0.75f);
                    else
                        SetParticleColourAndPosition(interactedBlock.GetRandomColor((ICoreClientAPI)api, blockSel.Position, BlockFacing.NORTH), blockSel.Position.ToVec3d(), new Vec3d(1, 0.5, 1));

                    handling = EnumHandHandling.Handled;
                }
            }
        }
        private void ProcessLogStrippingStep(float secondsUsed, EntityAgent byEntity, BlockSelection blockSel)
        {
            if (secondsUsed >= strippingTime)
                SpawnLoot(blockSel, byEntity);

            if (api.Side == EnumAppSide.Client)
                if (canStripBark == true)
                {
                    api.World.SpawnParticles(strippingWoodParticles);

                    SetParticleColour(api.World.BlockAccessor.GetBlock(blockSel.Position, BlockLayersAccess.SolidBlocks).GetRandomColor((ICoreClientAPI)api, blockSel.Position, BlockFacing.NORTH));
                }
        }
        private void ProcessBarrelCreationStart(EntityAgent byEntity, BlockSelection blockSel, ref EnumHandHandling handling)
        {
            Block interactedBlock = api.World.BlockAccessor.GetBlock(blockSel.Position, BlockLayersAccess.SolidBlocks);

            if (interactedBlock.Attributes == null)
                return;

            if (interactedBlock.Attributes["primitiveBarrelProps"].Exists && interactedBlock.Attributes["primitiveBarrelProps"]["nextStage"].Exists && this.Attributes["strippingTimeModifier"].Exists)
            {
                canStripBark = true;

                //-- Stripping time modifier increases the speed at which the wood is stripped. By default, it's based on tool tier --//
                strippingTime = api.World.Config.GetDouble("BaseBarkStrippingSpeed", 1.0) * this.Attributes["strippingTimeModifier"].AsDouble();

                byEntity.StartAnimation("adzestrip");

                if (api.Side == EnumAppSide.Server)
                    api.World.PlaySoundAt(new AssetLocation("ancienttools", "sounds/block/stripwood"), blockSel.Position, 0, null, true, 32f, 0.75f);
                else
                    SetParticleColourPositionVelocity(interactedBlock.GetRandomColor((ICoreClientAPI)api, blockSel.Position, BlockFacing.NORTH), blockSel.Position.ToVec3d() + new Vec3d(0, 0.8, 0), new Vec3d(1, 0, 1), new Vec3f(-0.1f, 0.25f, -0.1f), new Vec3f(0.1f, 0.75f, 0.1f));

                handling = EnumHandHandling.Handled;
            }
        }
        private void ProcessBarrelCreationStep(float secondsUsed, EntityAgent byEntity, BlockSelection blockSel)
        {
            if (secondsUsed >= strippingTime)
                IncrementBarrelStage(blockSel, byEntity);

            if (api.Side == EnumAppSide.Client)
                if (canStripBark == true)
                {
                    api.World.SpawnParticles(strippingWoodParticles);

                    SetParticleColour(api.World.BlockAccessor.GetBlock(blockSel.Position, BlockLayersAccess.SolidBlocks).GetRandomColor((ICoreClientAPI)api, blockSel.Position, BlockFacing.NORTH));
                }
        }
        //-- Spawn bark pieces when the player meets/exceeds the time it takes to strip the log. Also changes the interacted block to a stripped log variant --//
        private void SpawnLoot(BlockSelection blockSel, EntityAgent byEntity)
        {
            if (canStripBark == false)
                return;

            canStripBark = false;

            if (api.Side == EnumAppSide.Server)
            {
                Block interactedBlock = api.World.BlockAccessor.GetBlock(blockSel.Position, BlockLayersAccess.SolidBlocks);

                AssetLocation resultingLog = new AssetLocation(interactedBlock.Attributes["woodStrippable"]["resultingLog"].AsString());

                api.World.BlockAccessor.SetBlock(api.World.GetBlock(resultingLog).Id, blockSel.Position);
                api.World.BlockAccessor.MarkBlockDirty(blockSel.Position);

                for (int i = 0; i < api.World.Config.GetInt("BarkPerLog") * interactedBlock.Attributes["woodStrippable"]["barkMultiplier"].AsFloat(); i++)
                    api.World.SpawnItemEntity(new ItemStack(api.World.GetItem(new AssetLocation("ancienttools", "bark-" + resultingLog.SecondCodePart())), 1), blockSel.Position.ToVec3d() +
                        new Vec3d(0.5, 0.5, 0.5));
                    
                if(byEntity is EntityPlayer player)
                    this.DamageItem(api.World, byEntity, player.RightHandItemSlot, 1);
            }
        }
        private void IncrementBarrelStage(BlockSelection blockSel, EntityAgent byEntity)
        {
            if (canStripBark == false)
                return;

            canStripBark = false;

            if(api.Side == EnumAppSide.Server)
            {
                Block interactedBlock = api.World.BlockAccessor.GetBlock(blockSel.Position, BlockLayersAccess.SolidBlocks);

                AssetLocation nextStage = new AssetLocation(interactedBlock.Attributes["primitiveBarrelProps"]["nextStage"].AsString());

                api.World.BlockAccessor.SetBlock(api.World.GetBlock(nextStage).Id, blockSel.Position);
                api.World.BlockAccessor.MarkBlockDirty(blockSel.Position);

                if (byEntity is EntityPlayer player)
                    this.DamageItem(api.World, byEntity, player.RightHandItemSlot, 1);
            }
        }
        private SimpleParticleProperties InitializeStrippingWoodParticles()
        {
            return new SimpleParticleProperties()
            {
                MinSize = 0.3f,
                MaxSize = 0.8f,

                MinQuantity = 5,
                AddQuantity = 3,

                MinVelocity = new Vec3f(-1f, 0, -1f),
                AddVelocity = new Vec3f(1f, 0.5f, 1f),

                LifeLength = 0.5f,
                addLifeLength = 0.5f,

                GravityEffect = 0.3f,
                Bounciness = 0.5f,
                ParticleModel = EnumParticleModel.Cube
            };
        }
        private void SetParticleColourAndPosition(int colour, Vec3d minpos, Vec3d addpos)
        {
            SetParticleColour(colour);

            strippingWoodParticles.MinPos = minpos;
            strippingWoodParticles.AddPos = addpos;
        }
        private void SetParticleColourPositionVelocity(int colour, Vec3d minpos, Vec3d addpos, Vec3f minvelocity, Vec3f addvelocity)
        {
            SetParticleColourAndPosition(colour, minpos, addpos);

            strippingWoodParticles.MinVelocity = minvelocity;
            strippingWoodParticles.AddVelocity = addvelocity;
        }
        private void SetParticleColour(int colour)
        {
            strippingWoodParticles.Color = colour;
        }
    }
}
