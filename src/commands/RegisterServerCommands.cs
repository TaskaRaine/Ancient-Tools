using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace AncientTools.Commands
{
    class RegisterServerCommands: ModSystem
    {
        public override bool ShouldLoad(EnumAppSide side)
        {
            return side == EnumAppSide.Server;
        }
        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);

            api.RegisterCommand("removemobilestoragedebuff", Lang.Get("ancienttools:commanddesc-removemobilestoragedebuff"), Lang.Get("ancienttools:commanddesc-removemobilestoragedebuff-syntax"),
            (IServerPlayer player, int groupId, CmdArgs args) => {
                if(args.Length == 0)
                {
                    if(player.Entity.Stats["walkspeed"].ValuesByKey.ContainsKey("cartspeedmodifier"))
                    {
                        player.Entity.Stats.Remove("walkspeed", "cartspeedmodifier");

                        api.SendMessage(player, 0, Lang.Get("ancienttools:commandmsg-removemobilestoragedebuff-success"), EnumChatType.OwnMessage);

                        return;
                    }

                    api.SendMessage(player, 0, Lang.Get("ancienttools:commandmsg-removemobilestoragedebuff-failure"), EnumChatType.OwnMessage);
                }
                else
                {
                    foreach(IPlayer playerOnline in api.World.AllOnlinePlayers)
                    {
                        if(playerOnline.PlayerName == args[0])
                        {
                            if(playerOnline.Entity.Stats["walkspeed"].ValuesByKey.ContainsKey("cartspeedmodifier"))
                            {
                                playerOnline.Entity.Stats.Remove("walkspeed", "cartspeedmodifier");

                                api.SendMessage(player, 0, Lang.Get("ancienttools:commandmsg-removemobilestoragedebuff-success-player", playerOnline.PlayerName ), EnumChatType.OwnMessage);
                            }
                            else
                                api.SendMessage(player, 0, Lang.Get("ancienttools:commandmsg-removemobilestoragedebuff-failure-player", playerOnline.PlayerName), EnumChatType.OwnMessage);

                            return;
                        }
                    }

                    api.SendMessage(player, 0, Lang.Get("ancienttools:commandmsg-removemobilestoragedebuff-noplayer", args[0]), EnumChatType.OwnMessage);
                }

            }, Privilege.controlserver);;

        }
    }
}
