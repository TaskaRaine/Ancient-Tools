using AncientTools.EntityBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Util;

namespace AncientTools.Items
{
    class ItemSalvePortion: Item
    {
        WorldInteraction[] interactions = null;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            interactions = ObjectCacheUtil.GetOrCreate(api, "salveHealInteractions", () =>
            {
                return new WorldInteraction[] {
                    new WorldInteraction()
                        {
                            ActionLangCode = "ancienttools:itemhelp-heal",
                            MouseButton = EnumMouseButton.Right
                        },
                };
            });

        }
        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            dsc.Append("\n");
            dsc.AppendLine(Lang.Get("ancienttools:iteminfo-salve-heal", this.Attributes["totalhealing"], this.Attributes["healingtime"]));
        }
        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            return interactions;
        }
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            if(api.Side == EnumAppSide.Server)
                if(!byEntity.HasBehavior<EntityBehaviorSalveHeal>())
                {
                    byEntity.AddBehavior(new EntityBehaviorSalveHeal(byEntity) { 
                        TotalHealing = this.Attributes["totalhealing"].AsFloat(), 
                        HealingTime = this.Attributes["healingtime"].AsFloat() 
                    });

                    slot.TakeOut(1);
                    slot.MarkDirty();
                }

            handling = EnumHandHandling.Handled;
        }
    }
}
