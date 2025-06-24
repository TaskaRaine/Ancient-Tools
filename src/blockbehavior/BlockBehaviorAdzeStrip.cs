using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace AncientTools.BlockBehaviors
{
    public class BlockBehaviorAdzeStrip : BlockBehavior
    {
        public WorldInteraction[] StripLogInteractions { get; set; } = null;

        private int BarkAmount { get; set; } = 4;
        private double StrippingTime { get; set; } = 1.0;

        private SimpleParticleProperties WoodParticles { get; set; }

        public BlockBehaviorAdzeStrip(Block block) : base(block)
        {

        }
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            List<ItemStack> adze = new List<ItemStack>();

            foreach (Item item in api.World.Items)
            {
                if (item.Attributes == null) continue;

                if (item.Attributes["strippingTimeModifier"].Exists)
                    adze.Add(new ItemStack(item));
            }

            WorldInteraction stripInteraction = new WorldInteraction()
            {
                ActionLangCode = "ancienttools:blockhelp-adze-strip",
                MouseButton = EnumMouseButton.Right,
                HotKeyCode = "sneak",
                Itemstacks = adze.ToArray()
            };

            StripLogInteractions = ObjectCacheUtil.GetOrCreate(api, "stripLogInteractions", () =>
            {
                return new WorldInteraction[]
                {
                    stripInteraction
                };
            });

            BarkAmount = api.World.Config.GetInt("BarkPerLog", BarkAmount);
            StrippingTime = api.World.Config.GetDouble("BaseBarkStrippingSpeed", StrippingTime);

            WoodParticles = InitializeWoodParticles();
        }
        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer, ref EnumHandling handling)
        {
            return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer, ref handling).Append(StripLogInteractions);
        }
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            if (blockSel == null || !byPlayer.Entity.Controls.Sneak || byPlayer.InventoryManager.ActiveHotbarSlot.Empty)
                return false;

            ItemStack interactedStack = byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack;

            if (interactedStack.Attributes == null || !interactedStack.Collectible.Attributes["strippingTimeModifier"].Exists)
                return false;


            StrippingTime = world.Config.GetDouble("BaseBarkStrippingSpeed", 1.0) * interactedStack.Collectible.Attributes["strippingTimeModifier"].AsDouble();

            byPlayer.Entity.StartAnimation("adzestrip");
            world.PlaySoundAt(new AssetLocation("ancienttools", "sounds/block/stripwood"), blockSel.Position, 0, byPlayer, true, 32f, 0.75f);

            if(world.Api.Side == EnumAppSide.Client)
                SetParticleColourAndPosition(block.GetRandomColor((ICoreClientAPI)world.Api, blockSel.Position, BlockFacing.NORTH), blockSel.Position.ToVec3d(), new Vec3d(1, 0.5, 1));

            handling = EnumHandling.PreventSubsequent;
            return true;
        }
        public override bool OnBlockInteractStep(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            if (blockSel == null || !byPlayer.Entity.Controls.Sneak || byPlayer.InventoryManager.ActiveHotbarSlot.Empty)
                return false;

            ItemStack interactedStack = byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack;

            if (interactedStack.Attributes == null || !interactedStack.Collectible.Attributes["strippingTimeModifier"].Exists)
                return false;

            if(world.Side == EnumAppSide.Client)
            {
                world.SpawnParticles(WoodParticles);
                SetParticleColour(block.GetRandomColor((ICoreClientAPI)world.Api, blockSel.Position, BlockFacing.NORTH));
            }

            if(secondsUsed >= StrippingTime)
            {
                byPlayer.Entity.StopAnimation("adzestrip");

                handling = EnumHandling.Handled;
                return false;
            }

            handling = EnumHandling.PreventSubsequent;
            return true;
        }
        public override void OnBlockInteractStop(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            if (block.Attributes == null || !block.Attributes["woodStrippable"].Exists || !byPlayer.Entity.Controls.Sneak)
                return;

            ItemStack interactedStack = byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack;

            if (interactedStack.Attributes == null || !interactedStack.Collectible.Attributes["strippingTimeModifier"].Exists)
                return;

            if (secondsUsed >= StrippingTime)
            {
                Block strippedLog = world.GetBlock(new AssetLocation(block.Attributes["woodStrippable"]["resultingLog"].ToString()));

                world.BlockAccessor.SetBlock(strippedLog.Id, blockSel.Position);
                world.BlockAccessor.MarkBlockDirty(blockSel.Position);

                for (int i = 0; i < world.Config.GetInt("BarkPerLog") * block.Attributes["woodStrippable"]["barkMultiplier"].AsFloat(); i++)
                    world.SpawnItemEntity(new ItemStack(world.GetItem(new AssetLocation("ancienttools", "bark-" + strippedLog.VariantStrict["wood"])), 1), blockSel.Position.ToVec3d() +
                        new Vec3d(0.5, 0.5, 0.5));

                    byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Collectible.DamageItem(world, byPlayer.Entity, byPlayer.InventoryManager.ActiveHotbarSlot, 1);

                handling = EnumHandling.Handled;
            }
        }
        private SimpleParticleProperties InitializeWoodParticles()
        {
            return new SimpleParticleProperties()
            {
                MinSize = 0.3f,
                MaxSize = 0.8f,

                MinQuantity = 5,
                AddQuantity = 3,

                MinVelocity = new Vec3f(-1f, 0, -1f),
                AddVelocity = new Vec3f(2f, 0.5f, 2f),

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

            WoodParticles.MinPos = minpos;
            WoodParticles.AddPos = addpos;
        }
        private void SetParticleColour(int colour)
        {
            WoodParticles.Color = colour;
        }
    }
}
