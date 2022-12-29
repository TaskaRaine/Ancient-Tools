using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace AncientTools.Inventory
{
    class InventoryMobileStorage : InventoryBase
    {
        public override ItemSlot this[int slotId]
        {
            get
            {
                if (slotId < 0 || slotId >= Count) throw new ArgumentOutOfRangeException(nameof(slotId));
                return Slots[slotId];
            }
            set
            {
                if (slotId < 0 || slotId >= Count) throw new ArgumentOutOfRangeException(nameof(slotId));
                Slots[slotId] = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        protected ItemSlot[] Slots { get; set; }

        public override int Count
        {
            get { return Slots.Length; }
        }
        public InventoryMobileStorage(int quantitySlots, string className, string instanceId, ICoreAPI api, NewSlotDelegate onNewSlot = null) : base(className, instanceId, api)
        {

        }
        public override void FromTreeAttributes(ITreeAttribute tree)
        {

        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {

        }
        public override object ActivateSlot(int slotId, ItemSlot sourceSlot, ref ItemStackMoveOperation op)
        {
            return base.ActivateSlot(slotId, sourceSlot, ref op);
        }
    }
}
