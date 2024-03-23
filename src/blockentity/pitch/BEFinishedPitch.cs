using AncientTools.Utility;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace AncientTools.BlockEntities
{
    class BEFinishedPitch : DisplayInventory
    {
        public ItemSlot PitchSlot
        {
            get { return GenericDisplayInventory[0]; }
        }
        public MeshData PitchMesh
        {
            get { return Meshes[0]; }
            protected set { Meshes[0] = value; }
        }
        public BEFinishedPitch(): base()
        {
            InventorySize = 1;
            InitializeInventory();
        }
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            PitchSlot.MaxSlotStackSize = 4;

            UpdateMeshes();
        }
        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            if (!PitchSlot.Empty)
            {
                this.AddMesh(mesher, PitchMesh);
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

            if (!PitchSlot.Empty)
            {
                string resourcePath = "ancienttools:shapes/block/pitch/resourceshapes/pitch" + PitchSlot.Itemstack.StackSize;

                CurrentObject = PitchSlot.Itemstack.Item;
                PitchMesh = GenMesh(Api as ICoreClientAPI, resourcePath);
            }
        }
        public void GiveObject(IPlayer byPlayer, ItemSlot inventorySlot)
        {
            if (byPlayer.InventoryManager.ActiveHotbarSlot != null)
                if (byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack != null)
                    if (byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Collectible.Code.Domain == "game" && byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Collectible.FirstCodePart(0) == "stick")
                    {
                        if (byPlayer.InventoryManager.TryGiveItemstack(new ItemStack(Api.World.GetItem(new AssetLocation("ancienttools", "pitch-stick")))))
                        {
                            inventorySlot.TakeOut(1);
                            byPlayer.InventoryManager.ActiveHotbarSlot.TakeOut(1);
                            UpdateMeshes();
                        }
                    }

            if (inventorySlot.Empty)
            {
                this.Api.World.BlockAccessor.SetBlock(this.Api.World.GetBlock(new AssetLocation("ancienttools", "pitchpot-residuecovered")).Id, this.Pos);
                this.Api.World.BlockAccessor.MarkBlockDirty(this.Pos);
            }

            MarkDirty(true);
        }
        public void InsertPitch()
        {
            PitchSlot.Itemstack = new ItemStack(Api.World.GetItem(new AssetLocation("ancienttools", "pitch-ball")), 4);
            PitchSlot.MarkDirty();
            MarkDirty(true);
        }
    }
}
