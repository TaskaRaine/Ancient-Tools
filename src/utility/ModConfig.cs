using AncientTools.Config;
using Vintagestory.API.Common;

namespace AncientTools.Utility
{
    class ModConfig
    {
        private AncientToolsConfig config;

        public void ReadConfig(ICoreAPI api)
        {
            try
            {
                config = LoadConfig(api);

                if (config == null)
                {
                    GenerateConfig(api);
                    config = LoadConfig(api);
                }
                else
                {
                    GenerateConfig(api, config);
                }
            }
            catch
            {
                GenerateConfig(api);
                config = LoadConfig(api);
            }

            api.World.Config.SetInt("MortarOutputModifier", config.MortarOutputModifier);
            api.World.Config.SetFloat("MortarGrindTime", config.MortarGrindTime);
            api.World.Config.SetInt("BarkPerLog", config.BarkPerLog);
            api.World.Config.SetDouble("BaseBarkStrippingSpeed", config.BaseBarkStrippingSpeed);
            api.World.Config.SetDouble("SalveMixTime", config.SalveMixTime);
            api.World.Config.SetBool("BarkBreadEnabled", config.BarkBreadEnabled);
            api.World.Config.SetBool("DisableVanillaHideCrafting", config.DisableVanillaHideCrafting);
            api.World.Config.SetBool("DisableVanillaHideCraftingRecipeOnly", config.DisableVanillaHideCraftingRecipeOnly);
            api.World.Config.SetBool("DisableVanillaGlue", config.DisableVanillaGlue);
            api.World.Config.SetBool("SalveEnabled", config.SalveEnabled);
            api.World.Config.SetBool("AllowCarvingForResin", config.AllowCarvingForResin);
            api.World.Config.SetFloat("SkinningTime", config.SkinningTime);
            api.World.Config.SetFloat("WaterSackConversionHours", config.WaterSackConversionHours);
            api.World.Config.SetInt("BrainsPerBrainingSolutionCraft", config.BrainsPerBrainingSolutionCraft);
            api.World.Config.SetFloat("BrainedHideSealHours", config.BrainedHideSealHours);
            api.World.Config.SetFloat("BrainedHideSmokingSeconds", config.BrainedHideSmokingSeconds);
            api.World.Config.SetBool("InWorldBeamCraftingOnly", config.InWorldBeamCraftingOnly);
            api.World.Config.SetBool("AdzeStrippingOnly", config.AdzeStrippingOnly);
            api.World.Config.SetInt("CandleChamberstickLightLevel", config.CandleChamberstickLightLevel);
            api.World.Config.SetInt("PitchChamberstickLightLevel", config.PitchChamberstickLightLevel);
        }
        private AncientToolsConfig LoadConfig(ICoreAPI api)
        {
            return api.LoadModConfig<AncientToolsConfig>("AncientToolsConfig.json");
        }
        private void GenerateConfig(ICoreAPI api)
        {
            api.StoreModConfig<AncientToolsConfig>(new AncientToolsConfig(), "AncientToolsConfig.json");
        }
        private void GenerateConfig(ICoreAPI api, AncientToolsConfig previousConfig)
        {
            api.StoreModConfig<AncientToolsConfig>(new AncientToolsConfig(previousConfig), "AncientToolsConfig.json");
        }
    }
}
