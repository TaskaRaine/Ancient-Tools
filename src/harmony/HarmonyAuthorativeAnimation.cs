using AncientTools.Blocks;
using AncientTools.CollectibleBehaviors;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace AncientTools
{
    [HarmonyPatch(typeof(CollectibleBehaviorAnimationAuthoritative), "OnHeldAttackStart", MethodType.Normal)]
    public class HarmonyAnimationAuthorativeAttackStart
    {
        static void Postfix(EntityAgent byEntity, BlockSelection blockSel, ref EnumHandHandling handHandling, ref EnumHandling handling)
        {
            if (blockSel == null || byEntity == null || byEntity.ActiveHandItemSlot.Empty) return;

            if (byEntity.World.BlockAccessor.GetBlock(blockSel.Position) is BlockSplitLog && byEntity.ActiveHandItemSlot.Itemstack.Collectible.HasBehavior<CollectibleBehaviorWedgeSmack>())
            {
                handHandling = EnumHandHandling.PreventDefaultAction;
                handling = EnumHandling.PreventDefault;
            }
            else if (byEntity.World.BlockAccessor.GetBlock(blockSel.Position) is BlockGroundStorage && byEntity.ActiveHandItemSlot.Itemstack.Collectible.HasBehavior<CollectibleBehaviorChopBarkStack>())
            {
                handHandling = EnumHandHandling.PreventDefaultAction;
                handling = EnumHandling.PreventDefault;
            }
        }
    }
}
