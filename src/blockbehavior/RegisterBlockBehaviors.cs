﻿using Vintagestory.API.Common;

namespace AncientTools.BlockBehaviors
{
    class RegisterBlockBehaviors: ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            api.RegisterBlockBehaviorClass("ConvertToResinLog", typeof(BlockBehaviorConvertToResinLog));
            api.RegisterBlockBehaviorClass("SplitLog", typeof(BlockBehaviorSplitLog));
            api.RegisterBlockBehaviorClass("AdzeStrip", typeof(BlockBehaviorAdzeStrip));
            api.RegisterBlockBehaviorClass("ATCarveLogBarrel", typeof(BlockBehaviorCarveLogBarrel));
            api.RegisterBlockBehaviorClass("ATSealLogBarrelInfo", typeof(BlockBehaviorSealLogBarrelInfo));
        }
    }
}
