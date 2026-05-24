using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace AncientTools
{
    class HarmonySetup : ModSystem
    {
        private const string HarmonyId = "ca.taska.ancienttools";
        private readonly Harmony harmony = new Harmony(HarmonyId);

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            ApplyPatches();
        }

        public override void Dispose()
        {
            base.Dispose();
            RemovePatches();
        }

        private void ApplyPatches()
        {
            harmony.Patch(
                AccessTools.Method(typeof(BlockEntityForge), "OnPlayerInteract"),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(HarmonyForgeInteract), nameof(HarmonyForgeInteract.Prefix))));

            harmony.Patch(
                AccessTools.Method(typeof(CollectibleBehaviorAnimationAuthoritative), "OnHeldAttackStart"),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(HarmonyAnimationAuthorativeAttackStart), nameof(HarmonyAnimationAuthorativeAttackStart.Postfix))));

            harmony.Patch(
                AccessTools.Method(typeof(CollectibleBehaviorAnimationAuthoritative), "OnHeldAttackStep"),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(HarmonyAnimationAuthorativeAttackStep), nameof(HarmonyAnimationAuthorativeAttackStep.Postfix))));

            harmony.Patch(
                AccessTools.Method(typeof(BEBehaviorTemperatureSensitive), "onTick"),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(RunFireproofCheck), nameof(RunFireproofCheck.Prefix))));

            harmony.Patch(
                AccessTools.Method(typeof(BEBehaviorBurning), "OnSlowServerTick"),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(HarmonyBurningSlowTick), nameof(HarmonyBurningSlowTick.Prefix))));

            harmony.Patch(
                AccessTools.Method(typeof(BlockEntityFirepit), "igniteFuel"),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(SetFireproof), nameof(SetFireproof.Prefix))));

            harmony.Patch(
                AccessTools.Method(typeof(BlockEntityPitKiln), "OnPlayerInteractStart"),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(HarmonyPitKilnInteract), nameof(HarmonyPitKilnInteract.Prefix))));

            // BlockEntityForge.OnCommonTick200ms → HarmonyForgeIgnite is intentionally not patched.
            // The class still exists but its 1.21 forge tick API (fuelLevel / contents fields, OnCommonTick name)
            // no longer matches 1.22's partialFuelConsumed + inventory-based shape. Re-enable here when the
            // fireproof-fuel forge feature is rewritten against the new API.
        }

        private void RemovePatches()
        {
            harmony.Unpatch(AccessTools.Method(typeof(BlockEntityForge), "OnPlayerInteract"), HarmonyPatchType.Prefix, HarmonyId);
            harmony.Unpatch(AccessTools.Method(typeof(CollectibleBehaviorAnimationAuthoritative), "OnHeldAttackStart"), HarmonyPatchType.Postfix, HarmonyId);
            harmony.Unpatch(AccessTools.Method(typeof(CollectibleBehaviorAnimationAuthoritative), "OnHeldAttackStep"), HarmonyPatchType.Postfix, HarmonyId);
            harmony.Unpatch(AccessTools.Method(typeof(BEBehaviorTemperatureSensitive), "onTick"), HarmonyPatchType.Prefix, HarmonyId);
            harmony.Unpatch(AccessTools.Method(typeof(BEBehaviorBurning), "OnSlowServerTick"), HarmonyPatchType.Prefix, HarmonyId);
            harmony.Unpatch(AccessTools.Method(typeof(BlockEntityFirepit), "igniteFuel"), HarmonyPatchType.Prefix, HarmonyId);
            harmony.Unpatch(AccessTools.Method(typeof(BlockEntityPitKiln), "OnPlayerInteractStart"), HarmonyPatchType.Prefix, HarmonyId);
        }
    }
}
