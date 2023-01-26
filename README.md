# yaaiomni
[![Total Downloads](https://img.shields.io/github/downloads/sgkoishi/yaaiomni/total?label=Downloads%40Release&style=for-the-badge) ![Latest Downloads](https://img.shields.io/github/downloads-pre/sgkoishi/yaaiomni/latest/total?label=Downloads%40Latest%20Release&style=for-the-badge)](https://github.com/sgkoishi/yaaiomni/releases) [![NuGet](https://img.shields.io/nuget/dt/Chireiden.TShock.Omni?label=NuGet&style=for-the-badge)](https://www.nuget.org/packages/Chireiden.TShock.Omni/)

Yet another misc plugin for TShock

> __Note__
> If you are using Linux and don't know which version to download, download the tar one.

### Commands

| Command | Description | Hidden | Permission | Note |
| --- | --- | --- | --- | --- |
| `/whynot` | Show recent permission queries related to your player. | Hidden by default. | `chireiden.omni.whynot` | |
| `/ghost` | Hide yourself from viewing, `/playing`, etc. | | `chireiden.omni.ghost` | |
| `/setlang` | Set the language. | | `chireiden.omni.setlang` | For admin. |
| `/_pvp` | Toggle PvP. | Hidden by default. | `chireiden.omni.pvp` <br /> `chireiden.omni.admin.setpvp` | |
| `/_team` | Toggle team. | Hidden by default. | `chireiden.omni.team` <br /> `chireiden.omni.admin.setteam` | |
| `/_debugstat` | Show debug stats. | Hidden by default. | `chireiden.omni.admin.debugstat` | |
| `/_gc` | Trigger garbage collection. | Hidden by default. | `chireiden.omni.admin.gc` <br /> `tshock.cfg.maintenance` | For admin. |
| `/maxplayers` | Set max players. | | `chireiden.omni.admin.maxplayers` | Might cause unexpected behaviour if lower than current max. |
| `/tileprovider` | Set tile provider. | | `chireiden.omni.admin.tileprovider` | For admin. |
| `/settimeout` | Run delay command. | | `chireiden.omni.settimeout` | |
| `/setinterval` | Run delay command repeatedly. | | `chireiden.omni.setinterval` | |
| `/clearinterval` | Remove a delay command. | | `chireiden.omni.clearinterval` | |
| `/showdelay` | Show all pending delay commands. | | `chireiden.omni.showdelay` | |
| `/rbc` | Broadcast a message. | | `chireiden.omni.admin.rawbroadcast` | For admin. |
| `/runas` | Execute a command as another player. | | `chireiden.omni.admin.sudo` | For owner. |
| `/listclients` | Show connected clients, including pending/joining ones. | | `chireiden.omni.admin.listclients` | For owner. |
| `/dumpbuffer` | Dump buffer. | | `chireiden.omni.admin.dumpbuffer` | For owner. |
| `/kc` | Disconnect a client. | | `chireiden.omni.admin.terminatesocket` | For admin. |
| `/resetcharacter` | Reset character. | Hidden by default. | `chireiden.omni.resetcharacter` <br /> `chireiden.omni.admin.resetcharacter` <br /> `chireiden.omni.admin.resetcharacter.all` | For admin. |
| `/_ping` | Ping. | Hidden by default. | `chireiden.omni.ping` | |
| `/_chat` | Chat. | Hidden by default. | `chireiden.omni.chat` <br /> `tshock.canchat` | |
| `/downloadcharacter` | Download SSC to this player. | | `chireiden.omni.admin.downloadcharacter` | For admin. |
| `/exportcharacter` | Export SSC data (server side). | | `chireiden.omni.admin.exportcharacter` | For admin. |

### Defaults
* The permission of `/ghost`, `/setlang`, `/_debugstat`, timeout/delay/interval series commands, `/downloadcharacter` are granted to the topmost parent of `owner` with kick permission, or `newadmin`'s parent if `owner` is not found.
* The permission of `/_gc`, `/tileprovider`, `/maxplayers`, `/rbc`, `/kc`, `/exportcharacter` are granted to the topmost parent of `owner` with maintenance permission, or `trustedadmin`'s parent if `owner` is not found.
* The permission of `/runas`, `/listclients`, `/dumpbuffer` are granted to the topmost parent of `owner` with sudo permission.
* The permission of switch loadout, pvp and team, `/_ping`, `/_chat` are granted to the guest group as TShock's config.
  * Unable to switch without these permissions. (`.Permission.Restrict` in config)
* Vanilla version check is disabled. (`.SyncVersion` in config)
* Errors thrown from TShock's update check will be silently ignored. (`.SuppressUpdate` in config)
* Chat spam limit to 3 msg/5 sec, 5 msg/20 sec. (`.Mitigation.ChatSpamRestrict` in config)

### More features
* `.TrimMemory` in config can reduce memory usage.
  * Depends on the content of the map, may vary from no effect to ~600MB reduced.
  * No side effects.
* `.Mode.Vanilla` in config can switch to vanilla mode.
  * Will allow common actions that are restricted by default.
  * Will create a group `chireiden_vanilla` as the parent of the topmost parent of the registered group.
* `.CommandRenames` in config can rename commands.
  * It's a `Dictionary<sigOfCommandDelegate: string, newalias: List<string>>`.
  * e.g. `{"Chireiden.TShock.Omni.Plugin.Command_PermissionCheck": ["whynot123", "whynot456"]}`
* `.LavaHandler` in config can stop lava spam.
  * It does not prevent lava from spawning, but rather vacuums it after it *might* spawns.
  * If you have a lava pool and spawn lots of lava slimes (or similar) and butcher, the total amount of lava will be reduced instead of unchanged.
* `.PlayerWildcardFormat` in config allow wildcard selector as player target.
  * e.g. `/g zenith *all*` will give Zenith to everyone online!
* `.Permission.Log` in config record permission queries for `/whynot`.
  * With `-v` flag shows more stack trace.
  * With `-t`/`-f` flag filters by allowed(true)/rejected(false).
* Timeout/Interval commands works like similar functions in JavaScript.
  * Time unit is in tick/frame/update, which is 60 per second.
  * Remember to quote or escape your command when necessary.
  * Use your permission.
* Sudo is called `/runas` to avoid conflict with TShock's `/sudo`.
  * With `-f` flag bypasses permission check.

### Do not touch unless you know what you are doing
> __Warning__  
> **KEEP IT UNCHANGED. DO NOT TOUCH UNLESS YOU KNOW WHAT YOU ARE DOING**
* `.Soundness` in config enforce some soundness permission checks.
* `.Mitigation` in config can fix some issues that exist but not blame to TShock.
* `.Socket` in config can switch to a different socket implementation.
  * `AnotherAsyncSocket` might help with 'memory leak'.
  * Don't use `Hacky*`.
* `/_gc` triggers garbage collection.
* `.TileProvider` in config can switch to a different tile provider.
  * `CheckedTypedCollection` and `CheckedGenericCollection` might improve performance slightly but mayy cause NRE.
* `.DebugPacket` in config can log all packets and networking exceptions.