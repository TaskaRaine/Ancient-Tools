using AncientTools.BlockEntities;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace AncientTools.Blocks
{
    class BlockSalveContainer: Block
    {
        WorldInteraction[] emptyInteractions = null;
        WorldInteraction[] barkAndOilInteractions = null;
        WorldInteraction[] birchBarkAndOilInteractions = null;
        WorldInteraction[] pineBarkAndOilInteractions = null;
        WorldInteraction[] barkFilledInteractions = null;
        WorldInteraction[] oilFilledInteractions = null;
        WorldInteraction[] oilFilledBirchInteractions = null;
        WorldInteraction[] oilFilledPineInteractions = null;
        WorldInteraction[] thickenerInteractions = null;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            ItemStack[] healingBarks = { 
                new ItemStack(api.World.GetItem(new AssetLocation("ancienttools", "bark-birch"))),
                new ItemStack(api.World.GetItem(new AssetLocation("ancienttools", "bark-pine")))
            };

            ItemStack[] birchBark = {
                new ItemStack(api.World.GetItem(new AssetLocation("ancienttools", "bark-birch")))
            };

            ItemStack[] pineBark = {
                new ItemStack(api.World.GetItem(new AssetLocation("ancienttools", "bark-pine")))
            };

            ItemStack[] oilIngredients = {
                new ItemStack(api.World.GetItem(new AssetLocation("game", "fat")))
            };

            ItemStack[] thickenerIngredients = {
                new ItemStack(api.World.GetItem(new AssetLocation("game", "beeswax")))
            };

            WorldInteraction healingBarkInteraction = new WorldInteraction()
            {
                ActionLangCode = "ancienttools:blockhelp-insert-healingbark",
                MouseButton = EnumMouseButton.Right,
                Itemstacks = healingBarks
            };

            WorldInteraction birchBarkInteraction = new WorldInteraction()
            {
                ActionLangCode = "ancienttools:blockhelp-insert-birchbark",
                MouseButton = EnumMouseButton.Right,
                Itemstacks = birchBark
            };

            WorldInteraction pineBarkInteraction = new WorldInteraction()
            {
                ActionLangCode = "ancienttools:blockhelp-insert-pinebark",
                MouseButton = EnumMouseButton.Right,
                Itemstacks = pineBark
            };

            WorldInteraction oilInteraction = new WorldInteraction()
            {
                ActionLangCode = "ancienttools:blockhelp-insert-oil",
                MouseButton = EnumMouseButton.Right,
                Itemstacks = oilIngredients
            };

            WorldInteraction thickenerInteraction = new WorldInteraction()
            {
                ActionLangCode = "ancienttools:blockhelp-insert-thickener",
                MouseButton = EnumMouseButton.Right,
                Itemstacks = thickenerIngredients
            };

            emptyInteractions = ObjectCacheUtil.GetOrCreate(api, "emptySalveContainerInteractions", () =>
            {
                return new WorldInteraction[]
                {
                    healingBarkInteraction,
                    oilInteraction,
                    thickenerInteraction
                };
            });

            barkAndOilInteractions = ObjectCacheUtil.GetOrCreate(api, "barkAndOilSalveContainerInteractions", () =>
            {
                return new WorldInteraction[]
                {
                    healingBarkInteraction,
                    oilInteraction
                };
            });

            birchBarkAndOilInteractions = ObjectCacheUtil.GetOrCreate(api, "birchBarkAndOilSalveContainerInteractions", () =>
            {
                return new WorldInteraction[]
                {
                    birchBarkInteraction,
                    oilInteraction
                };
            });

            pineBarkAndOilInteractions = ObjectCacheUtil.GetOrCreate(api, "pineBarkAndOilSalveContainerInteractions", () =>
            {
                return new WorldInteraction[]
                {
                    pineBarkInteraction,
                    oilInteraction
                };
            });

            barkFilledInteractions = ObjectCacheUtil.GetOrCreate(api, "barkFilledSalveContainerInteraction", () =>
            {
                return new WorldInteraction[]
                {
                    oilInteraction
                };
            });

            oilFilledInteractions = ObjectCacheUtil.GetOrCreate(api, "oilFilledSalveContainerInteraction", () =>
            {
                return new WorldInteraction[]
                {
                    healingBarkInteraction
                };
            });

            oilFilledBirchInteractions = ObjectCacheUtil.GetOrCreate(api, "oilFilledBirchSalveContainerInteraction", () =>
            {
                return new WorldInteraction[]
                {
                    birchBarkInteraction
                };
            });

            oilFilledPineInteractions = ObjectCacheUtil.GetOrCreate(api, "oilFilledPineSalveContainerInteraction", () =>
            {
                return new WorldInteraction[]
                {
                    pineBarkInteraction
                };
            });

            thickenerInteractions = ObjectCacheUtil.GetOrCreate(api, "thickenerSalveContainerInteraction", () =>
            {
                return new WorldInteraction[]
                {
                    thickenerInteraction
                };
            });
        }
        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            if (api.World.BlockAccessor.GetBlockEntity(selection.Position) is BESalveContainer salveContainer)
            {
                if(!salveContainer.ResourceSlot.Empty)
                {
                    if(salveContainer.ResourceSlot.StackSize == salveContainer.ResourceSlot.MaxSlotStackSize)
                    {
                        return barkFilledInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
                    }
                    else if(!salveContainer.LiquidSlot.Empty)
                    {
                        if(salveContainer.LiquidSlot.StackSize == salveContainer.LiquidSlot.MaxSlotStackSize)
                        {
                            if (salveContainer.ResourceSlot.Itemstack.Item.LastCodePart() == "birch")
                                return oilFilledBirchInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
                            else
                                return oilFilledPineInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
                        }
                    }

                    if (salveContainer.ResourceSlot.Itemstack.Item.LastCodePart() == "birch")
                        return birchBarkAndOilInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
                    else
                        return pineBarkAndOilInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
                }
                else if(!salveContainer.LiquidSlot.Empty)
                {
                    if(salveContainer.LiquidSlot.Itemstack.Item.Attributes["isSalveThickener"].AsBool() == true)
                    {
                        return thickenerInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
                    }
                    else
                    {
                        if (salveContainer.LiquidSlot.StackSize == salveContainer.LiquidSlot.MaxSlotStackSize)
                        {
                            return oilFilledInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
                        }

                        return barkAndOilInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
                    }
                }

                return emptyInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
            }

            return null;
        }
        public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
        {
            StringBuilder infoString = new StringBuilder();
            infoString.Append("\n");

            if (api.World.BlockAccessor.GetBlockEntity(pos) is BESalveContainer salveContainer)
            {
                if (salveContainer.ResourceSlot.Empty && salveContainer.LiquidSlot.Empty)
                {
                    infoString.AppendLine(Lang.Get("ancienttools:blockinfo-salve-empty-fillwithbark"));
                    infoString.AppendLine(Lang.Get("ancienttools:blockinfo-salve-empty-crouch"));
                }
                else if (!salveContainer.ResourceSlot.Empty || salveContainer.LiquidSlot.Itemstack.Collectible.Attributes["isSalveOil"].Exists)
                {
                    infoString.AppendLine(Lang.Get("ancienttools:blockinfo-salve-empty-partial"));
                    infoString.AppendLine(Lang.Get("ancienttools:blockinfo-salve-empty-bark", salveContainer.ResourceSlot.StackSize, salveContainer.ResourceSlot.MaxSlotStackSize));
                    infoString.AppendLine(Lang.Get("ancienttools:blockinfo-salve-empty-fat", salveContainer.LiquidSlot.StackSize, salveContainer.LiquidSlot.MaxSlotStackSize));
                }
                else if (salveContainer.LiquidSlot.Itemstack.Collectible.Attributes["isSalveThickener"].Exists)
                {
                    infoString.AppendLine(Lang.Get("ancienttools:blockinfo-salve-empty-partial"));
                    infoString.AppendLine(Lang.Get("ancienttools:blockinfo-salve-empty-wax", salveContainer.LiquidSlot.StackSize, salveContainer.LiquidSlot.MaxSlotStackSize));
                }

                return infoString.ToString();
            }
            else
            {
                infoString.AppendLine(Lang.Get("ancienttools:blockinfo-curingrack-no-entity"));

                return infoString.ToString();
            }
        }
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BESalveContainer salveEntity)
            {
                salveEntity.OnInteract(byPlayer);
                salveEntity.ConvertIfComplete();

                return true;
            }

            return false;
        }
    }
}
