using AncientTools.BlockEntityBehaviors;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace AncientTools
{
    [HarmonyPatch(typeof(BEBehaviorBurning), "OnSlowServerTick", MethodType.Normal)]
    public class HarmonyBurningSlowTick
    {
        static bool Prefix(BEBehaviorBurning __instance, Block ___fireBlock, Cuboidf ___fireCuboid)
        {
            try
            {
                BlockEntityBehaviorFireproofFuel fireproofFuelBehavior = __instance.Blockentity.GetBehavior<BlockEntityBehaviorFireproofFuel>();

                if (fireproofFuelBehavior != null && fireproofFuelBehavior.GetFedFireproofFuel() == true)
                {
                    if (!__instance.OnCanBurn(__instance.FuelPos) && __instance.Api.ModLoader.GetModSystem<ModSystemBlockReinforcement>()?.IsReinforced(__instance.FuelPos) != true)
                    {
                        //__instance.KillFire(false);
                        return true;
                    }

                    Entity[] entities = __instance.Api.World.GetEntitiesAround(__instance.FirePos.ToVec3d().Add(0.5, 0.5, 0.5), 3, 3, (e) => true);
                    Vec3d ownPos = __instance.FirePos.ToVec3d();
                    for (int i = 0; i < entities.Length; i++)
                    {
                        Entity entity = entities[i];
                        if (!CollisionTester.AabbIntersect(entity.SelectionBox, entity.ServerPos.X, entity.ServerPos.Y, entity.ServerPos.Z, ___fireCuboid, ownPos)) continue;

                        if (entity.Alive)
                        {
                            entity.ReceiveDamage(new DamageSource() { Source = EnumDamageSource.Block, SourceBlock = ___fireBlock, SourcePos = ownPos, Type = EnumDamageType.Fire }, 2f);
                        }

                        if (__instance.Api.World.Rand.NextDouble() < 0.125)
                        {
                            entity.Ignite();
                        }
                    }

                    if (__instance.FuelPos != __instance.FirePos && __instance.Api.World.BlockAccessor.GetBlock(__instance.FirePos, BlockLayersAccess.SolidBlocks).LiquidCode == "water")
                    {
                        //__instance.KillFire(false);
                        return true;
                    }

                    return false;
                }
            }
            catch
            {
                return true;
            }

            return true;
        }
    }
}