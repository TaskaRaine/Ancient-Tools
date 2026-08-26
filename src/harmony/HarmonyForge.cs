using AncientTools.BlockEntityBehaviors;
using HarmonyLib;
using System;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace AncientTools
{
    // 1.22 NOTE: BlockEntityForge.OnCommonTick was renamed to OnCommonTick200ms and the
    // private fields fuelLevel/contents were restructured (partialFuelConsumed + inventory
    // access). The patch below targeted the old shape and cannot bind in 1.22 — Harmony
    // PatchAll fails on it which also blocks the OnPlayerInteract patch below. Attribute
    // commented out to disable this patch. The fireproof-fuel feature (waterproof fuel in
    // the forge) is therefore inert pending a rewrite against the new forge tick API.
    //
    // Disabled in 1.22 (see comment above). When re-enabled, register from HarmonySetup.ApplyPatches.
    public class HarmonyForgeIgnite
    {
        public static bool Prefix(BlockEntityForge __instance, ref bool ___burning, ref double ___lastTickTotalHours, ref float ___fuelLevel, ref ItemStack ___contents)
        {
            try
            {
                BlockEntityBehaviorFireproofFuel fireproofFuelBehavior = __instance.GetBehavior<BlockEntityBehaviorFireproofFuel>();

                if (fireproofFuelBehavior != null)
                {
                    if(fireproofFuelBehavior.GetFedFireproofFuel() == true)
                    {
                        if (___burning)
                        {
                            double hoursPassed = __instance.Api.World.Calendar.TotalHours - ___lastTickTotalHours;

                            if (___fuelLevel > 0) ___fuelLevel = Math.Max(0, ___fuelLevel - (float)(2.5 / 24 * hoursPassed));

                            if (___fuelLevel <= 0)
                            {
                                ___burning = false;
                            }

                            if (___contents != null)
                            {
                                float temp = ___contents.Collectible.GetTemperature(__instance.Api.World, ___contents);
                                if (temp < 1100)
                                {
                                    float tempGain = (float)(hoursPassed * 1500);

                                    ___contents.Collectible.SetTemperature(__instance.Api.World, ___contents, Math.Min(1100, temp + tempGain));
                                }
                            }
                        }
                        ___lastTickTotalHours = __instance.Api.World.Calendar.TotalHours;

                        return false;
                    }
                }
            }
            catch
            {

            }

            return true;
        }
    }
    public class HarmonyForgeInteract
    {
        public static void Prefix(BlockEntityForge __instance, IPlayer byPlayer)
        {
            try
            {
                BlockEntityBehaviorFireproofFuel fireproofFuelBehavior = __instance.GetBehavior<BlockEntityBehaviorFireproofFuel>();

                ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;

                if(byPlayer.Entity.Controls.Sneak)
                {
                    if (fireproofFuelBehavior != null && slot.Itemstack != null)
                    {
                        CombustibleProperties combustProps = slot.Itemstack.Collectible.CombustibleProps;

                        if(combustProps != null && combustProps.BurnTemperature > 1000)
                        {
                            if(slot.Itemstack.Collectible.Attributes != null && slot.Itemstack.Collectible.Attributes["waterproofFuel"].Exists)
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
            catch
            {

            }
        }
    }
}
