using AncientTools.Items;
using AncientTools.Utility;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace AncientTools.BlockEntityBehaviors
{
    class BlockEntityBehaviorSplitLog: DisplayInventoryBehavior
    {
        public ItemSlot WedgeSlot(int index)
        {
            return GenericDisplayInventory[index];
        }
        public MeshData WedgeMesh(int index)
        {
            return Meshes[index];
        }
        public void SetWedgeMesh(int index, MeshData mesh)
        {
            Meshes[index] = mesh;
        }
        public BlockEntityBehaviorSplitLog(BlockEntity blockentity) : base(blockentity)
        {
            InventorySize = 4;
            InitializeInventory();
        }
        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);

            for (int i = 0; i < InventorySize; i++)
                WedgeSlot(i).MaxSlotStackSize = 1;

            UpdateMeshes();
        }
        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            for (int i = 0; i < InventorySize; i++)
                if (!WedgeSlot(i).Empty)
                    this.AddMesh(mesher, WedgeMesh(i));

            return true;
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

            for(int i = 0; i < InventorySize; i++)
            {
                if (WedgeSlot(i).Empty)
                    continue;

                CurrentObject = WedgeSlot(i).Itemstack.Collectible;
                MeshData mesh = GenMesh(GenericDisplayInventory[i].Itemstack);
                SetWedgeMesh(i, mesh);
            }

            Blockentity.MarkDirty(true);
        }
        public void OnInteract(IPlayer byPlayer, int index)
        {
            ItemSlot activeSlot = byPlayer.InventoryManager.ActiveHotbarSlot;

            if(activeSlot.Empty)
            {
                if(!WedgeSlot(index).Empty)
                    GiveObject(byPlayer, WedgeSlot(index));

                return;
            }

            CollectibleObject activeCollectible = activeSlot.Itemstack.Collectible;

            if(activeCollectible is ItemWedge)
            {
                InsertWedge(byPlayer, index);
            }
        }
        public void GiveObject(IPlayer byPlayer, ItemSlot inventorySlot)
        {
            if (byPlayer.InventoryManager.TryGiveItemstack(inventorySlot.TakeOutWhole()))
            {
                UpdateMeshes();
            }

            Blockentity.MarkDirty(true);
        }
        public void InsertWedge(IPlayer byPlayer, int index)
        {
            if (byPlayer.InventoryManager.ActiveHotbarSlot.TryPutInto(Api.World, WedgeSlot(index), 1) > 0)
            {
                UpdateMeshes();
            }

            Blockentity.MarkDirty(true);
        }
    }
}
