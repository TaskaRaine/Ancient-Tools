using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace AncientTools.CollectibleBehaviors
{
    class CollectibleBehaviorAttemptRepairClutter : CollectibleBehavior
    {
        private float RepairTime { get; set; } = 1.0f;

        private SimpleParticleProperties DripParticles { get; set; }
        private BlockPos InteractedBlockPosition { get; set; } = null;
        private WorldInteraction[] PitchRepairInteractions { get; set; } = null;


        public CollectibleBehaviorAttemptRepairClutter(CollectibleObject collObj) : base(collObj)
        {

        }
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            if(api.Side == EnumAppSide.Client)
            {
                InitializeDripParticles();
            }

            List<ItemStack> meltPitchForRepairCollectibles = new List<ItemStack>();

            foreach (CollectibleObject coll in api.World.Collectibles)
            {
                if (coll.Attributes == null)
                    continue;

                if (coll.Attributes["canMeltPitchForRepairByType"]?.AsBool() == true)
                {
                    meltPitchForRepairCollectibles.Add(new ItemStack(coll));
                }
            }

            WorldInteraction pitchRepairInteraction = new WorldInteraction
            {
                ActionLangCode = "ancienttools:itemhelp-pitchrepair",
                Itemstacks = meltPitchForRepairCollectibles.ToArray(),
                MouseButton = EnumMouseButton.Right,
                HotKeyCode = "shift"
            };

            PitchRepairInteractions = ObjectCacheUtil.GetOrCreate(api, "pitchRepairInteraction", () =>
            {
                return new WorldInteraction[]
                {
                    pitchRepairInteraction
                };
            });
        }
        public override void Initialize(JsonObject properties)
        {
            base.Initialize(properties);

            RepairTime = properties["repairTime"].AsFloat(RepairTime);
        }
        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot, ref EnumHandling handling)
        {
            return base.GetHeldInteractionHelp(inSlot, ref handling).Append(PitchRepairInteractions);
        }
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
        {
            if (blockSel == null || byEntity.LeftHandItemSlot == null)
                return;

            EntityPlayer player = (EntityPlayer)byEntity;

            if (byEntity.World.Claims.TryAccess(player.Player, blockSel.Position, EnumBlockAccessFlags.BuildOrBreak))
            {
                if (byEntity.LeftHandItemSlot.Itemstack?.Collectible.Attributes?["canMeltPitchForRepair"].AsBool() == true)
                {
                    Block interactedBlock = byEntity.World.BlockAccessor.GetBlock(blockSel.Position);
                    InteractedBlockPosition = blockSel.Position;

                    if (interactedBlock.HasBehavior<BlockBehaviorReparable>())
                    {
                        handling = EnumHandling.PreventDefault;
                        handHandling = EnumHandHandling.PreventDefault;

                        if(player.World.Side == EnumAppSide.Client)
                        {
                            player.StartAnimation("meltpitchrepair");
                        }
                    }
                }
            }
        }
        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandling handling)
        {
            if (blockSel == null || blockSel.Position != InteractedBlockPosition || byEntity.LeftHandItemSlot == null)
                return false;

            if (secondsUsed < RepairTime)
            {
                if(byEntity.Api.Side == EnumAppSide.Client)
                {
                    UpdateParticlePosition(byEntity.Pos.XYZ, byEntity.Pos.GetViewVector().ToVec3d(), byEntity.LocalEyePos);
                    byEntity.World.SpawnParticles(DripParticles);
                }

                handling = EnumHandling.PreventDefault;
                return true;
            }

            handling = EnumHandling.Handled;
            return false;
        }
        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandling handling)
        {
            EntityPlayer entityPlayer = (EntityPlayer)byEntity;
            IPlayer player = entityPlayer.Player;

            if (entityPlayer.World.Side == EnumAppSide.Client)
            {
                entityPlayer.StopAnimation("meltpitchrepair");
            }

            if (blockSel == null || blockSel.Position != InteractedBlockPosition || entityPlayer.LeftHandItemSlot == null)
            {
                return;
            }

            if (secondsUsed < RepairTime)
            {
                return;
            }

            if (entityPlayer.World.Claims.TryAccess(player, blockSel.Position, EnumBlockAccessFlags.BuildOrBreak))
            {
                if (entityPlayer.LeftHandItemSlot.Itemstack?.Collectible.Attributes?["canMeltPitchForRepair"].AsBool() == true)
                {
                    Block interactedBlock = entityPlayer.World.BlockAccessor.GetBlock(blockSel.Position);

                    if (interactedBlock.HasBehavior<BlockBehaviorReparable>())
                    {
                        float itemRepairAmount = collObj.Attributes["repairPercentage"].AsFloat(0.2f);

                        if (itemRepairAmount > 0f)
                        {
                            EnumClutterDropRule rule = GetRule(entityPlayer.World);

                            if (rule == EnumClutterDropRule.Reparable)
                            {
                                BEBehaviorShapeFromAttributes clutterShapeBehavior = interactedBlock.GetBEBehavior<BEBehaviorShapeFromAttributes>(blockSel.Position);

                                if (clutterShapeBehavior != null)
                                {
                                    if (clutterShapeBehavior.repairState < 1f && clutterShapeBehavior.reparability > 1)
                                    {
                                        clutterShapeBehavior.repairState += itemRepairAmount * 5f / (float)(clutterShapeBehavior.reparability - 1);
                                        slot.TakeOut(1);

                                        if (entityPlayer.World.Side == EnumAppSide.Client)
                                        {
                                            BlockPos position = blockSel.Position;
                                            AssetLocation location = AssetLocation.Create("game:sounds/player/gluerepair");
                                            entityPlayer.World.PlaySoundAt(location, (float)position.X + 0.5f, (float)position.Y + 0.5f, (float)position.Z + 0.5f, player, randomizePitch: true, 8f);
                                        }

                                        SendPlayerMessage(player as IServerPlayer, Lang.Get("game:clutter-repaired", interactedBlock.GetPlacedBlockName(entityPlayer.World, blockSel.Position)));
                                    }
                                    else
                                    {
                                        SendPlayerMessage(player as IServerPlayer, Lang.Get("game:clutter-fullyrepaired"));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        protected EnumClutterDropRule GetRule(IWorldAccessor world)
        {
            string text = world.Config.GetString("clutterObtainable", "ifrepaired").ToLowerInvariant();
            if (text == "yes")
            {
                return EnumClutterDropRule.AlwaysObtain;
            }

            if (text == "no")
            {
                return EnumClutterDropRule.NeverObtain;
            }

            return EnumClutterDropRule.Reparable;
        }
        private void InitializeDripParticles()
        {
            DripParticles = new SimpleParticleProperties()
            {
                MinSize = 0.10f,
                MaxSize = 0.45f,

                LifeLength = 0.2f,
                addLifeLength = 1,

                MinQuantity = 0.02f,
                AddQuantity = 0.1f,
                WithTerrainCollision = true,

                Color = ColorUtil.ToRgba(255, 10, 10, 10),
                //AddPos = new Vec3d(0.2, 0.0, 0.2),

                ParticleModel = EnumParticleModel.Cube,
                GravityEffect = 0.1f,
            };
        }
        private void UpdateParticlePosition(Vec3d pos, Vec3d viewVector, Vec3d eyePos)
        {
            DripParticles.MinPos = new Vec3d((pos.X) + (viewVector.X / 6), pos.Y + 1.15, (pos.Z) + (viewVector.Z / 6));
            DripParticles.AddPos = new Vec3d(0.1 * viewVector.Z, 0, -0.1 * viewVector.X);
        }
        private void SendPlayerMessage(IServerPlayer player, string message)
        {
            player?.SendIngameError(message, message);
        }
    }
}
