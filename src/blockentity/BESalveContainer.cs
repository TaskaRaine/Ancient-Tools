using AncientTools.Blocks;
using AncientTools.Utility;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace AncientTools.BlockEntity
{
    class BESalveContainer : DisplayInventory
    {
        public ItemSlot ResourceSlot
        {
            get { return inventory[0]; }
        }
        public ItemSlot LiquidSlot
        {
            get { return inventory[1]; }
        }
        public MeshData ResourceMesh 
        {
            get { return meshes[0]; }
            protected set { meshes[0] = value; }
        }
        public MeshData LiquidMesh 
        { 
            get { return meshes[1]; }
            protected set { meshes[1] = value; }
        }

        public BESalveContainer()
        {
            InventorySize = 2;
            InitializeInventory();
        }
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            ResourceSlot.MaxSlotStackSize = 8;
            LiquidSlot.MaxSlotStackSize = 4;

            UpdateMeshes();
            MarkDirty(true);
        }
        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            if (!ResourceSlot.Empty)
            {
                this.AddMesh(mesher, ResourceMesh);
            }
            if(!LiquidSlot.Empty)
            {
                this.AddMesh(mesher, LiquidMesh);
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

            if (!ResourceSlot.Empty)
            {
                string resourcePath = "ancienttools:shapes/block/salve/resourceshapes/barkfilled" + ResourceSlot.Itemstack.StackSize;

                currentObject = ResourceSlot.Itemstack.Item;
                ResourceMesh = GenMesh(Api as ICoreClientAPI, resourcePath);
            }
            if (!LiquidSlot.Empty)
            {
                string liquidPath = "";

                if (LiquidSlot.Itemstack.Collectible.Attributes["isSalveOil"].AsBool() == true)
                {
                    liquidPath = "ancienttools:shapes/block/salve/resourceshapes/salveoil" + LiquidSlot.Itemstack.StackSize;
                }
                else if (LiquidSlot.Itemstack.Collectible.Attributes["isSalveThickener"].AsBool() == true)
                {
                    liquidPath = "ancienttools:shapes/block/salve/resourceshapes/hardwax" + LiquidSlot.Itemstack.StackSize;
                }

                currentObject = LiquidSlot.Itemstack.Item;
                LiquidMesh = GenMesh(Api as ICoreClientAPI, liquidPath);
            }
        }
        //-- Give the player an object in one of the slots if an empty hand is used. Otherwise, takes an item from the player if that item matches criteria --//
        public void OnInteract(IPlayer byPlayer)
        {
            ItemSlot activeSlot = byPlayer.InventoryManager.ActiveHotbarSlot;

            if (activeSlot.Empty)
            {
                if(!LiquidSlot.Empty)
                {
                    GiveObject(byPlayer, LiquidSlot);
                }
                else if(!ResourceSlot.Empty)
                {
                    GiveObject(byPlayer, ResourceSlot);
                }

                return;
            }

            CollectibleObject activeCollectible = activeSlot.Itemstack.Collectible;

            if (activeCollectible.Attributes == null)
                return;

            if (activeCollectible.Attributes["isMedicinalBark"].Exists)
            {
                if (!LiquidSlot.Empty)
                    if (LiquidSlot.Itemstack.Collectible.Attributes["isSalveThickener"].Exists)
                        return;

                if(activeCollectible.Attributes["isMedicinalBark"].AsBool() == true)
                {
                    InsertObject(activeSlot, ResourceSlot, 1);
                    return;
                }
            }

            if (activeCollectible.Attributes["isSalveOil"].Exists)
                if(activeCollectible.Attributes["isSalveOil"].AsBool() == true)
                {
                    if(LiquidSlot.Empty || LiquidSlot.Itemstack.Collectible == activeSlot.Itemstack.Collectible)
                        InsertObject(activeSlot, LiquidSlot, 1);
                    return;
                }

            if(activeCollectible.Attributes["isSalveThickener"].Exists && ResourceSlot.Empty)
                if(activeCollectible.Attributes["isSalveThickener"].AsBool() == true)
                {
                    if (LiquidSlot.Empty || LiquidSlot.Itemstack.Collectible == activeSlot.Itemstack.Collectible)
                        InsertObject(activeSlot, LiquidSlot, 1);
                    return;
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
            if(byPlayer.InventoryManager.TryGiveItemstack(inventorySlot.TakeOut(1)))
            {
                UpdateMeshes();
            }

            MarkDirty(true);
        }
        //-- When the player completely fills the container with the appropriate resources, it is converted to a new block representing those ingredients. --//
        //-- This is done so that the player can pick up the container and the block won't drop the resources. It can then be placed in the firepit and cooked --//
        public void ConvertIfComplete()
        {
            if (Api.Side == EnumAppSide.Server)
            {
                if (!ResourceSlot.Empty && !LiquidSlot.Empty)
                {
                    if (ResourceSlot.Itemstack.StackSize == 8)
                    {
                        if (LiquidSlot.Itemstack.StackSize == 4)
                        {
                            Api.World.BlockAccessor.SetBlock(Api.World.BlockAccessor.GetBlock(new AssetLocation("ancienttools", "salvepot-" + ResourceSlot.Itemstack.Item.LastCodePart())).Id, Pos);
                            Api.World.BlockAccessor.RemoveBlockEntity(Pos);
                            Api.World.BlockAccessor.MarkBlockDirty(Pos);
                        }
                    }
                }
                else if(!LiquidSlot.Empty)
                {
                    if (LiquidSlot.Itemstack.Item.Attributes["isSalveThickener"].Exists)
                        if (LiquidSlot.Itemstack.StackSize == 4)
                        {
                            Api.World.BlockAccessor.SetBlock(Api.World.BlockAccessor.GetBlock(new AssetLocation("ancienttools", "salvepot-hardwax")).Id, Pos);
                            Api.World.BlockAccessor.RemoveBlockEntity(Pos);
                            Api.World.BlockAccessor.MarkBlockDirty(Pos);
                        }
                }
            }
        }
    }
}
