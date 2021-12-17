using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;

namespace AncientTools.BlockEntity
{
    //-- The main purpose for this block entity is to keep track of the conversion time remaining for a raw hide sack --//
    class BEHideWaterSack: Vintagestory.API.Common.BlockEntity
    {
        private long tickListener;

        private double previousHourChecked;
        private double thisHourChecked;

        private double timeRemaining;

        public override void Initialize(ICoreAPI api)
        {
            if (api.Side == EnumAppSide.Server)
                tickListener = api.World.RegisterGameTickListener(HourlyTicker, (int)(3600000 / api.World.Calendar.SpeedOfTime));

            timeRemaining = Block.Attributes["conversiontime"].AsDouble();
            previousHourChecked = api.World.Calendar.TotalHours;

            base.Initialize(api);
        }
        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            timeRemaining = tree.GetDouble("timeremaining");

            base.FromTreeAttributes(tree, worldAccessForResolve);
        }
        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            tree.SetDouble("timeremaining", timeRemaining);

            base.ToTreeAttributes(tree);
        }
        public override void OnBlockBroken(IPlayer player)
        {
            base.OnBlockBroken(player);

            if (Api.Side == EnumAppSide.Server)
                Api.World.UnregisterGameTickListener(tickListener);
        }
        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();

            if (Api.Side == EnumAppSide.Server)
                Api.World.UnregisterGameTickListener(tickListener);
        }
        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();

            if (Api.Side == EnumAppSide.Server)
                Api.World.UnregisterGameTickListener(tickListener);
        }
        public string GetTimeRemainingInfo()
        {
            return Lang.Get("ancienttools:blockdesc-hidewatersack-soak-x-hours", (int)(timeRemaining + 0.5)); 
        }
        public double GetTimeRemaining()
        {
            return timeRemaining;
        }
        private void HourlyTicker(float deltaTime)
        {
            thisHourChecked = Api.World.Calendar.TotalHours;

            timeRemaining -= (thisHourChecked - previousHourChecked);

            if (timeRemaining <= 0)
            {
                Api.World.BlockAccessor.ExchangeBlock(Api.World.BlockAccessor.GetBlock(new AssetLocation("ancienttools", "hidewatersack-soaked-" + Block.LastCodePart())).Id, this.Pos);
                OnBlockRemoved();
                Api.World.BlockAccessor.RemoveBlockEntity(this.Pos);
            }

            previousHourChecked = thisHourChecked;
            this.MarkDirty(true);
        }
    }
}
