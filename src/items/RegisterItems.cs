using Vintagestory.API.Common;

namespace AncientTools.Items
{
    class RegisterItems: ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            api.RegisterItemClass("ItemSaltedMeat", typeof(ItemSaltedMeat));
        }
    }
}
