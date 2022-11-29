using Vintagestory.API.Common;

namespace AncientTools.Items
{
    class RegisterItems: ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            api.RegisterItemClass("ItemCuringHook", typeof(ItemCuringHook));
            api.RegisterItemClass("ItemSaltedMeat", typeof(ItemSaltedMeat));
            api.RegisterItemClass("ItemAdze", typeof(ItemAdze));
            api.RegisterItemClass("ItemBark", typeof(ItemBark));
            api.RegisterItemClass("ItemSalvePortion", typeof(ItemSalvePortion));
            api.RegisterItemClass("ItemBrain", typeof(ItemBrain));
            api.RegisterItemClass("ItemWedge", typeof(ItemWedge));
            api.RegisterItemClass("ItemMallet", typeof(ItemMallet));
            api.RegisterItemClass("ItemCart", typeof(ItemCart));
        }
    }
}
