using AncientTools.BlockEntity;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace AncientTools.Blocks
{
    class BlockSalveContainer: Block
    {
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
                    infoString.AppendLine(Lang.Get("ancienttools:blockinfo-salve-empty-oil", salveContainer.LiquidSlot.StackSize, salveContainer.LiquidSlot.MaxSlotStackSize));
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
