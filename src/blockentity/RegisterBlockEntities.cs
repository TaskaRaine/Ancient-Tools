using Vintagestory.API.Common;

namespace AncientTools.BlockEntities
{
    class RegisterBlockEntities: ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterBlockEntityClass("BEMortar", typeof(BEMortar));
            api.RegisterBlockEntityClass("BECuringRack", typeof(BECuringRack));
            api.RegisterBlockEntityClass("BESalveContainer", typeof(BESalveContainer));
            api.RegisterBlockEntityClass("BEFinishedSalve", typeof(BEFinishedSalve));
            api.RegisterBlockEntityClass("BEStretchingFrame", typeof(BEStretchingFrame));
            api.RegisterBlockEntityClass("BEHideWaterSack", typeof(BEHideWaterSack));
            api.RegisterBlockEntityClass("BEPitchContainer", typeof(BEPitchContainer));
            api.RegisterBlockEntityClass("BEFinishedPitch", typeof(BEFinishedPitch));
            api.RegisterBlockEntityClass("BESplitLog", typeof(BESplitLog));
            api.RegisterBlockEntityClass("ATBELogBarrel", typeof(BELogBarrel));
        }
    }
}
