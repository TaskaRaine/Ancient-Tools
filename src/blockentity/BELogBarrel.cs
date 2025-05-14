using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace AncientTools.BlockEntities
{
    public class BELogBarrel: BlockEntityBarrel
    {
        public BELogBarrel(): base() {

            inventory[0].MaxSlotStackSize = 32;
        }
    }
}
