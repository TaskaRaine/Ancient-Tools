using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;

namespace AncientTools.BlockEntities
{
    //-- The main purpose for this block entity is to keep track of the conversion time remaining for a raw hide sack --//
    class BEHideWaterSack: Vintagestory.API.Common.BlockEntity
    {
        private ICoreAPI coreAPI;
        private long tickListener;

        private double previousHourChecked;
        private double thisHourChecked;

        private double timeRemaining;

        public override void Initialize(ICoreAPI api)
        {
            if (api.Side == EnumAppSide.Server)
            {
                coreAPI = api;

                tickListener = api.World.RegisterGameTickListener(HourlyTicker, (int)(3600000 / api.World.Calendar.SpeedOfTime));
                this.MarkDirty(false);
            }

            base.Initialize(api);
        }
        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            timeRemaining = tree.GetDouble("timeremaining");
            previousHourChecked = tree.GetDouble("previoushourchecked", worldAccessForResolve.Calendar.TotalHours);

            base.FromTreeAttributes(tree, worldAccessForResolve);
        }
        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            tree.SetDouble("timeremaining", timeRemaining);

            if (previousHourChecked != 0)
            {
                tree.SetDouble("previoushourchecked", previousHourChecked);
            }
            else
            {
                tree.SetDouble("previoushourchecked", Api.World.Calendar.TotalHours);
                previousHourChecked = Api.World.Calendar.TotalHours;
            }

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

            Api.World.UnregisterGameTickListener(tickListener);
        }
        public string GetTimeRemainingInfo()
        {
            return Lang.Get("ancienttools:blockdesc-hidewatersack-soak-x-hours", (int)(timeRemaining + 0.5)); 
        }
        public void SetTimeRemaining(double remaining)
        {
            timeRemaining = remaining;
        }
        public double GetTimeRemaining()
        {
            return timeRemaining;
        }
        private void HourlyTicker(float deltaTime)
        {
            thisHourChecked = coreAPI.World.Calendar.TotalHours;

            timeRemaining -= (thisHourChecked - previousHourChecked);

            if (timeRemaining <= 0)
            {
                coreAPI.World.BlockAccessor.ExchangeBlock(coreAPI.World.BlockAccessor.GetBlock(new AssetLocation("ancienttools", "hidewatersack-soaked-" + Block.LastCodePart())).Id, this.Pos);
                OnBlockRemoved();
                coreAPI.World.BlockAccessor.RemoveBlockEntity(this.Pos);
            }

            previousHourChecked = thisHourChecked;
            this.MarkDirty(true);
        }
    }
}
