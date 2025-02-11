using AncientTools.BlockEntityBehaviors;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace AncientTools
{
    [HarmonyPatch(typeof(BlockEntityFirepit), "igniteFuel", MethodType.Normal)]
    public class SetFireproof
    {
        static void Prefix(BlockEntityFirepit __instance)
        {
            try
            {
                BlockEntityBehaviorFireproofFuel fireproofFuelBehavior = __instance.GetBehavior<BlockEntityBehaviorFireproofFuel>();

                if (__instance.fuelStack != null && fireproofFuelBehavior != null)
                {
                    CollectibleObject fuelObject = __instance.fuelStack.Collectible;

                    if (fuelObject.Attributes != null && fuelObject.Attributes["waterproofFuel"].Exists && fuelObject.Attributes["waterproofFuel"].AsBool() == true)
                        fireproofFuelBehavior.SetFedFireproofFuel(true);
                    else
                        fireproofFuelBehavior.SetFedFireproofFuel(false);
                }
            }
            catch
            {

            }
        }
    }
}
