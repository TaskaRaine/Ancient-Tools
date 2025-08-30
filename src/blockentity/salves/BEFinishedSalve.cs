using AncientTools.Utility;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace AncientTools.BlockEntities
{
    class BEFinishedSalve : DisplayInventory
    {
        public ItemSlot SalveSlot
        {
            get { return GenericDisplayInventory[0]; }
        }
        public MeshData SalveMesh
        {
            get { return Meshes[0]; }
            protected set { Meshes[0] = value; }
        }
        public BEFinishedSalve()
        {
            InventorySize = 1;
            InitializeInventory();
        }
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            SalveSlot.MaxSlotStackSize = 4;

            UpdateMeshes();
        }
        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            if (!SalveSlot.Empty)
            {
                this.AddMesh(mesher, SalveMesh);
            }
            return false;
        }
        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);

            if (worldForResolving.Side == EnumAppSide.Client && Api != null)
            {
                UpdateMeshes();
            }
        }
        public override void UpdateMeshes()
        {
            if (Api.Side != EnumAppSide.Client)
                return;

            if (!SalveSlot.Empty)
            {
                string resourcePath = "ancienttools:shapes/block/salve/resourceshapes/finishedsalve" + SalveSlot.Itemstack.StackSize;

                CurrentObject = SalveSlot.Itemstack.Item;
                SalveMesh = GenMesh(Api as ICoreClientAPI, resourcePath);
            }
        }
        //-- This is called when the two prepared salve ingredients are mixed inworld to fill the container with the salve portions --//
        public void InsertSalvePortions()
        {
            SalveSlot.Itemstack = new ItemStack(Api.World.GetItem(new AssetLocation("ancienttools", "salveportion")), SalveSlot.MaxSlotStackSize);
            SalveSlot.MarkDirty();

            UpdateMeshes();
            MarkDirty(true);
        }
        public void GiveObject(IPlayer byPlayer, ItemSlot inventorySlot)
        {
            if (byPlayer.InventoryManager.TryGiveItemstack(inventorySlot.TakeOut(1)))
            {
                UpdateMeshes();
            }

            if(inventorySlot.Empty)
            {
                this.Api.World.BlockAccessor.SetBlock(this.Api.World.GetBlock(new AssetLocation("ancienttools", "salvepot-" + Block.VariantStrict["color"] + "-residuecovered")).Id, this.Pos);
                this.Api.World.BlockAccessor.MarkBlockDirty(this.Pos);
            }

            MarkDirty(true);
        }
    }
}
