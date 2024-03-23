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
        static void Postfix(BlockSelection blockSel, ref EnumHandling handling)
        {
            if (blockSel.Block is BlockSplitLog)
            {
                handling = EnumHandling.PreventDefault;
            }
        }
    }
}
