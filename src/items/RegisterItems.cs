using Vintagestory.API.Common;

namespace AncientTools.Items
{
    class RegisterItems: ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            api.RegisterItemClass("ItemSaltedMeat", typeof(ItemSaltedMeat));
            api.RegisterItemClass("ItemAdze", typeof(ItemAdze));
            api.RegisterItemClass("ItemBark", typeof(ItemBark));
            api.RegisterItemClass("ItemSalvePortion", typeof(ItemSalvePortion));
        }
    }
}
