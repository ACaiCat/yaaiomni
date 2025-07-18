﻿using TShockAPI;
using TShockAPI.Hooks;

namespace Chireiden.TShock.Omni;

public partial class Plugin
{
    private void TSHook_Wildcard_PlayerCommand(PlayerCommandEventArgs args)
    {
        if (args.Handled)
        {
            return;
        }

        for (var i = 0; i < args.Parameters.Count; i++)
        {
            var arg = args.Parameters[i];

            if (this.config.SelfWildcardFormat.Value.Contains(arg))
            {
                if (!args.Player.RealPlayer)
                {
                    continue;
                }
                
                args.Parameters[i] = args.Player.Name;
                continue;
            }
            
            if (this.config.PlayerWildcardFormat.Value.Contains(arg))
            {
                args.Handled = true;
                foreach (var player in Utils.ActivePlayers)
                {
                    var newargs = args.Parameters.ToList();
                    newargs[i] = player.Name;
                    TShockAPI.Commands.HandleCommand(args.Player, Utils.ToCommand($"{args.CommandPrefix}{args.CommandName}", newargs));
                }
                return;
            }
        }
    }

    private List<TSPlayer> Detour_Wildcard_GetPlayers(Func<string, List<TSPlayer>> orig, string name)
    {
        return this.config.ServerWildcardFormat.Value.Contains(name) ? new List<TSPlayer> { TSPlayer.Server } : orig(name);
    }
}