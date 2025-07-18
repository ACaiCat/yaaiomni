﻿namespace Chireiden.TShock.Omni;

/// <summary>
/// This is the config file for Omni.
/// </summary>
public class Config
{
    /// <summary>
    /// Weather to show the config file on load/reload.
    /// </summary>
    public Optional<bool> ShowConfig = Optional.Default(false);

    /// <summary>
    /// Weather to log all exceptions.
    /// </summary>
    public Optional<bool> LogFirstChance = Optional.Default(false, true);

    /// <summary>
    /// DateTime format for logging.
    /// </summary>
    public Optional<string> DateTimeFormat = Optional.Default("yyyy-MM-dd HH:mm:ss.fff");

    /// <summary>
    /// Let Chireiden.TShock.Omni to handle packets earlier.
    /// </summary>
    public Optional<bool> PrioritizedPacketHandle = Optional.Default(true);

    
    /// <summary>
    /// The wildcard of self player.
    /// </summary>
    public Optional<List<string>> SelfWildcardFormat = Optional.Default(new List<string> {
        "*self*"
    });

    
    /// <summary>
    /// The wildcard of matching all players. Directly using "*" itself is not
    /// suggested as some commands might have special meaning for it.
    /// </summary>
    public Optional<List<string>> PlayerWildcardFormat = Optional.Default(new List<string> {
        "*all*"
    });

    /// <summary>
    /// The pattern of matching the server itself.
    /// </summary>
    public Optional<List<string>> ServerWildcardFormat = Optional.Default(new List<string> {
        "*server*",
        "*console*",
    });

    /// <summary>
    /// Hidden commands that won't show up in the help.
    /// </summary>
    public Optional<List<string>> HideCommands = Optional.Default(new List<string> {
        DefinedConsts.Commands.Whynot,
        DefinedConsts.Commands.Admin.DebugStat,
        DefinedConsts.Commands.ResetCharacter,
        DefinedConsts.Commands.Ping,
        DefinedConsts.Commands.Echo,
        DefinedConsts.Commands.Admin.ApplyDefaultPermission,
        DefinedConsts.Commands.Admin.InspectTileFrame,
        DefinedConsts.Commands.Admin.RunBackground,
        DefinedConsts.Commands.Admin.RunLocked,
    });

    /// <summary>
    /// Startup commands.
    /// </summary>
    public Optional<List<string>> StartupCommands = Optional.Default(new List<string>());

    /// <summary>
    /// Rename commands. The key of the dictionary is the method name with the full name of the declaring type, e.g. "Chireiden.TShock.Omni.Plugin.Command_PermissionCheck".
    /// </summary>
    public Optional<Dictionary<string, List<string>>> CommandRenames = Optional.Default(new Dictionary<string, List<string>>());

    /// <summary>
    /// Random features of improvement.
    /// </summary>
    public Optional<EnhancementsSettings> Enhancements = Optional.Default(new EnhancementsSettings());

    /// <summary>
    /// Troubleshooting networking issues.
    /// </summary>
    public Optional<DebugPacketSettings> DebugPacket = Optional.Default(new DebugPacketSettings());

    /// <summary>
    /// Problems that should be fixed.
    /// </summary>
    public Optional<SoundnessSettings> Soundness = Optional.Default(new SoundnessSettings(), true);

    public Optional<PermissionSettings> Permission = Optional.Default(new PermissionSettings());

    public Optional<Modes> Mode = Optional.Default(new Modes());

    /// <summary>
    /// Other settings that can't be perfectly resolved and might have side effects.
    /// </summary>
    public Optional<MitigationSettings> Mitigation = Optional.Default(new MitigationSettings());

    public record class EnhancementsSettings
    {
        /// <summary>
        /// Remove unused client-side objects to save memory.
        /// </summary>
        public Optional<bool> TrimMemory = Optional.Default(true, true);

        /// <summary>
        /// Alternative command syntax implementation.
        /// Allow multiple commands in one line, quote inside text (e.g. te"x"t)
        /// <para>
        /// Note: this is not fully compatible with TShock's command syntax.
        /// </para>
        /// </summary>
        public Optional<bool> AlternativeCommandSyntax = Optional.Default(true);

        /// <summary>
        /// Override config file with CLI input (port, maxplayers)
        /// </summary>
        public Optional<bool> CLIoverConfig = Optional.Default(true, true);

        /// <summary>
        /// Fix the broken default language detect.
        /// <see href="https://github.com/Pryaxis/TShock/issues/2957" />
        /// </summary>
        public Optional<bool> DefaultLanguageDetect = Optional.Default(true, true);

        /// <summary>
        /// Action for TShock's update
        /// </summary>
        public Optional<UpdateOptions> SuppressUpdate = Optional.Default(UpdateOptions.Silent);

        /// <summary>
        /// <para>
        /// Socket Provider
        /// </para>
        /// <para>
        /// Different types of wrapper implementation around socket. May affect the memory usage.
        /// </para>
        /// </summary>
        public Optional<SocketType> Socket = Optional.Default(SocketType.AnotherAsyncSocketAsFallback, true);

        public Optional<NameCollisionAction> NameCollision = Optional.Default(NameCollisionAction.Unhandled, true);

        /// <summary>
        /// A few experimental tile providers. Offering slightly better performance but uses more memory (same as vanilla).
        /// </summary>
        public Optional<TileProviderOptions> TileProvider = Optional.Default(TileProviderOptions.AsIs, true);

        /// <summary>
        /// Allow extra large worlds. This only applies to the server.
        /// Vanilla PC clients will crash when the world is too large, modded or PE clients works fine.
        /// </summary>
        public Optional<bool> ExtraLargeWorld = Optional.Default(true, true);

        /// <summary>
        /// Show command aliases in the help.
        /// <para>
        /// /broadcast (/bc), /whisper (/w), ...
        /// </para>
        /// </summary>
        public Optional<int> ShowCommandAlias = Optional.Default(0);

        /// <summary>
        /// Support regex (`namea:player.*`) and IP mask (`ipa:1.1.0.0/16`).
        /// </summary>
        public Optional<bool> BanPattern = Optional.Default(true, true);

        /// <summary>
        /// Try to resolve references from loaded assemblies.
        /// </summary>
        public Optional<bool> ResolveAssembly = Optional.Default(true);

        /// <summary>
        /// IPv6 Dual Stack Support
        /// </summary>
        public Optional<bool> IPv6DualStack = Optional.Default(true);

        public enum UpdateOptions
        {
            Silent,
            Disabled,
            AsIs,
        }

        /// <summary>
        /// We found 'memory leak', from the memory dump it seems that the async networking is using much more memory than expected.
        /// <code>
        /// System.Threading.ThreadPool.s_workQueue
        /// -> System.Net.Sockets.SocketAsyncContext+BufferMemorySendOperation
        ///   -> System.Action&lt;System.Int32, System.Byte[], System.Int32, System.Net.Sockets.SocketFlags, System.Net.Sockets.SocketError&gt;
        ///     -> System.Net.Sockets.Socket.AwaitableSocketAsyncEventArgs
        /// -> System.Threading.QueueUserWorkItemCallbackDefaultContext
        ///   -> System.Net.Sockets.SocketAsyncContext+BufferMemorySendOperation
        ///     -> System.Action&lt;System.Int32, System.Byte[], System.Int32, System.Net.Sockets.SocketFlags, System.Net.Sockets.SocketError&gt;
        ///       -> System.Net.Sockets.Socket.AwaitableSocketAsyncEventArgs
        /// </code>
        /// This 'memory leak' is now confirmed to be related to <seealso cref="Chireiden.TShock.Omni.Config.MitigationSettings.InventorySlotPE"/>.
        /// </summary>
        public enum SocketType
        {
            Vanilla,
            TShock,
            AsIs,
            Unset,
            HackyBlocked,
            HackyAsync,
            AnotherAsyncSocket,
            AnotherAsyncSocketAsFallback
        }

        public enum NameCollisionAction
        {
            /// <summary>
            /// Kick the first player
            /// </summary>
            First,
            /// <summary>
            /// Kick the second player
            /// </summary>
            Second,
            /// <summary>
            /// Kick both players
            /// </summary>
            Both,
            /// <summary>
            /// Kick neither player
            /// </summary>
            None,
            /// <summary>
            /// Kick whoever does not using a known ip and not logged in, fallback to <see cref="Second"/>
            /// </summary>
            Known,
            /// <summary>
            /// Do nothing
            /// </summary>
            Unhandled,
        }

        public enum TileProviderOptions
        {
            AsIs,
            CheckedTypedCollection,
            CheckedGenericCollection,
        }
    }

    public record class DebugPacketSettings
    {
        public Optional<PacketFilter> In = Optional.Default(new PacketFilter(false), true);
        public Optional<PacketFilter> Out = Optional.Default(new PacketFilter(false), true);
        public Optional<PacketFilter> BytesOut = Optional.Default(new PacketFilter(false), true);
        public Optional<CatchedException> ShowCatchedException = Optional.Default(CatchedException.Uncommon);

        public enum CatchedException
        {
            None,
            Uncommon,
            All,
        }
    }

    public unsafe struct PacketFilter : IEquatable<PacketFilter>
    {
        public static readonly byte MaxPacket = (byte) Enum.GetValues(typeof(PacketTypes)).Cast<int>().Max();
        private fixed bool _matches[byte.MaxValue];

        public PacketFilter(bool accept)
        {
            for (var i = 0; i < byte.MaxValue; i++)
            {
                this._matches[i] = accept;
            }
        }

        public PacketFilter(params byte[] accept)
        {
            foreach (var value in accept)
            {
                this._matches[value] = true;
            }
        }

        public readonly bool Handle(byte type)
        {
            return this._matches[type];
        }

        public readonly bool Handle(int type)
        {
            return this.Handle((byte) type);
        }

        public readonly bool Handle(PacketTypes type)
        {
            return this.Handle((byte) type);
        }

        public readonly bool Equals(PacketFilter other)
        {
            var eq = true;
            for (var i = 0; i < byte.MaxValue; i++)
            {
                eq &= this._matches[i] == other._matches[i];
            }
            return eq;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is PacketFilter pf && this.Equals(pf);
        }

        public override readonly int GetHashCode()
        {
            var h = 0;
            for (var i = 0; i < byte.MaxValue; i++)
            {
                h ^= Convert.ToInt32(this._matches[i]) << (i % 32);
            }
            return h;
        }

        public static bool operator ==(PacketFilter left, PacketFilter right) => left.Equals(right);

        public static bool operator !=(PacketFilter left, PacketFilter right) => !(left == right);
    }

    public record class SoundnessSettings
    {
        /// <summary>
        /// Permission restrict server-side tile modification projectiles like liquid bombs &amp; rockets, dirt bombs.
        /// </summary>
        public Optional<bool> ProjectileKillMapEditRestriction = Optional.Default(true);

        /// <summary>
        /// Restrict quick stack to have build permission.
        /// </summary>
        public Optional<bool> QuickStackRestriction = Optional.Default(true);

        /// <summary>
        /// Restrict sign edit to have build permission.
        /// </summary>
        public Optional<bool> SignEditRestriction = Optional.Default(true);

        /// <summary>
        /// Restrict tile entity interaction to have build permission.
        /// </summary>
        public Optional<bool> ObjectInteractionRestriction = Optional.Default(true);

        /// <summary>
        /// <para>
        /// Might experience encoding issues when using legacy Windows.
        /// </para>
        /// <para>
        /// This will try to reset the encoding.
        /// Default: -1, use Encoding.Default when Win32NT and Version &lt;= 10
        /// </para>
        /// </summary>
        public Optional<int> UseDefaultEncoding = Optional.Default(Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major < 10 ? -1 : 0);

        /// <summary>
        /// <para>
        /// Terraria will translate chat commands into command id. TShock
        /// translate them back to keep the command working.
        /// However, when the server and the client have different locale,
        /// a enUS player send `/help` will be sent as `CommandId: Help`
        /// and a deDE server will translate it back to `/hilfe`, thus the
        /// command is broken.
        /// </para>
        /// <para>Cause some commands broken.</para>
        /// <para>
        /// This will try to change the translate target to enUS, so that
        /// the command will be translated back to `/help`. A deDE player
        /// may run `/help` (CommandId: Say, Content: /help) or
        /// `/hilfe` (CommandId: Help), and both works.
        /// </para>
        /// <see href="https://github.com/Pryaxis/TShock/issues/2914"/>
        /// </summary>
        public Optional<bool> UseEnglishCommand = Optional.Default(true, true);

        /// <summary>
        /// <para>
        /// Accept vanilla localized command, which is already done via <see cref="UseEnglishCommand"/> if the
        /// command comes from the client. However, the localized commands from the server side is still broken.
        /// </para>
        /// <para>
        /// This will add alias for related commands. Requires <see cref="UseEnglishCommand"/>.
        /// </para>
        /// </summary>
        public Optional<bool> AllowVanillaLocalizedCommand = Optional.Default(true, true);
    }

    public record class PermissionSettings
    {
        public Optional<PermissionLogSettings> Log = Optional.Default(new PermissionLogSettings());
        public Optional<PresetSettings> Preset = Optional.Default(new PresetSettings());

        public record class PermissionLogSettings
        {
            public Optional<bool> Enabled = Optional.Default(true);
            public Optional<int> LogCount = Optional.Default(50);
            public Optional<bool> LogDuplicate = Optional.Default(false, true);
            public Optional<double> LogDistinctTime = Optional.Default(1.0, true);
            public Optional<bool> LogStackTrace = Optional.Default(false, true);
        }

        public record class PresetSettings
        {
            public Optional<bool> Enabled = Optional.Default(true);
            public Optional<bool> AlwaysApply = Optional.Default(false, true);
            public Optional<bool> DebugForAdminOnly = Optional.Default(false);
        }
    }

    public record class Modes
    {
        public Optional<BuildingMode> Building = Optional.Default(new BuildingMode(), true);
        public Optional<PvPMode> PvP = Optional.Default(new PvPMode(), true);
        public Optional<VanillaMode> Vanilla = Optional.Default(new VanillaMode());

        public record class BuildingMode
        {
            public Optional<bool> Enabled = Optional.Default(false);
        }

        public record class PvPMode
        {
            public Optional<bool> Enabled = Optional.Default(false);
        }

        public record class VanillaMode
        {
            public Optional<bool> Enabled = Optional.Default(false);
            public Optional<List<string>> Permissions = Optional.Default(new List<string> {
                TShockAPI.Permissions.canregister,
                TShockAPI.Permissions.canlogin,
                TShockAPI.Permissions.canlogout,
                TShockAPI.Permissions.canchangepassword,
                TShockAPI.Permissions.hurttownnpc,
                TShockAPI.Permissions.spawnpets,
                TShockAPI.Permissions.summonboss,
                TShockAPI.Permissions.startinvasion,
                TShockAPI.Permissions.startdd2,
                TShockAPI.Permissions.home,
                TShockAPI.Permissions.spawn,
                TShockAPI.Permissions.rod,
                TShockAPI.Permissions.wormhole,
                TShockAPI.Permissions.pylon,
                TShockAPI.Permissions.tppotion,
                TShockAPI.Permissions.magicconch,
                TShockAPI.Permissions.demonconch,
                TShockAPI.Permissions.editspawn,
                TShockAPI.Permissions.usesundial,
                TShockAPI.Permissions.movenpc,
                TShockAPI.Permissions.canbuild,
                TShockAPI.Permissions.canpaint,
                TShockAPI.Permissions.toggleparty,
                TShockAPI.Permissions.whisper,
                TShockAPI.Permissions.canpartychat,
                TShockAPI.Permissions.cantalkinthird,
                TShockAPI.Permissions.canchat,
                TShockAPI.Permissions.synclocalarea,
                TShockAPI.Permissions.sendemoji,
                DefinedConsts.Permission.Ping
            });
            public Optional<bool> AllowJourneyPowers = Optional.Default(false);
            public Optional<bool> IgnoreAntiCheat = Optional.Default(false);
            public Optional<VanillaAntiCheat> AntiCheat = Optional.Default(new VanillaAntiCheat(), true);

            public record class VanillaAntiCheat
            {
                public Optional<bool> Enabled = Optional.Default(false);
            }
        }
    }

    public record class MitigationSettings
    {
        public Optional<bool> DisableAllMitigation = Optional.Default(false, true);

        /// <summary>
        /// <para>
        /// Mobile (PE) client keep (likely per frame) send PlayerSlot packet
        /// to the server if any item exists in non-active loadout.
        /// </para>
        /// <para>Cause lag and high memory usage.</para>
        /// <para>
        /// This will silently proceed the packet without boardcasting it, and
        /// stop future unnecessary sync.
        /// </para>
        /// Tracking: <see href="https://forums.terraria.org/index.php?threads/network-broadcast-storm.117270/"/>
        /// </summary>
        public Optional<bool> InventorySlotPE = Optional.Default(true, true);

        /// <summary>
        /// <para>
        /// Mobile (PE) client can use healing potion (etc.) without getting
        /// or being restricted by PotionSickness.
        /// </para>
        /// <para>Cause imbalance.</para>
        /// <para>
        /// This will silently revert the attempt of healing.
        /// Item is still consumed as punishment.
        /// </para>
        /// Tracking: <see href="https://forums.terraria.org/index.php?threads/almost-invincible-by-healing-with-potions-but-without-cooldown.117269/"/>
        /// </summary>
        public Optional<bool> PotionSicknessPE = Optional.Default(true, true);

        /// <summary>
        /// <para>
        /// Similar to <seealso cref="PotionSicknessPE"/>, but generic for
        /// all items.
        /// </para>
        /// <para>Cause imbalance.</para>
        /// <para>
        /// This will silently revert the attempt of using the item.
        /// Might cause player slightly desync when they try to do so.
        /// </para>
        /// Tracking: <see href="https://forums.terraria.org/index.php?threads/almost-invincible-by-healing-with-potions-but-without-cooldown.117269/"/>
        /// </summary>
        public Optional<bool> SwapWhileUsePE = Optional.Default(true, true);

        /// <summary>
        /// <para>
        /// Whether to revert the player action or not. Likely causes
        /// obvious latency desync (laggy/backwards teleport).
        /// </para>
        /// </summary>
        public Optional<bool> SwapWhileUsePEHandleAttempt = Optional.Default(false, true);

        /// <summary>
        /// <para>
        /// Chat spam rate limit. This restriction also applies to commands.
        /// Each item is a pair of rate and maximum.
        /// </para>
        /// <para>Higher rate and lower maximum means more strict.</para>
        /// <para>
        /// The default limit:
        ///   3 messages per 5 seconds,
        ///   5 messages per 20 seconds
        /// </para>
        /// </summary>
        public Optional<List<LimiterConfig>> ChatSpamRestrict = Optional.Default(new List<LimiterConfig> {
            new LimiterConfig { RateLimit = 1.6, Maximum = 5 },
            new LimiterConfig { RateLimit = 4, Maximum = 20 }
        });

        /// <summary>
        /// <para>
        /// Restrict the rate of sending <seealso cref="PacketTypes.NpcUpdateBuff"/>.
        /// In some cases, the client will send <seealso cref="PacketTypes.NpcAddBuff"/> frequently,
        /// and the server will boardcast in O(n^2) and cause network storm.
        /// </para>
        /// <para>Likely caused by shimmer.</para>
        /// <para>
        /// This will replace the logic of these two packets and only boardcast at time interval.
        /// Use with caution.
        /// </para>
        /// </summary>
        public Optional<bool> NpcUpdateBuffRateLimit = Optional.Default(false, true);

        /// <summary>
        /// <para>
        /// Some terminals does not support Operating System Commands (OSCs) for setting the title.
        /// This will cause title being interpreted as stdio.
        /// </para>
        /// <para>
        /// linux should support OSC commands according to the implementation of <seealso cref="Console.Title"/>,
        /// but some doesn't.
        /// <see href="https://source.dot.net/#System.Console/System/TerminalFormatStrings.cs,e0a3bdd93a9caf05,references" />
        /// </para>
        /// <para>Cause spam in console.</para>
        /// <para>
        /// This will prevent the title from being set if TERM has no xterm.
        /// </para>
        /// </summary>
        public Optional<TitleSuppression> SuppressTitle = Optional.Default(TitleSuppression.Smart, true);

        /// <summary>
        /// <para>
        /// Some script kiddies spam connection requests to the server and occupy the connection pool.
        /// </para>
        /// <para>Cause player unable to connect.</para>
        /// <para>
        /// This will silently drop the connection request that exceeds the limit.
        /// Does not apply to local address.
        /// </para>
        /// <para>
        /// The default limit:
        ///   1.6 connections per 5 seconds,
        ///   4 connections per 60 seconds
        /// </para>
        /// </summary>
        public Optional<List<LimiterConfig>> ConnectionLimit = Optional.Default(new List<LimiterConfig> {
            new LimiterConfig { RateLimit = 3, Maximum = 5 },
            new LimiterConfig { RateLimit = 15, Maximum = 60 },
        });

        /// <summary>
        /// The restricted network type - all, public ipv4, none
        /// </summary>
        public Optional<NetworkLimit> LimitedNetwork = Optional.Default(NetworkLimit.Public, true);

        /// <summary>
        /// <para>
        /// Some script kiddies spam connection requests to the server and occupy the connection pool.
        /// </para>
        /// <para>Cause player unable to connect.</para>
        /// <para>
        /// This will disconnect the client if they are in the state for too long.
        /// Also apply to local address.
        /// </para>
        /// <para>
        /// The default limit:
        ///   Socket created: 1 second
        ///   <seealso cref="PacketTypes.ConnectRequest" /> received: +3 seconds
        /// </para>
        /// </summary>
        public Optional<Dictionary<int, double>> ConnectionStateTimeout = Optional.Default(new Dictionary<int, double> {
            { 0, 1 },
            { 1, 4 },
        }, true);

        /// <summary>
        /// <para>
        /// Disabled players are restricted from most actions including being hurt.
        /// </para>
        /// <para>Cause imbalance.</para>
        /// <para>
        /// This will allow disabled players to be hurt.
        /// </para>
        /// <see href="https://github.com/Pryaxis/TShock/issues/1151" />
        /// </summary>
        public Optional<DisabledDamageAction> DisabledDamageHandler = Optional.Default(DisabledDamageAction.Hurt, true);

        /// <summary>
        /// <para>
        /// In expert mode enemies can pick up coins.
        /// Each client will attempts to pick up coins, causing the NPC picking up multiple times,
        /// and grows exponentially as the iteration goes.
        /// </para>
        /// <para>Cause imbalance.</para>
        /// <para>
        /// This will try to change the behavior of the coin pickup.
        /// </para>
        /// <see href="https://github.com/Pryaxis/TShock/issues/2004"/>
        /// </summary>
        public Optional<ExpertCoinHandler> ExpertExtraCoin = Optional.Default(ExpertCoinHandler.ServerSide, true);

        /// <summary>
        /// <para>
        /// The legacy HttpServer.dll from .NET Framework 2.0 era is not responding correctly if
        /// the request is too long.
        /// </para>
        /// <para>Cause broken REST endpoint.</para>
        /// <para>
        /// This will try to remove the Connection header from the request.
        /// </para>
        /// <see href="https://github.com/Pryaxis/TShock/issues/2923"/>
        /// </summary>
        public Optional<bool> KeepRestAlive = Optional.Default(true, true);

        /// <summary>
        /// <para>
        /// TShock update legacy config { "key1": "value1", "key2": "value2" } to
        /// the new format { "Settings": { "key1": "value1", "key2": "value2" } }.
        /// If the config is partially updated, { "key1": "value1", "Settings": {
        /// "key2": "value2" } }, the key1 is discarded.
        /// </para>
        /// <para>Cause some config options not being applied.</para>
        /// <para>
        /// This will allow those partially updated config.
        /// </para>
        /// </summary>
        public Optional<PartialConfigAction> AcceptPartialUpdatedConfig = Optional.Default(PartialConfigAction.Replace, true);

        /// <summary>
        /// <para>
        /// There are many exploits that can be used to spawn arbitrary items.
        /// </para>
        /// <para>
        /// This will try to stop them.
        /// </para>
        /// </summary>
        public Optional<bool> OverflowWorldGenItemID = Optional.Default(true);

        /// <summary>
        /// <para>
        /// There are many exploits that can be used to crash the server via stack overflow.
        /// </para>
        /// <para>
        /// This will try to clear related tiles when detected (only when `/inspecttileframe`)
        /// </para>
        /// </summary>
        public Optional<bool> ClearOverflowWorldGenStackTrace = Optional.Default(false);

        /// <summary>
        /// <para>
        /// There are many exploits that can be used to crash the server via stack overflow.
        /// </para>
        /// <para>
        /// This will try to save the map when detected (only when `/inspecttileframe`)
        /// </para>
        /// </summary>
        public Optional<bool> DumpMapOnStackOverflowWorldGen = Optional.Default(true);

        /// <summary>
        /// <para>
        /// The `WorldGen.countTiles` is recursive and might cause stack overflow.
        /// Reported by a Mac (rosetta) user @ Discord during spider cave gen every time.
        /// </para>
        /// <para>
        /// This will try to rewrite to non-recursive.
        /// </para>
        /// </summary>
        public Optional<bool> NonRecursiveWorldGenTileCount = Optional.Default(true);

        /// <summary>
        /// <para>
        /// Allow journey and non-journey players to join the server.
        /// </para>
        /// <para>
        /// There is nothing wrong with this option. It is inside the Mitigations because it
        /// requires GetData detour like many other mitigations and I put them together.
        /// </para>
        /// </summary>
        public Optional<bool> AllowCrossJourney = Optional.Default(false);

        /// <summary>
        /// Loadout switch is kind of broken since it interacts with the SSC even when it's disabled.
        /// </summary>
        public Optional<bool> LoadoutSwitchWithoutSSC = Optional.Default(true, true);

        /// <summary>
        /// <para>
        /// Packet spam rate limit. This restriction also applies to commands.
        /// Each item is a pair of rate and maximum.
        /// </para>
        /// <para>Higher rate and lower maximum means more strict.</para>
        /// <para>
        /// The default limit: disabled
        /// </para>
        /// <para>
        /// Sample:
        /// {
        ///
        /// }
        /// </para>
        /// </summary>
        public Optional<Dictionary<PacketFilter, LimiterConfig>?> PacketSpamLimit = Optional.Default<Dictionary<PacketFilter, LimiterConfig>?>(null);

        /// <summary>
        /// <para>
        /// Restrict all socket send operations to have exactly one message/packet per
        /// Send call. Requires AnotherAsyncSocket.
        /// </para>
        /// <para>
        /// If you SendRawData and it contains more than one message/packet, turn this off.
        /// </para>
        /// </summary>
        public Optional<bool> RestrictiveSocketSend = Optional.Default(true, true);

        /// <summary>
        /// <para>
        /// Echo unchanged inventory back to the owner. This is meaningless and might be
        /// used to duplicate items.
        /// </para>
        /// <para>
        /// If you believe that this causes performance issues, turn this off.
        /// </para>
        /// </summary>
        public Optional<bool> EchoUnchangedItem = Optional.Default(true, true);

        /// <summary>
        /// <para>
        /// Undo and re-Apply ILHooks might cause random fatal error. Not clear if it is
        /// MMRD-relared but let's disable reloading this first.
        /// </para>
        /// </summary>
        public Optional<bool> ReloadILHook = Optional.Default(false);

        /// <summary>
        /// Break tile will be tracked and TileFrame-ed again after the KillTile.
        /// This will trigger recursive tile break.
        /// </summary>
        public Optional<bool> RecursiveTileBreak = Optional.Default(false, true);

        /// <summary>
        /// Force QuickStackChests to sync inventory incrementally.
        /// <para>
        /// The new implementation will use MassWireOperationPay to consume items.
        /// It will be harder to duplicate items using QuickStackChests, but only
        /// the items from the inventory will be handled (void bag will be ignored).
        /// </para>
        /// This feature is known to be broken and will cause inventories being locked.
        /// </summary>
        public Optional<bool> IncrementalChestStack = Optional.Default(false);

        /// <summary>
        /// Allow non-vanilla name change. Commonly used by cheaters.
        /// </summary>
        public Optional<bool> AllowNonVanillaNameChange = Optional.Default(false, true);

        /// <summary>
        /// Allow invalid packets for join states. Used by both cheaters and
        /// other valid non-vanilla purposes.
        /// </summary>
        public Optional<bool> AllowNonVanillaJoinState = Optional.Default(false, true);

        public enum DisabledDamageAction
        {
            AsIs,
            Hurt,
            Ghost
        }

        public enum ExpertCoinHandler
        {
            /// <summary>
            /// Disable the picked up coin value. Some coins may vanish.
            /// </summary>
            DisableValue,
            /// <summary>
            /// Server side coin pickup.
            /// </summary>
            ServerSide,
            /// <summary>
            /// Untouched like vanilla.
            /// </summary>
            AsIs,
        }

        public enum PartialConfigAction
        {
            Ignore,
            Replace,
            // Merge
        }

        public enum NetworkLimit
        {
            All,
            Public,
            None
        }

        public enum TitleSuppression
        {
            Disabled,
            Smart,
            Enabled,
        }
    }

    public record class LimiterConfig
    {
        public double RateLimit { get; set; }
        public double Maximum { get; set; }
        public string? Action { get; set; }

        public static explicit operator Limiter(LimiterConfig config)
        {
            return new Limiter
            {
                Config = config
            };
        }
    }
}

public class Limiter
{
    public required Config.LimiterConfig Config { get; set; }
    public double Counter { get; set; }
    public string? Action => this.Config.Action;
    public bool Allowed
    {
        get
        {
            var time = new TimeSpan(DateTime.Now.Ticks).TotalSeconds;
            var tat = Math.Max(this.Counter, time) + this.Config.RateLimit;
            if (tat > time + this.Config.Maximum)
            {
                return false;
            }
            this.Counter = tat;
            return true;
        }
    }
}

public abstract class Optional
{
    public abstract bool IsHiddenValue();
    public abstract object? ObjectValue { get; set; }
    public static Optional<T> Default<T>(T value, bool hide = false)
    {
        return new Optional<T>(value, hide);
    }
}

public class Optional<T>(T value, bool hide = false) : Optional, IEquatable<Optional<T>>
{
    public bool IsDefault { private set; get; } = true;
    public bool HideWhenDefault { private set; get; } = hide;
    internal T _defaultValue = value;
    internal T? _value;
    public T Value
    {
        get => this.IsDefault ? this._defaultValue : this._value!;
        set
        {
            if (typeof(T).IsGenericType && typeof(T).GetInterface(typeof(IEnumerable<>).Name) is Type st)
            {
                var se = typeof(Enumerable).GetMethods()
                    .First(m => m.Name == nameof(Enumerable.SequenceEqual) && m.GetParameters().Length == 2)
                    .MakeGenericMethod(st.GetGenericArguments());
                if (se != null)
                {
                    this.IsDefault = (bool) se.Invoke(null, [value, this._defaultValue])!;
                    if (!this.IsDefault)
                    {
                        this._value = value;
                    }
                }
            }
            else if (EqualityComparer<T>.Default.Equals(value, this._defaultValue))
            {
                this.IsDefault = true;
            }
            else
            {
                this.IsDefault = false;
                this._value = value;
            }
        }
    }

    public override bool IsHiddenValue()
    {
        return this.IsDefault && this.HideWhenDefault;
    }

    public bool Equals(Optional<T>? other)
    {
        return this.IsDefault == other?.IsDefault
            && EqualityComparer<T>.Default.Equals(this._defaultValue, other._defaultValue)
            && EqualityComparer<T>.Default.Equals(this._value, other._value);
    }

    public override object? ObjectValue
    {
        get => this.Value;
        set
        {
            if (value is T t)
            {
                this.Value = t;
            }
        }
    }

    public static implicit operator T(Optional<T> self) => self.Value;

    public override bool Equals(object? obj)
    {
        return obj is Optional<T> ot && this.Equals(ot);
    }

    public override int GetHashCode()
    {
        if (this._defaultValue is null)
        {
            return 0;
        }
        var v = this._value is null
            ? 0
            : EqualityComparer<T>.Default.GetHashCode(this._value);
        return EqualityComparer<T>.Default.GetHashCode(this._defaultValue) ^ v;
    }
}