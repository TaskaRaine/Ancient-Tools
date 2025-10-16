using AncientTools.Items;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace AncientTools.BlockBehaviors
{
    public class BlockBehaviorCarveLogBarrel : BlockBehavior
    {
        public WorldInteraction[] CarveLogInteractions { get; set; } = null;

        private double CarvingTime { get; set; } = 1.0;

        private SimpleParticleProperties WoodParticles { get; set; }

        public BlockBehaviorCarveLogBarrel(Block block) : base(block)
        {
            
        }
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            List<ItemStack> adze = new List<ItemStack>();

            foreach(Item item in api.World.Items)
            {
                if(item.Attributes == null) continue;

                if (item.Attributes["carvingTimeModifier"].Exists)
                    adze.Add(new ItemStack(item));
            }

            WorldInteraction carveInteraction = new WorldInteraction()
            {
                ActionLangCode = "ancienttools:blockhelp-carve-logbarrel",
                MouseButton = EnumMouseButton.Right,
                HotKeyCode = "sprint",
                Itemstacks = adze.ToArray()
            };

            CarveLogInteractions = ObjectCacheUtil.GetOrCreate(api, "carveLogInteractions", () =>
            {
                return new WorldInteraction[]
                {
                    carveInteraction
                };
            });

            CarvingTime = api.World.Config.GetDouble("BasePrimitiveBarrelCarvingSpeed", CarvingTime);

            WoodParticles = InitializeWoodParticles();
        }
        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer, ref EnumHandling handling)
        {
            return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer, ref handling).Append(CarveLogInteractions);
        }
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            if (blockSel == null || !byPlayer.Entity.Controls.Sprint || byPlayer.InventoryManager.ActiveHotbarSlot.Empty)
                return false;

            ItemStack interactedStack = byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack;

            if (interactedStack  == null|| interactedStack.Collectible.Attributes == null || block.Attributes == null || !interactedStack.Collectible.Attributes["carvingTimeModifier"].Exists || !block.Attributes["primitiveBarrelProps"].Exists)
                return false;

            CarvingTime = world.Config.GetDouble("BaseBarkStrippingSpeed", 1.0) * interactedStack.Collectible.Attributes["carvingTimeModifier"].AsDouble();

            byPlayer.Entity.StartAnimation("adzestrip");
            world.PlaySoundAt(new AssetLocation("ancienttools", "sounds/block/stripwood"), blockSel.Position, 0, byPlayer, true, 32f, 0.75f);

            handling = EnumHandling.PreventSubsequent;
            return true;
        }
        public override bool OnBlockInteractStep(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            if (blockSel == null || !byPlayer.Entity.Controls.Sprint || byPlayer.InventoryManager.ActiveHotbarSlot.Empty)
                return false;

            ItemStack interactedStack = byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack;

            if (interactedStack == null || interactedStack.Collectible.Attributes == null || !interactedStack.Collectible.Attributes["carvingTimeModifier"].Exists)
                return false;

            if (world.Side == EnumAppSide.Client)
            {
                SetParticleColourPosition(block.GetRandomColor((ICoreClientAPI)world.Api, blockSel.Position, BlockFacing.NORTH), blockSel.Position.ToVec3d() + new Vec3d(0, 1, 0));
                world.SpawnParticles(WoodParticles);
            }

            if (secondsUsed >= CarvingTime)
            {
                byPlayer.Entity.StopAnimation("adzestrip");

                handling = EnumHandling.Handled;
                return false;
            }

            handling = EnumHandling.Handled;
            return true;
        }
        public override void OnBlockInteractStop(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            if (block.Attributes == null || !block.Attributes["primitiveBarrelProps"].Exists || !byPlayer.Entity.Controls.Sprint)
            {
                handling = EnumHandling.PassThrough;
                return;
            }
            if (secondsUsed >= CarvingTime)
            {
                Block carvedLog = world.GetBlock(new AssetLocation(block.Attributes["primitiveBarrelProps"]["nextStage"].ToString()));

                world.BlockAccessor.SetBlock(carvedLog.Id, blockSel.Position);
                world.BlockAccessor.MarkBlockDirty(blockSel.Position);

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

                MinQuantity = 2,
                AddQuantity = 3,

                MinVelocity = new Vec3f(-.25f, 1, -.25f),
                AddVelocity = new Vec3f(.5f, 2f, .5f),

                LifeLength = 0.5f,
                addLifeLength = 0.3f,

                GravityEffect = 0.6f,
                Bounciness = 0.5f,
                ParticleModel = EnumParticleModel.Cube
            };
        }
        private void SetParticleColourPosition(int colour, Vec3d minpos)
        {
            SetParticleColour(colour);

            switch(block.Code.EndVariant())
            {
                case "1":
                    minpos.Y = minpos.Y - 0.2;
                    break;
                case "2":
                    minpos.Y = minpos.Y - 0.4;
                    break;
                case "3":
                    minpos.Y = minpos.Y - 0.6;
                    break;
                default:
                    minpos.Y = minpos.Y - 0.0;
                    break;
            }

            WoodParticles.MinPos = minpos;
            WoodParticles.AddPos = new Vec3d(1, 0, 1);
        }
        private void SetParticleColour(int colour)
        {
            WoodParticles.Color = colour;
        }
    }
}
