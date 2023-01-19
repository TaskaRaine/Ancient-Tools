using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace AncientTools.Items
{
    class ItemBrain: Item
    {
        protected override void tryEatBegin(ItemSlot slot, EntityAgent byEntity, ref EnumHandHandling handling, string eatSound = "eat", int eatSoundRepeats = 1)
        {
            handling = EnumHandHandling.NotHandled;
        }
        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            /*
            int newlineCount = 0;
            int foodInfoStartIndex = 0;

            for(int i = 0; i < dsc.Length; i++)
            {
                if(dsc[i] == '\n')
                {
                    newlineCount++;

                    if (newlineCount == 3)
                        foodInfoStartIndex = i + 1;
                }
            }

            dsc.Remove(foodInfoStartIndex, dsc.Length - foodInfoStartIndex);
            dsc.Append("\n" + Lang.Get("ancienttools:iteminfo-brain"));
            dsc.Append("\n" + Lang.Get("ancienttools:iteminfo-brain-food"));
            */
        }
        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            return null;
        }
    }
}
