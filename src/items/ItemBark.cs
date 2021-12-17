using Vintagestory.API.Common;

namespace AncientTools.Items
{
    class ItemBark: Item
    {
        public override void OnCreatedByCrafting(ItemSlot[] allInputslots, ItemSlot outputSlot, GridRecipe byRecipe)
        {
            outputSlot.Itemstack.StackSize = api.World.Config.GetInt("BarkPerLog", 4);

            base.OnCreatedByCrafting(allInputslots, outputSlot, byRecipe);
        }
    }
}
