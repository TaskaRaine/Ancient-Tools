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

            api.ChatCommands.Create(Lang.Get("ancienttools:commandname-removemobilestoragedebuff"))
                .RequiresPrivilege(Privilege.controlserver)
                .WithDescription(Lang.Get("ancienttools:commanddesc-removemobilestoragedebuff"))
                .WithExamples(new string[] {
                    Lang.Get("ancienttools:commanddesc-removemobilestoragedebuff-syntax-personal"),
                    Lang.Get("ancienttools:commanddesc-removemobilestoragedebuff-syntax-otherplayer")
                })
                .WithArgs(api.ChatCommands.Parsers.OptionalWord("player"))
                .HandleWith(RemoveMobileStorageDebuff);
        }
        private TextCommandResult RemoveMobileStorageDebuff(TextCommandCallingArgs args)
        {
            if(args.ArgCount == 1)
            {
                if(args[0] == null)
                {
                    if (args.Caller.Entity.Stats["walkspeed"].ValuesByKey.ContainsKey("cartspeedmodifier"))
                    {
                        args.Caller.Entity.Stats.Remove("walkspeed", "cartspeedmodifier");

                        return TextCommandResult.Success(Lang.Get("ancienttools:commandmsg-removemobilestoragedebuff-success"));
                    }

                    return TextCommandResult.Error(Lang.Get("ancienttools:commandmsg-removemobilestoragedebuff-failure"));
                }
                else
                {
                    foreach (IPlayer playerOnline in args.Caller.Entity.World.AllOnlinePlayers)
                    {
                        if (playerOnline.PlayerName == args[0].ToString())
                        {
                            if (playerOnline.Entity.Stats["walkspeed"].ValuesByKey.ContainsKey("cartspeedmodifier"))
                            {
                                playerOnline.Entity.Stats.Remove("walkspeed", "cartspeedmodifier");

                                return TextCommandResult.Success(Lang.Get("ancienttools:commandmsg-removemobilestoragedebuff-success-player", playerOnline.PlayerName));
                            }
                            else
                                return TextCommandResult.Error(Lang.Get("ancienttools:commandmsg-removemobilestoragedebuff-failure-player", playerOnline.PlayerName));
                        }
                    }
                    return TextCommandResult.Error(Lang.Get("ancienttools:commandmsg-removemobilestoragedebuff-noplayer", args[0]));
                }
            }

            return TextCommandResult.Error("Argument length mismatch. You can only remove the debuff one player at a time!");
        }
    }
}
