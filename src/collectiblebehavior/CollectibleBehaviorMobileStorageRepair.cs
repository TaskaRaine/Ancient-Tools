using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace AncientTools.CollectibleBehaviors
{
    class CollectibleBehaviorMobileStorageRepair: CollectibleBehavior
    {
        private int RepairRate { get; set; } = 5;
        private float RepairInterval { get; set; } = 1;

        public CollectibleBehaviorMobileStorageRepair(CollectibleObject collObj) : base(collObj)
        {

        }
        public override void Initialize(JsonObject properties)
        {
            base.Initialize(properties);

            if (properties["repairRate"].Exists)
                RepairRate = properties["repairRate"].AsInt();

            if (properties["repairInterval"].Exists)
                RepairInterval = properties["repairInterval"].AsFloat();
        }
    }
}
