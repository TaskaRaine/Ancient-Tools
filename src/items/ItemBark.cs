using Vintagestory.API.Common;

namespace AncientTools.Items
{
    class ItemBark: Item
    {
        public override void OnCreatedByCrafting(ItemSlot[] allInputslots, ItemSlot outputSlot, GridRecipe byRecipe)
        {
            int logCount = 1;

            foreach(ItemSlot slot in allInputslots)
            {
                if (slot.Empty)
                    continue;

                if (slot.Itemstack.Collectible.FirstCodePart() == "log")
                    logCount = slot.Itemstack.StackSize;
            }

            outputSlot.Itemstack.StackSize = api.World.Config.GetInt("BarkPerLog", 4) * logCount;

            base.OnCreatedByCrafting(allInputslots, outputSlot, byRecipe);
        }
    }
}
