using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace AncientTools.Config
{
    class AncientToolsConfig
    {
        public float MortarGrindTime = 4.0f;
        public int MortarOutputModifier = 1;
        public int BarkPerLog = 4;
        public double BaseBarkStrippingSpeed = 1.0;
        public double SalveMixTime = 1.5;
        public bool BarkBreadEnabled = true;
        public bool SalveEnabled = true;
        public bool DisableVanillaHideCrafting = false;
        public bool DisableVanillaGlue = true;
        public bool AllowCarvingForResin = true;
        public float SkinningTime = 4.0f;
        public float WaterSackConversionHours = 24.0f;
        public float BrainedHideSealHours = 48.0f;
        public bool InWorldBeamCraftingOnly = true;
        public bool AdzeStrippingOnly = true;

        public AncientToolsConfig()
        {

        }
        public AncientToolsConfig(AncientToolsConfig previousConfig)
        {
            MortarGrindTime = previousConfig.MortarGrindTime;
            MortarOutputModifier = previousConfig.MortarOutputModifier;
            BarkPerLog = previousConfig.BarkPerLog;
            BaseBarkStrippingSpeed = previousConfig.BaseBarkStrippingSpeed;
            SalveMixTime = previousConfig.SalveMixTime;
            BarkBreadEnabled = previousConfig.BarkBreadEnabled;
            SalveEnabled = previousConfig.SalveEnabled;
            DisableVanillaHideCrafting = previousConfig.DisableVanillaHideCrafting;
            DisableVanillaGlue = previousConfig.DisableVanillaGlue;
            AllowCarvingForResin = previousConfig.AllowCarvingForResin;
            SkinningTime = previousConfig.SkinningTime;
            WaterSackConversionHours = previousConfig.WaterSackConversionHours;
            BrainedHideSealHours = previousConfig.BrainedHideSealHours;
            InWorldBeamCraftingOnly = previousConfig.InWorldBeamCraftingOnly;
            AdzeStrippingOnly = previousConfig.AdzeStrippingOnly;
        }
    }
}
