using AncientTools.Utility;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
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

        private SimpleParticleProperties woodParticles;

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

            woodParticles = InitializeWoodParticles();
        }
        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            return interactions;
        }
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);

            //-- Do not process the stripping action if the player is not sneaking, or no block is selected --//
            if (!byEntity.Controls.Sneak || blockSel == null)
                return;

            Block interactedBlock = api.World.BlockAccessor.GetBlock(blockSel.Position);

            if (interactedBlock.Attributes["woodStrippable"].Exists)
            {
                canStripBark = true;

                //-- Stripping time modifier increases the speed at which the wood is stripped. By default, it's based on tool tier --//
                strippingTime = api.World.Config.GetDouble("BaseBarkStrippingSpeed", 1.0) * this.Attributes["strippingTimeModifier"].AsDouble();

                byEntity.StartAnimation("adzestrip");

                if(api.Side == EnumAppSide.Server)
                    api.World.PlaySoundAt(new AssetLocation("ancienttools", "sounds/block/stripwood"), byEntity, null, true, 32f, 0.75f);
                else
                    SetParticleColourAndPosition(interactedBlock.GetRandomColor((ICoreClientAPI)api, blockSel.Position, BlockFacing.NORTH), blockSel.Position.ToVec3d()); 
                
                handling = EnumHandHandling.Handled;
            }
        }
        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {

            if (blockSel == null)
                return false;

            if (secondsUsed >= strippingTime)
                SpawnLoot(blockSel, byEntity);

            if (api.Side == EnumAppSide.Client)
                if (canStripBark == true)
                {
                    api.World.SpawnParticles(woodParticles);

                    SetParticleColour(api.World.BlockAccessor.GetBlock(blockSel.Position).GetRandomColor((ICoreClientAPI)api, blockSel.Position, BlockFacing.NORTH));
                }

            return true;
        }
        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            base.OnHeldInteractStop(secondsUsed, slot, byEntity, blockSel, entitySel);

            canStripBark = false;
        }
        //-- Spawn bark pieces when the player meets/exceeds the time it takes to strip the log. Also changes the interacted block to a stripped log variant --//
        private void SpawnLoot(BlockSelection blockSel, EntityAgent byEntity)
        {
            if (canStripBark == false)
                return;

            canStripBark = false;

            if (api.Side == EnumAppSide.Server)
            {
                Block interactedBlock = api.World.BlockAccessor.GetBlock(blockSel.Position);

                string logType = interactedBlock.LastCodePart(1);
                string logRotation = interactedBlock.LastCodePart();

                api.World.BlockAccessor.SetBlock(api.World.GetBlock(new AssetLocation("ancienttools", "strippedlog-" + logType + "-" + logRotation)).Id, blockSel.Position);
                api.World.BlockAccessor.MarkBlockDirty(blockSel.Position);

                for (int i = 0; i < api.World.Config.GetInt("BarkPerLog"); i++)
                    api.World.SpawnItemEntity(new ItemStack(api.World.GetItem(new AssetLocation("ancienttools", "bark-" + logType)), 1), blockSel.Position.ToVec3d() +
                        new Vec3d(0.5, 0.5, 0.5));
                    
                if(byEntity is EntityPlayer player)
                    this.DamageItem(api.World, byEntity, player.RightHandItemSlot, 1);
            }
        }
        private SimpleParticleProperties InitializeWoodParticles()
        {
            return new SimpleParticleProperties()
            {
                MinSize = 0.3f,
                MaxSize = 1.5f,

                MinQuantity = 5,
                AddQuantity = 5,

                MinVelocity = new Vec3f(-0.5f, -0.5f, -0.5f),
                AddVelocity = new Vec3f(1f, 2f, 1f),

                LifeLength = 0.5f,
                addLifeLength = 0.5f,

                GravityEffect = 0.3f,
                Bouncy = true,
                ParticleModel = EnumParticleModel.Cube
            };
        }
        private void SetParticleColourAndPosition(int colour, Vec3d minpos)
        {
            SetParticleColour(colour);

            woodParticles.MinPos = minpos;
            woodParticles.AddPos = new Vec3d(1, 1, 1);
        }
        private void SetParticleColour(int colour)
        {
            woodParticles.Color = colour;
        }
    }
}
