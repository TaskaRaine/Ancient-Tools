﻿using AncientTools.BlockEntities;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace AncientTools.Blocks
{
    class BlockPitchContainer: Block
    {
        WorldInteraction[] emptyInteractions = null;
        WorldInteraction[] charcoalAndResinInteractions = null;
        WorldInteraction[] charcoalAndGrassInteractions = null;
        WorldInteraction[] resinAndGrassInteractions = null;
        WorldInteraction[] charcoalInteractions = null;
        WorldInteraction[] resinInteractions = null;
        WorldInteraction[] grassInteractions = null;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            List<ItemStack> powderedCharcoalList = new List<ItemStack>();
            List<ItemStack> resinList = new List<ItemStack>();
            List<ItemStack> grassList = new List<ItemStack>();

            foreach(Item item in api.World.Items)
            {
                if (item.Attributes == null)
                    continue;

                if (item.Attributes["isPitchCharcoal"].Exists)
                    powderedCharcoalList.Add(new ItemStack(item));
                else if (item.Attributes["isPitchResin"].Exists)
                    resinList.Add(new ItemStack(item));
                else if (item.Attributes["isPitchGrass"].Exists)
                    grassList.Add(new ItemStack(item));
            }

            WorldInteraction pickupInteraction = new WorldInteraction()
            {
                ActionLangCode = "game:blockhelp-behavior-rightclickpickup",
                RequireFreeHand = true,
                MouseButton = EnumMouseButton.Right
            };
            WorldInteraction charcoalInteraction = new WorldInteraction()
            {
                ActionLangCode = "ancienttools:blockhelp-insert-powderedcharcoal",
                MouseButton = EnumMouseButton.Right,
                Itemstacks = powderedCharcoalList.ToArray()
            };
            WorldInteraction resinInteraction = new WorldInteraction()
            {
                ActionLangCode = "ancienttools:blockhelp-insert-resin",
                MouseButton = EnumMouseButton.Right,
                Itemstacks = resinList.ToArray()
            };
            WorldInteraction grassInteraction = new WorldInteraction()
            {
                ActionLangCode = "ancienttools:blockhelp-insert-grass",
                MouseButton = EnumMouseButton.Right,
                Itemstacks = grassList.ToArray()
            };

            emptyInteractions = ObjectCacheUtil.GetOrCreate(api, "emptyPitchContainerInteractions", () =>
            {
                return new WorldInteraction[]
                {
                    pickupInteraction,
                    charcoalInteraction,
                    resinInteraction,
                    grassInteraction
                };
            });

            charcoalAndResinInteractions = ObjectCacheUtil.GetOrCreate(api, "charcoalAndResinContainerInteractions", () =>
            {
                return new WorldInteraction[]
                {
                    charcoalInteraction,
                    resinInteraction
                };
            });

            charcoalAndGrassInteractions = ObjectCacheUtil.GetOrCreate(api, "charcoalAndGrassContainerInteractions", () =>
            {
                return new WorldInteraction[]
                {
                    charcoalInteraction,
                    grassInteraction
                };
            });

            resinAndGrassInteractions = ObjectCacheUtil.GetOrCreate(api, "grassAndResinInteractions", () =>
            {
                return new WorldInteraction[]
                {
                    resinInteraction,
                    grassInteraction
                };
            });

            charcoalInteractions = ObjectCacheUtil.GetOrCreate(api, "charcoalInteractions", () =>
            {
                return new WorldInteraction[]
                {
                    charcoalInteraction
                };
            });

            resinInteractions = ObjectCacheUtil.GetOrCreate(api, "resinInteractions", () =>
            {
                return new WorldInteraction[]
                {
                    resinInteraction
                };
            });

            grassInteractions = ObjectCacheUtil.GetOrCreate(api, "grassInteraction", () =>
            {
                return new WorldInteraction[]
                {
                    grassInteraction
                };
            });
        }
        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            if (api.World.BlockAccessor.GetBlockEntity(selection.Position) is BEPitchContainer pitchContainer)
            {
                if(!pitchContainer.CharcoalSlot.Empty)
                {
                    if(pitchContainer.CharcoalSlot.StackSize == pitchContainer.CharcoalSlot.MaxSlotStackSize)
                    {
                        if (pitchContainer.ResinSlot.StackSize == pitchContainer.ResinSlot.MaxSlotStackSize && pitchContainer.GrassSlot.StackSize == pitchContainer.GrassSlot.MaxSlotStackSize)
                            return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer);
                        else if (pitchContainer.ResinSlot.StackSize == pitchContainer.ResinSlot.MaxSlotStackSize)
                            return grassInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
                        else if (pitchContainer.GrassSlot.StackSize == pitchContainer.GrassSlot.MaxSlotStackSize)
                            return resinInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));

                        return resinAndGrassInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
                    }
                }
                else if(!pitchContainer.ResinSlot.Empty)
                {
                    if(pitchContainer.ResinSlot.StackSize == pitchContainer.ResinSlot.MaxSlotStackSize)
                    {
                        if (pitchContainer.CharcoalSlot.StackSize == pitchContainer.CharcoalSlot.MaxSlotStackSize && pitchContainer.GrassSlot.StackSize == pitchContainer.GrassSlot.MaxSlotStackSize)
                            return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer);
                        else if (pitchContainer.CharcoalSlot.StackSize == pitchContainer.CharcoalSlot.MaxSlotStackSize)
                            return grassInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
                        else if (pitchContainer.GrassSlot.StackSize == pitchContainer.GrassSlot.MaxSlotStackSize)
                            return charcoalInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));

                        return charcoalAndGrassInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
                    }
                }
                else if(!pitchContainer.GrassSlot.Empty)
                {
                    if(pitchContainer.GrassSlot.StackSize == pitchContainer.GrassSlot.MaxSlotStackSize)
                    {
                        if (pitchContainer.CharcoalSlot.StackSize == pitchContainer.CharcoalSlot.MaxSlotStackSize && pitchContainer.ResinSlot.StackSize == pitchContainer.ResinSlot.MaxSlotStackSize)
                            return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer);
                        else if (pitchContainer.CharcoalSlot.StackSize == pitchContainer.CharcoalSlot.MaxSlotStackSize)
                            return resinInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
                        else if (pitchContainer.ResinSlot.StackSize == pitchContainer.ResinSlot.MaxSlotStackSize)
                            return grassInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));

                        return charcoalAndResinInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
                    }
                }

                return emptyInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
            }
            return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer);
        }
        public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
        {
            StringBuilder infoString = new StringBuilder();
            infoString.Append("\n");

            if (api.World.BlockAccessor.GetBlockEntity(pos) is BEPitchContainer pitchContainer)
            {
                if(pitchContainer.CharcoalSlot.Empty && pitchContainer.ResinSlot.Empty && pitchContainer.GrassSlot.Empty)
                {
                    infoString.AppendLine(Lang.Get("ancienttools:blockinfo-pitch-empty"));
                    infoString.AppendLine(Lang.Get("ancienttools:blockinfo-pitch-insert"));
                }
                else
                {
                    infoString.AppendLine(Lang.Get("ancienttools:blockinfo-contains"));
                    infoString.AppendLine(Lang.Get("ancienttools:blockinfo-pitch-empty-charcoal", pitchContainer.CharcoalSlot.StackSize, pitchContainer.CharcoalSlot.MaxSlotStackSize));
                    infoString.AppendLine(Lang.Get("ancienttools:blockinfo-pitch-empty-resin", pitchContainer.ResinSlot.StackSize, pitchContainer.ResinSlot.MaxSlotStackSize));
                    infoString.AppendLine(Lang.Get("ancienttools:blockinfo-pitch-empty-grass", pitchContainer.GrassSlot.StackSize, pitchContainer.GrassSlot.MaxSlotStackSize));
                }

                return infoString.ToString();
            }
            else
            {
                infoString.AppendLine(Lang.Get("ancienttools:blockinfo-curingrack-no-entity"));
            }

            return infoString.ToString();
        }
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEPitchContainer pitchEntity)
            {
                if(pitchEntity.Inventory.Empty)
                {
                    if (byPlayer.InventoryManager.ActiveHotbarSlot.Empty)
                    {
                        ReturnContainer(byPlayer, blockSel.Position);
                        return true;
                    }
                }

                pitchEntity.OnInteract(byPlayer);
                pitchEntity.ConvertIfComplete();

                return true;
            }

            return false;
        }
        private void ReturnContainer(IPlayer byPlayer, BlockPos blockPos)
        {
            byPlayer.Entity.TryGiveItemStack(new ItemStack(api.World.GetBlock(this.Id)));
            api.World.BlockAccessor.SetBlock(0, blockPos);

            api.World.BlockAccessor.MarkBlockDirty(blockPos, byPlayer);
        }
    }
}
