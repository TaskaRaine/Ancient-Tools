using AncientTools.BlockEntityBehaviors;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace AncientTools
{
    [HarmonyPatch(typeof(BlockEntityPitKiln), "OnPlayerInteractStart", MethodType.Normal)]
    public class HarmonyPitKilnInteract
    {
        static void Prefix(BlockEntityPitKiln __instance, IPlayer player)
        {
            BlockEntityBehaviorFireproofFuel fireproofFuelBehavior = __instance.GetBehavior<BlockEntityBehaviorFireproofFuel>();

            ItemSlot slot = player.InventoryManager.ActiveHotbarSlot;

            if (fireproofFuelBehavior != null && slot.Itemstack != null)
            {
                if (slot.Itemstack.Collectible.Attributes != null && slot.Itemstack.Collectible.Attributes["waterproofFuel"].Exists)
                {
                    if (slot.Itemstack.Collectible.Attributes["waterproofFuel"].AsBool() == true)
                        fireproofFuelBehavior.SetFedFireproofFuel(true);
                }
                else
                {
                    fireproofFuelBehavior.SetFedFireproofFuel(false);
                }
            }
        }
    }
}
