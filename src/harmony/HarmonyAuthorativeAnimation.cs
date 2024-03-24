using AncientTools.Blocks;
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
            if (blockSel == null || byEntity == null) return;

            if (byEntity.World.BlockAccessor.GetBlock(blockSel.Position) is BlockSplitLog)
            {
                handHandling = EnumHandHandling.PreventDefaultAction;
                handling = EnumHandling.PreventDefault;
            }
        }
    }
}
