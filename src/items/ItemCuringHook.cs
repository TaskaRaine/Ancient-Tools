
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace AncientTools.Items
{
    class ItemCuringHook: Item
    {
        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            dsc.Append("\n" + Lang.Get("ancienttools:iteminfo-hook"));
        }
    }
}
