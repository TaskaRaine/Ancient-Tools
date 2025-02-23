using AncientTools.BlockEntityBehaviors;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace AncientTools
{
    [HarmonyPatch(typeof(BEBehaviorTemperatureSensitive), "onTick", MethodType.Normal)]
    public class RunFireproofCheck
    {
        static bool Prefix(BEBehaviorTemperatureSensitive __instance)
        {
            try
            {
                if(__instance.Api.Side == EnumAppSide.Server)
                {
                    var lblock = __instance.Api.World.BlockAccessor.GetBlock(__instance.Pos, BlockLayersAccess.Fluid);
                    if (lblock.IsLiquid() && lblock.LiquidCode != "lava")
                    {
                        return true;
                    }

                    BlockEntityBehaviorFireproofFuel fireproofFuelBehavior = __instance.Blockentity.GetBehavior<BlockEntityBehaviorFireproofFuel>();

                    if (fireproofFuelBehavior == null || fireproofFuelBehavior.GetFedFireproofFuel() == false)
                        return true;

                    bool rainCheck =
                        __instance.Api.Side == EnumAppSide.Server
                        && __instance.Api.World.BlockAccessor.GetRainMapHeightAt(__instance.Pos.X, __instance.Pos.Z) <= __instance.Pos.Y;

                    if (rainCheck)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return true;
            }
        }
    }
}
