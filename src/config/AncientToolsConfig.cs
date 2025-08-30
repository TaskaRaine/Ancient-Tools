using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace AncientTools.Config
{
    class AncientToolsConfig
    {
        public float MortarGrindTime = 4.0f;
        public int MortarOutputModifier = 1;
        public bool AllowMortarGrindablePlaceableWithSneak = false;
        public int BarkPerLog = 4;
        public double BaseBarkStrippingSpeed = 1.0;
        public double BasePrimitiveBarrelCarvingSpeed = 1.0;
        public double SalveMixTime = 1.5;
        public bool BarkBreadEnabled = true;
        public bool SalveEnabled = true;
        public bool DisableVanillaHideCrafting = false;
        public bool DisableVanillaHideCraftingRecipeOnly = false;
        public bool DisableVanillaGlue = false;
        public bool AllowCarvingForResin = true;
        public float SkinningTime = 4.0f;
        public double WaterSackConversionHours = 48.0;
        public bool DisableBrainDrops = false;
        public int BrainsPerBrainingSolutionCraft = 1;
        public float BrainedHideSealHours = 96.0f;
        public float BrainedHideSmokingSeconds = 1200.0f;
        public bool InWorldBeamCraftingOnly = true;
        public bool DisableWedgePickupWireframe = false;
        public bool AdzeStrippingOnly = true;
        public int CandleChamberstickLightLevel = 14;
        public int PitchChamberstickLightLevel = 16;

        public AncientToolsConfig()
        {

        }
        public AncientToolsConfig(AncientToolsConfig previousConfig)
        {
            MortarGrindTime = previousConfig.MortarGrindTime;
            MortarOutputModifier = previousConfig.MortarOutputModifier;
            AllowMortarGrindablePlaceableWithSneak = previousConfig.AllowMortarGrindablePlaceableWithSneak;
            BarkPerLog = previousConfig.BarkPerLog;
            BaseBarkStrippingSpeed = previousConfig.BaseBarkStrippingSpeed;
            BasePrimitiveBarrelCarvingSpeed = previousConfig.BasePrimitiveBarrelCarvingSpeed;
            SalveMixTime = previousConfig.SalveMixTime;
            BarkBreadEnabled = previousConfig.BarkBreadEnabled;
            SalveEnabled = previousConfig.SalveEnabled;
            DisableVanillaHideCrafting = previousConfig.DisableVanillaHideCrafting;
            DisableVanillaHideCraftingRecipeOnly = previousConfig.DisableVanillaHideCraftingRecipeOnly;
            DisableVanillaGlue = previousConfig.DisableVanillaGlue;
            AllowCarvingForResin = previousConfig.AllowCarvingForResin;
            SkinningTime = previousConfig.SkinningTime;
            WaterSackConversionHours = previousConfig.WaterSackConversionHours;
            DisableBrainDrops = previousConfig.DisableBrainDrops;
            BrainsPerBrainingSolutionCraft = previousConfig.BrainsPerBrainingSolutionCraft;
            BrainedHideSealHours = previousConfig.BrainedHideSealHours;
            BrainedHideSmokingSeconds = previousConfig.BrainedHideSmokingSeconds;
            InWorldBeamCraftingOnly = previousConfig.InWorldBeamCraftingOnly;
            DisableWedgePickupWireframe = previousConfig.DisableWedgePickupWireframe;
            AdzeStrippingOnly = previousConfig.AdzeStrippingOnly;
            CandleChamberstickLightLevel = previousConfig.CandleChamberstickLightLevel;
            PitchChamberstickLightLevel = previousConfig.PitchChamberstickLightLevel;
        }
    }
}
