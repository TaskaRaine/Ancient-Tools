using AncientTools.Utility;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace AncientTools.EntityRenderers
{
    class MobileStorageRenderer : EntityRenderer
    {
        protected struct PlacementProperties
        {
            public double[] Translation;
            public double[] Rotation;
            public double Scale;
        }
        public ICoreClientAPI Capi { get; set; }
        public Shape CurrentShape { get; set; }
        public Shape[] InventoryShapes { get; set; }
        public EntityMobileStorage MobileStorageEntity { get; set; }

        protected PlacementProperties StoragePlacementProperties;

        public MobileStorageRenderer(EntityMobileStorage entity, ICoreClientAPI api) : base(entity, api)
        {
            Capi = api;

            MobileStorageEntity = entity;
        }
        public void AssignStoragePlacementProperties(JsonObject props)
        {
            if (props == null)
            {
                StoragePlacementProperties.Translation = new double[] { 0.0, 0.0, 0.0 };
                StoragePlacementProperties.Rotation = new double[] { 0.0, 0.0, 0.0 };
                StoragePlacementProperties.Scale = 1.0;

                return;
            }

            if (props.KeyExists("translation"))
                StoragePlacementProperties.Translation = props["translation"].AsArray<double>();
            else
                StoragePlacementProperties.Translation = new double[] { 0.0, 0.0, 0.0 };

            if (props.KeyExists("rotation"))
                StoragePlacementProperties.Rotation = props["rotation"].AsArray<double>();
            else
                StoragePlacementProperties.Rotation = new double[] { 0.0, 0.0, 0.0 };

            if (props.KeyExists("scale"))
                StoragePlacementProperties.Scale = props["scale"].AsDouble();
            else
                StoragePlacementProperties.Scale = 1.0;
        }
        public override void Dispose()
        {

        }
    }
}
