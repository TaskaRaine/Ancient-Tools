using AncientTools.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace AncientTools.Inventory
{
    class InventoryMobileStorage : InventoryBase
    {
        public class StorageInventory
        {
            public int SlotCount { get; set; }
            public ItemSlot[] Slots { get; set; }

            public StorageInventory()
            {

            }
            public StorageInventory(int count)
            {
                SlotCount = count;
                Slots = new ItemSlot[SlotCount];
            }

            public ItemSlot GetSlotAtIndex(int index)
            {
                return Slots[index];
            }
            public void SetSlotAtIndex(int index, ItemSlot slot)
            {
                Slots[index] = slot;
            }
            public bool Empty()
            {
                foreach(ItemSlot slot in Slots)
                {
                    if (!slot.Empty)
                        return false;
                }

                return true;
            }
        }
        public EntityMobileStorage StorageEntity;
        protected ItemSlot[] MobileStorageInventory { get; set; }
        protected List<StorageInventory> StorageContents { get; set; }

        private int StorageBlockCount { get; set; }

        public override ItemSlot this[int slotId]
        {
            /*
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
            */
            get
            {
                if (slotId < 0 || slotId >= Count) throw new ArgumentOutOfRangeException(nameof(slotId));

                if (slotId < MobileStorageInventory.Length)
                    return MobileStorageInventory[slotId];
                else
                {
                    int curInventory = 0;
                    int adjustedId = slotId - MobileStorageInventory.Length;

                    while(adjustedId >= StorageContents[curInventory].SlotCount)
                    {
                        curInventory++;

                        adjustedId -= StorageContents[curInventory].SlotCount;
                    }

                    return StorageContents[curInventory].GetSlotAtIndex(slotId - MobileStorageInventory.Length);
                }
            }
            set
            {
                if (slotId < 0 || slotId >= Count) throw new ArgumentOutOfRangeException(nameof(slotId));

                if (slotId < MobileStorageInventory.Length)
                    MobileStorageInventory[slotId] = value ?? throw new ArgumentNullException(nameof(value));
                else
                {
                    int curInventory = 0;
                    int adjustedId = slotId - MobileStorageInventory.Length;

                    while (adjustedId >= StorageContents[curInventory].SlotCount)
                    {
                        curInventory++;

                        adjustedId -= StorageContents[curInventory].SlotCount;
                    }

                    StorageContents[curInventory].SetSlotAtIndex(slotId - MobileStorageInventory.Length, value);
                }
            }
        }

        public override int Count
        {
            get 
            {
                if (StorageContents == null || StorageContents.Count == 0)
                    return MobileStorageInventory.Length;
                else
                {
                    return StoredInventorySlotCount() + MobileStorageInventory.Length;
                }
            }
        }
        public InventoryMobileStorage(EntityMobileStorage entity, int mobileInventorySlots, string className, string instanceId, ICoreAPI api, NewSlotDelegate onNewSlot = null) : base(className, instanceId, api)
        {
            StorageEntity = entity;
            StorageBlockCount = mobileInventorySlots;

            MobileStorageInventory = new ItemSlot[StorageBlockCount];

            MobileStorageInventory = GenEmptySlots(StorageBlockCount);
            StorageContents = new List<StorageInventory>();
        }
        public override void FromTreeAttributes(ITreeAttribute stitchedInventoryTrees)
        {
            MobileStorageInventory = SlotsFromTreeAttributes(stitchedInventoryTrees.GetTreeAttribute("mobilestorageinventory"), MobileStorageInventory);

            for(int i = 0; i < MobileStorageInventory.Length; i++)
            {
                if(!MobileStorageInventory[i].Empty)
                {
                    GenerateEmptyStorageInventory(MobileStorageInventory[i].Itemstack.Collectible.Attributes["mobileStorageProps"]["quantitySlots"].AsInt());
                    StorageContents[i].Slots = SlotsFromTreeAttributes(stitchedInventoryTrees.GetTreeAttribute("storagecontents" + i), StorageContents[i].Slots);
                }
            }
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            SlotsToTreeAttributes(MobileStorageInventory, tree.GetOrAddTreeAttribute("mobilestorageinventory"));

            for(int i = 0; i < MobileStorageInventory.Length; i++)
            {
                if(!MobileStorageInventory[i].Empty)
                {
                    SlotsToTreeAttributes(StorageContents[i].Slots, tree.GetOrAddTreeAttribute("storagecontents" + i));
                }
            }
        }
        public StorageInventory GetStorageContent(int index)
        {
            return StorageContents[index];
        }
        public override object ActivateSlot(int slotId, ItemSlot sourceSlot, ref ItemStackMoveOperation op)
        {
            object packet = base.ActivateSlot(slotId, sourceSlot, ref op);

            if (Api.Side == EnumAppSide.Client)
            {
                ICoreClientAPI Capi = (ICoreClientAPI)Api;

                Capi.Network.SendEntityPacket(StorageEntity.EntityId, packet);
            }

            return packet;
        }
        public void GenerateEmptyStorageInventory(int size)
        {
            if(StorageContents.Count < StorageBlockCount)
            {
                StorageContents.Add(new StorageInventory(size));
                StorageContents.Last().Slots = GenEmptySlots(size);
            }
        }
        public void RemoveEmptyStorage(int storageIndex)
        {
            if (StorageContents.Count > 0 && StorageContents[storageIndex].Empty())
                StorageContents.RemoveAt(storageIndex);
        }
        public int StorageSlotCount()
        {
            return MobileStorageInventory.Length;
        }
        public int StoredInventorySlotCount()
        {
            int totalCount = 0;

            foreach (StorageInventory storage in StorageContents)
                totalCount += storage.SlotCount;

            return totalCount;
        }
        public ItemSlot GetMobileStorageSlot(int index)
        {
            return MobileStorageInventory[index];
        }
        public ItemSlot GetStorageInventorySlot(int inventoryIndex, int slotIndex)
        {
            return StorageContents[inventoryIndex].GetSlotAtIndex(slotIndex);
        }
    }
}
