using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace AncientTools.CollectibleBehaviors
{
    class RegisterCollectibleBehaviors: ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterCollectibleBehaviorClass("SalveIngredient", typeof(CollectibleBehaviorSalveIngredient));
            api.RegisterCollectibleBehaviorClass("ConvertHide", typeof(CollectibleBehaviorConvertHide));
        }
    }
}
