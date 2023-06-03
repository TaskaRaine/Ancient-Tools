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
        List<ItemStack> healingBarks = new List<ItemStack>();
        List<ItemStack> salveOils = new List<ItemStack>();
        List<ItemStack> salveThickeners = new List<ItemStack>();

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            foreach (CollectibleObject collectible in api.World.Collectibles)
            {
                if(collectible.Attributes != null)
                {
                    if(collectible.Attributes["salveProperties"].Exists)
                        if (collectible.Attributes["salveProperties"]["isMedicinalBark"].Exists)
                        { 
                            healingBarks.Add(new ItemStack(collectible));
                        }
                        if (collectible.Attributes["salveProperties"]["isSalveOil"].Exists)
                        {
                            salveOils.Add(new ItemStack(collectible));
                        }
                        if (collectible.Attributes["salveProperties"]["isSalveThickener"].Exists)
                        { 
                            salveThickeners.Add(new ItemStack(collectible));
                        }
                }
            }
        }
        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            if (api.World.BlockAccessor.GetBlockEntity(selection.Position) is BESalveContainer salveContainer)
            {
                return GenerateInteractions(salveContainer.ResourceSlot, salveContainer.LiquidSlot);
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
                else if (!salveContainer.ResourceSlot.Empty || salveContainer.LiquidSlot.Itemstack.Collectible.Attributes["salveProperties"]["isSalveOil"].Exists)
                {
                    infoString.AppendLine(Lang.Get("ancienttools:blockinfo-salve-empty-partial"));
                    infoString.AppendLine(Lang.Get("ancienttools:blockinfo-salve-empty-bark", salveContainer.ResourceSlot.StackSize, salveContainer.ResourceSlot.MaxSlotStackSize));
                    infoString.AppendLine(Lang.Get("ancienttools:blockinfo-salve-empty-fat", salveContainer.LiquidSlot.StackSize, salveContainer.LiquidSlot.MaxSlotStackSize));
                }
                else if (salveContainer.LiquidSlot.Itemstack.Collectible.Attributes["salveProperties"]["isSalveThickener"].Exists)
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
        private WorldInteraction GetBarkInteractions()
        {
            WorldInteraction emptyBarkInteraction = new WorldInteraction
            {
                ActionLangCode = "ancienttools:blockhelp-insert-healingbark",
                MouseButton = EnumMouseButton.Right,
                Itemstacks = healingBarks.ToArray()
            };

            return emptyBarkInteraction;
        }
        private WorldInteraction GetBarkInteractions(ItemSlot barkSlot)
        {
            if (barkSlot.Empty)
                return GetBarkInteractions();

            if (barkSlot.Itemstack?.Collectible?.Attributes["salveProperties"]?["isMedicinalBark"].AsBool() == true)
            {
                ItemStack[] displayItemstack = new ItemStack[] { barkSlot.Itemstack.Clone() };
                displayItemstack[0].StackSize = barkSlot.MaxSlotStackSize - displayItemstack[0].StackSize;

                return new WorldInteraction
                {
                    ActionLangCode = barkSlot.Itemstack?.Collectible?.Attributes["salveProperties"]?["langCode"]?.ToString(),
                    MouseButton = EnumMouseButton.Right,
                    Itemstacks = displayItemstack
                };
            }
            else
            {
                string langCode = "ancienttools:blockhelp-salve-remove-incompatiblematerial";

                if (barkSlot.Itemstack?.Collectible?.Attributes["salveProperties"]?["langCode"].Exists == true)
                    langCode = barkSlot.Itemstack?.Collectible?.Attributes["salveProperties"]?["langCode"]?.ToString();

                return new WorldInteraction
                {
                    ActionLangCode = langCode,
                    MouseButton = EnumMouseButton.Right,
                    RequireFreeHand = true
                };
            }
        }
        private WorldInteraction GetOilInteractions()
        {
            WorldInteraction emptyOilInteraction = new WorldInteraction
            {
                ActionLangCode = "ancienttools:blockhelp-insert-oil",
                MouseButton = EnumMouseButton.Right,
                Itemstacks = salveOils.ToArray()
            };

            return emptyOilInteraction;
        }
        private WorldInteraction GetOilInteractions(ItemSlot oilSlot)
        {
            if (oilSlot.Empty)
                return GetOilInteractions();

            ItemStack[] displayItemstack = new ItemStack[] { oilSlot.Itemstack.Clone() };
            displayItemstack[0].StackSize = oilSlot.MaxSlotStackSize - displayItemstack[0].StackSize;

            return new WorldInteraction
            {
                ActionLangCode = oilSlot.Itemstack?.Collectible?.Attributes["salveProperties"]?["langCode"]?.ToString(),
                MouseButton = EnumMouseButton.Right,
                Itemstacks = displayItemstack
            };
        }
        private WorldInteraction GetThickenerInteractions()
        {
            WorldInteraction emptyThickenerInteraction = new WorldInteraction
            {
                ActionLangCode = "ancienttools:blockhelp-insert-thickener",
                MouseButton = EnumMouseButton.Right,
                Itemstacks = salveThickeners.ToArray()
            };

            return emptyThickenerInteraction;
        }
        private WorldInteraction GetThickenerInteractions(ItemSlot thickenerSlot)
        {
            if (thickenerSlot.Empty)
                return GetOilInteractions();

            ItemStack[] displayItemstack = new ItemStack[] { thickenerSlot.Itemstack.Clone() };
            displayItemstack[0].StackSize = thickenerSlot.MaxSlotStackSize - displayItemstack[0].StackSize;

            return new WorldInteraction
            {
                ActionLangCode = thickenerSlot.Itemstack?.Collectible?.Attributes["salveProperties"]?["langCode"]?.ToString(),
                MouseButton = EnumMouseButton.Right,
                Itemstacks = displayItemstack
            };
        }
        private WorldInteraction[] GenerateInteractions(ItemSlot resourceSlot, ItemSlot liquidSlot)
        {
            List<WorldInteraction> generatedInteraction = new List<WorldInteraction>();

            if (resourceSlot.Empty && liquidSlot.Empty)
            {
                generatedInteraction.Add(GetBarkInteractions());
                generatedInteraction.Add(GetOilInteractions());
                generatedInteraction.Add(GetThickenerInteractions());
            }
            else if (!resourceSlot.Empty)
            { 
                if (resourceSlot.StackSize < resourceSlot.MaxSlotStackSize)
                    generatedInteraction.Add(GetBarkInteractions(resourceSlot));
                if (liquidSlot.StackSize < liquidSlot.MaxSlotStackSize) 
                    generatedInteraction.Add(GetOilInteractions(liquidSlot));
            }
            else if (!liquidSlot.Empty)
            {
                if (liquidSlot.StackSize < liquidSlot.MaxSlotStackSize)
                {
                    if (liquidSlot.Itemstack.Collectible.Attributes["salveProperties"]["isSalveThickener"].Exists)
                        generatedInteraction.Add(GetThickenerInteractions(liquidSlot));
                    if (liquidSlot.Itemstack.Collectible.Attributes["salveProperties"]["isSalveOil"].Exists)
                    {
                        if (resourceSlot.StackSize < resourceSlot.MaxSlotStackSize)
                            generatedInteraction.Add(GetBarkInteractions(resourceSlot));

                        generatedInteraction.Add(GetOilInteractions(liquidSlot));
                    }
                }
            }

            return generatedInteraction.ToArray();
        }
    }
}
