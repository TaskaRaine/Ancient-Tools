using AncientTools.Config;
using Vintagestory.API.Common;

namespace AncientTools.Utility
{
    static class ModConfig
    {
        private static AncientToolsConfig config;

        public static void ReadConfig(ICoreAPI api)
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
            api.World.Config.SetBool("SalveEnabled", config.SalveEnabled);
            api.World.Config.SetFloat("SkinningTime", config.SkinningTime);
            api.World.Config.SetFloat("WaterSackConversionHours", config.WaterSackConversionHours);
            api.World.Config.SetFloat("BrainedHideSealHours", config.BrainedHideSealHours);
        }
        private static AncientToolsConfig LoadConfig(ICoreAPI api)
        {
            return api.LoadModConfig<AncientToolsConfig>("AncientToolsConfig.json");
        }
        private static void GenerateConfig(ICoreAPI api)
        {
            api.StoreModConfig<AncientToolsConfig>(new AncientToolsConfig(), "AncientToolsConfig.json");
        }
        private static void GenerateConfig(ICoreAPI api, AncientToolsConfig previousConfig)
        {
            api.StoreModConfig<AncientToolsConfig>(new AncientToolsConfig(previousConfig), "AncientToolsConfig.json");
        }
    }
}
