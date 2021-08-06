using System;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;

namespace AncientTools.Items
{
    class ItemSaltedMeat: Item
    {
        public override void OnCreatedByCrafting(ItemSlot[] allInputslots, ItemSlot outputSlot, GridRecipe byRecipe)
        {
            base.OnCreatedByCrafting(allInputslots, outputSlot, byRecipe);

            string inputCode;
            
            //-- Ensures that the meat perish and curing values are accurately set so that the player has a sense of how much time remains for both when crafted --//
            foreach(ItemSlot slot in allInputslots)
            {
                if(!slot.Empty)
                {
                    inputCode = slot.Itemstack.Collectible.Code.Domain + ":" + slot.Itemstack.Collectible.Code.Path;

                    if (inputCode == "game:bushmeat-raw" || inputCode == "game:redmeat-raw" || inputCode == "game:poultry-raw")
                    {
                        if(slot.Itemstack.Attributes.HasAttribute("transitionstate"))
                        {
                            TreeAttribute modifiedAttributes = slot.Itemstack.Attributes.GetTreeAttribute("transitionstate").Clone() as TreeAttribute; ;

                            TransitionState transitionState = outputSlot.Itemstack.Collectible.UpdateAndGetTransitionState(api.World, slot, EnumTransitionType.Perish);

                            FloatArrayAttribute freshHours = new FloatArrayAttribute(new float[] { transitionState.Props.FreshHours.avg });
                            FloatArrayAttribute transitionHours = new FloatArrayAttribute(new float[] { transitionState.Props.TransitionHours.avg });

                            modifiedAttributes["freshHours"] = freshHours;
                            modifiedAttributes["transitionHours"] = transitionHours;

                            outputSlot.Itemstack.Attributes["transitionstate"] = modifiedAttributes;

                            outputSlot.Itemstack.Attributes.SetDouble("curinghoursremaining", outputSlot.Itemstack.ItemAttributes["curinghoursremaining"].AsDouble());
                        }
                    }
                }
            }
        }
        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            if (LastCodePart() != "raw")
                return;

            if (inSlot.Itemstack.Attributes.HasAttribute("curinghoursremaining"))
            {
                dsc.AppendLine(Lang.Get("ancienttools:itemdesc-saltedmeat-cure-x-days", Math.Ceiling(inSlot.Itemstack.Attributes.GetDouble("curinghoursremaining") / 24)));
            }
            else
            {
                dsc.AppendLine(Lang.Get("ancienttools:itemdesc-saltedmeat-cure-x-days", Math.Ceiling(inSlot.Itemstack.Item.Attributes["curinghoursremaining"].AsDouble() / 24)));
            }
        }
    }
}