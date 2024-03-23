using AncientTools.Utility;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace AncientTools.BlockEntities
{
    class BEPitchContainer : DisplayInventory
    {
        public ItemSlot GrassSlot
        {
            get { return GenericDisplayInventory[0]; }
        }
        public ItemSlot ResinSlot
        {
            get { return GenericDisplayInventory[1]; }
        }
        public ItemSlot CharcoalSlot
        {
            get { return GenericDisplayInventory[2]; }
        }
        public MeshData GrassMesh
        {
            get { return Meshes[0]; }
            protected set { Meshes[0] = value; }
        }
        public MeshData ResinMesh
        {
            get { return Meshes[1]; }
            protected set { Meshes[1] = value; }
        }
        public MeshData CharcoalMesh
        {
            get { return Meshes[2]; }
            protected set { Meshes[2] = value; }
        }
        public BEPitchContainer()
        {
            InventorySize = 3;
            InitializeInventory();
        }
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            GrassSlot.MaxSlotStackSize = 1;
            ResinSlot.MaxSlotStackSize = 4;
            CharcoalSlot.MaxSlotStackSize = 2;

            UpdateMeshes();
            MarkDirty(true);
        }
        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            if (!GrassSlot.Empty)
            {
                this.AddMesh(mesher, GrassMesh);
            }
            if (!ResinSlot.Empty)
            {
                this.AddMesh(mesher, ResinMesh);
            }
            if(!CharcoalSlot.Empty)
            {
                this.AddMesh(mesher, CharcoalMesh);
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

            if (!GrassSlot.Empty)
            {
                string grassPath = "ancienttools:shapes/block/pitch/resourceshapes/grass1";

                CurrentObject = GrassSlot.Itemstack.Item;
                GrassMesh = GenMesh(Api as ICoreClientAPI, grassPath);
            }
            if (!ResinSlot.Empty)
            {
                string resinPath = "ancienttools:shapes/block/pitch/resourceshapes/resin" + ResinSlot.Itemstack.StackSize;

                CurrentObject = ResinSlot.Itemstack.Item;
                ResinMesh = GenMesh(Api as ICoreClientAPI, resinPath);
            }
            if(!CharcoalSlot.Empty)
            {
                string charcoalPath = "ancienttools:shapes/block/pitch/resourceshapes/charcoal" + CharcoalSlot.Itemstack.StackSize;

                CurrentObject = CharcoalSlot.Itemstack.Item;
                CharcoalMesh = GenMesh(Api as ICoreClientAPI, charcoalPath);
            }
        }
        public void OnInteract(IPlayer byPlayer)
        {
            ItemSlot activeSlot = byPlayer.InventoryManager.ActiveHotbarSlot;

            if (activeSlot.Empty)
            {
                if (!GrassSlot.Empty)
                {
                    GiveObject(byPlayer, GrassSlot);
                }
                else if (!ResinSlot.Empty)
                {
                    GiveObject(byPlayer, ResinSlot);
                }
                else if(!CharcoalSlot.Empty)
                {
                    GiveObject(byPlayer, CharcoalSlot);
                }

                return;
            }

            CollectibleObject activeCollectible = activeSlot.Itemstack.Collectible;

            if (activeCollectible.Attributes == null)
                return;

            if (activeCollectible.Attributes["isPitchGrass"].Exists)
            {
                if (activeCollectible.Attributes["isPitchGrass"].AsBool() == true)
                {
                    InsertObject(activeSlot, GrassSlot, 1);
                    return;
                }
            }
            if (activeCollectible.Attributes["isPitchResin"].Exists)
            {
                if (activeCollectible.Attributes["isPitchResin"].AsBool() == true)
                {
                    InsertObject(activeSlot, ResinSlot, 1);
                    return;
                }
            }
            if (activeCollectible.Attributes["isPitchCharcoal"].Exists)
            {
                if (activeCollectible.Attributes["isPitchCharcoal"].AsBool() == true)
                {
                    InsertObject(activeSlot, CharcoalSlot, 1);
                    return;
                }
            }
        }
        private void InsertObject(ItemSlot playerActiveSlot, ItemSlot inventorySlot, int takeQuantity)
        {
            if (playerActiveSlot.TryPutInto(Api.World, inventorySlot, takeQuantity) > 0)
            {
                UpdateMeshes();
            }

            MarkDirty(true);
        }
        private void GiveObject(IPlayer byPlayer, ItemSlot inventorySlot)
        {
            if (byPlayer.InventoryManager.TryGiveItemstack(inventorySlot.TakeOut(1)))
            {
                UpdateMeshes();
            }

            MarkDirty(true);
        }
        public void ConvertIfComplete()
        {
            if (Api.Side == EnumAppSide.Server)
            {
                if (!GrassSlot.Empty && !CharcoalSlot.Empty && !ResinSlot.Empty)
                {
                    if (GrassSlot.Itemstack.StackSize == 1)
                    {
                        if (CharcoalSlot.Itemstack.StackSize == 2)
                        {
                            if(ResinSlot.Itemstack.StackSize == 4)
                            {
                                if(Block.Code.EndVariant() == "residuecovered")
                                    Api.World.BlockAccessor.SetBlock(Api.World.BlockAccessor.GetBlock(new AssetLocation("ancienttools", "pitchpot-unmixedresiduecovered")).Id, Pos);
                                else
                                    Api.World.BlockAccessor.SetBlock(Api.World.BlockAccessor.GetBlock(new AssetLocation("ancienttools", "pitchpot-unmixed")).Id, Pos);

                                Api.World.BlockAccessor.RemoveBlockEntity(Pos);
                                Api.World.BlockAccessor.MarkBlockDirty(Pos);
                            }
                        }
                    }
                }
            }
        }
    }
}
