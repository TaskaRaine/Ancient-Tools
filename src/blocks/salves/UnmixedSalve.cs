﻿using AncientTools.BlockEntities;
using System;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace AncientTools.Blocks
{
    class BlockUnmixedSalve: Block
    {
        public WorldInteraction[] interactions = null;
        public SimpleParticleProperties salveParticles;

        private double salveMixTime;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            interactions = ObjectCacheUtil.GetOrCreate(api, "unfinishedSalveInteractions", () =>
            {
                if (this.LastCodePart() == "barkoil")
                {
                    ItemStack[] waxStack = { new ItemStack(api.World.BlockAccessor.GetBlock(new AssetLocation("ancienttools", "salvepot-softwax"))) };

                    return new WorldInteraction[]
                    {
                        new WorldInteraction()
                        {
                            ActionLangCode = "ancienttools:blockhelp-pour-wax",
                            HotKeyCode = "shift",
                            MouseButton = EnumMouseButton.Right,
                            Itemstacks = waxStack
                        },
                    };
                }
                else if (this.LastCodePart() == "softwax")
                {
                    ItemStack[] barkoilStack = { new ItemStack(api.World.BlockAccessor.GetBlock(new AssetLocation("ancienttools", "salvepot-barkoil"))) };

                    return new WorldInteraction[]
                    {
                        new WorldInteraction()
                        {
                            ActionLangCode = "ancienttools:blockhelp-pour-barkoil",
                            HotKeyCode = "shift",
                            MouseButton = EnumMouseButton.Right,
                            Itemstacks = barkoilStack
                        },
                    };
                }

                return null;
            });

            salveParticles = new SimpleParticleProperties
            {
                MinSize = 0.2f,
                MaxSize = 0.4f,

                LifeLength = 0.2f,

                MinQuantity = 1,
                AddQuantity = 1,
                WithTerrainCollision = false,

                AddPos = new Vec3d(0.2, 0.2, 0.2),

                ParticleModel = EnumParticleModel.Cube,
                GravityEffect = 0.2f,
            };

            if(this.LastCodePart() == "barkoil")
            {
                salveParticles.Color = ColorUtil.ToRgba(100, 211, 93, 0);
            }
            else
            {
                salveParticles.Color = ColorUtil.ToRgba(100, 194, 135, 20);
            }

            salveMixTime = api.World.Config.GetDouble("SalveMixTime", 1.5);
        }
        public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
        {
            StringBuilder infoString = new StringBuilder();
            infoString.Append("\n");

            if (this.LastCodePart() == "softwax")
            {
                infoString.AppendLine(Lang.Get("ancienttools:blockinfo-salve-mixwithoil"));
                return infoString.ToString();
            }
            else if(this.LastCodePart() == "barkoil")
            {
                infoString.AppendLine(Lang.Get("ancienttools:blockinfo-salve-mixwithwax"));
                return infoString.ToString();
            }

            return base.GetPlacedBlockInfo(world, pos, forPlayer);
        }
        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            return interactions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
        }
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            if (blockSel == null)
                return;

            if(byEntity.Controls.ShiftKey && 
                api.World.BlockAccessor.GetBlock(blockSel.Position, BlockLayersAccess.SolidBlocks) is BlockUnmixedSalve interactedBlock && 
                interactedBlock.LastCodePart() != this.LastCodePart())
            {
                byEntity.StartAnimation("dumpsalve");

                if(api.Side == EnumAppSide.Client)
                    api.World.PlaySoundAt(new AssetLocation("ancienttools", "sounds/block/salvepour"), byEntity, byEntity as IPlayer, true, 32f, 0.5f);
                
                handling = EnumHandHandling.PreventDefault;
            }

            if (handling == EnumHandHandling.NotHandled)
            {
                base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
            }
        }
        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            if (secondsUsed >= salveMixTime)
            {
                return false;
            }

            if (byEntity.World is IClientWorldAccessor)
            {
                IPlayer byPlayer = null;
                if (byEntity is EntityPlayer) byPlayer = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);

                Vec3d pos = blockSel.Position.ToVec3d();
                pos.X += 0.5;
                pos.Z += 0.5;
                pos.Y += 0.5;

                salveParticles.MinPos = pos.AddCopy(-0.10, -0.10, -0.10);

                byEntity.World.SpawnParticles(salveParticles, byPlayer);
            }

            return true;
        }
        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            byEntity.StopAnimation("dumpsalve");

            if(secondsUsed >= salveMixTime)
            if (api.World.BlockAccessor.GetBlock(blockSel.Position, BlockLayersAccess.SolidBlocks) is BlockUnmixedSalve interactedBlock)
            {
                if (interactedBlock.Id == this.Id)
                    return;

                slot.TakeOut(1);
                byEntity.TryGiveItemStack(new ItemStack(api.World.GetBlock(new AssetLocation("ancienttools", "salvepot-residuecovered"))));
                api.World.BlockAccessor.SetBlock(api.World.GetBlock(new AssetLocation("ancienttools", "salvepot-finishedsalve")).Id, blockSel.Position);
                api.World.BlockAccessor.MarkBlockDirty(blockSel.Position);


                if (api.World.BlockAccessor.GetBlockEntity(blockSel.Position) is BEFinishedSalve finishedEntity)
                {
                    finishedEntity.InsertSalvePortions();
                }
            }
        }
    }
}
