﻿using Newtonsoft.Json;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace Chireiden.TShock.Omni;

[ApiVersion(2, 1)]
public partial class Plugin : TerrariaPlugin
{
    public override string Name => Assembly.GetExecutingAssembly().GetName().Name!;
    public override string Author => "SGKoishi";

    public string ConfigPath = Path.Combine(TShockAPI.TShock.SavePath, Consts.ConfigFile);

    public Config config;

    public Plugin(Main game) : base(game)
    {
        AppDomain.CurrentDomain.FirstChanceException += this.FirstChanceExceptionHandler;
        this.Order = int.MinValue;
        this.config = new Config();
        var bfany = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        this.Detour(
            nameof(this.Detour_UpdateCheckAsync),
            typeof(UpdateManager)
                .GetMethod(nameof(UpdateManager.UpdateCheckAsync), bfany)!,
            this.Detour_UpdateCheckAsync);
        this.Detour(
            nameof(this.Detour_HasPermission),
            typeof(TSPlayer)
                .GetMethod(nameof(TSPlayer.HasPermission), bfany, new[] { typeof(string) })!,
            this.Detour_HasPermission);
        this.Detour(
            nameof(this.Detour_PlayerActive),
            typeof(TSPlayer)
                .GetProperty(nameof(TSPlayer.Active), bfany)!
                .GetMethod!,
            this.Detour_PlayerActive);
        this.Detour(
            nameof(this.Detour_Lava_HitEffect),
            typeof(NPC)
                .GetMethod(nameof(NPC.HitEffect), bfany)!,
            this.Detour_Lava_HitEffect);
        this.Detour(
            nameof(this.Detour_Lava_KillTile),
            typeof(WorldGen)
                .GetMethod(nameof(WorldGen.KillTile), bfany)!,
            this.Detour_Lava_KillTile);
        this.Detour(
            nameof(this.Detour_Wildcard_GetPlayers),
            typeof(TSPlayer)
                .GetMethod(nameof(TSPlayer.FindByNameOrID), bfany)!,
            this.Detour_Wildcard_GetPlayers);
        this.Detour(
            nameof(this.Detour_Backport_2894),
            typeof(TShockAPI.DB.CharacterManager)
                .GetMethod(nameof(TShockAPI.DB.CharacterManager.InsertPlayerData), bfany)!,
            this.Detour_Backport_2894);
    }

    private void OnReload(ReloadEventArgs? e)
    {
        try
        {
            if (!File.Exists(this.ConfigPath))
            {
                File.WriteAllText(this.ConfigPath, JsonConvert.SerializeObject(this.config, Formatting.Indented));
            }
            else
            {
                this.config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(this.ConfigPath))!;
            }
        }
        catch (Exception ex)
        {
            e?.Player?.SendErrorMessage(ex.Message);
            return;
        }
        e?.Player?.SendSuccessMessage("Chireiden.Omni loaded.");
        if (this.config.ShowConfig)
        {
            e?.Player?.SendInfoMessage(JsonConvert.SerializeObject(this.config, Formatting.Indented));
        }
        switch (this.config.TileProvider)
        {
            case Config.TileProviderOptions.CheckedGenericCollection:
                Terraria.Main.tile = Utils.CloneTileCollection(Terraria.Main.tile, new CheckedGenericCollection());
                break;
            case Config.TileProviderOptions.CheckedTypedCollection:
                Terraria.Main.tile = Utils.CloneTileCollection(Terraria.Main.tile, new CheckedTypedCollection());
                break;
            case Config.TileProviderOptions.AsIs:
            case Config.TileProviderOptions.Preset:
                break;
        }
        this.PermissionSetup();
        this.VanillaSetup();
        foreach (var command in Commands.ChatCommands)
        {
            Utils.TryRenameCommand(command, this.config.CommandRenames);
        }
        foreach (var command in Commands.TShockCommands)
        {
            Utils.TryRenameCommand(command, this.config.CommandRenames);
        }
        foreach (var command in this._hiddenCommands)
        {
            Utils.TryRenameCommand(command, this.config.CommandRenames);
        }
        var spamlim = this.config.Mitigation.ChatSpamRestrict;
        if (spamlim.Count > 0)
        {
            e?.Player?.SendInfoMessage("ChatSpam limit applied:");
            foreach (var (rate, max) in spamlim)
            {
                e?.Player?.SendInfoMessage($"  {(int) (max * 10.0 / rate) / 10.0:G} messages per {(int) (max / 6.0) / 10.0:G} seconds");
            }
        }
        foreach (var field in typeof(Consts.DataKey).GetFields())
        {
            if (field.GetValue(null) is string key)
            {
                foreach (var player in TShockAPI.TShock.Players)
                {
                    if (player is null)
                    {
                        continue;
                    }
                    player.SetData<object?>(key, null);
                }
            }
        }
    }

    public override void Initialize()
    {
        On.Terraria.MessageBuffer.GetData += this.Hook_PatchVersion_GetData;
        On.Terraria.GameContent.Tile_Entities.TEDisplayDoll.ctor += this.Hook_MemoryTrim_DisplayDoll;
        On.Terraria.GameContent.Tile_Entities.TEHatRack.ctor += this.Hook_MemoryTrim_HatRack;
        On.Terraria.NetMessage.SendData += this.Hook_DebugPacket_SendData;
        On.Terraria.MessageBuffer.GetData += this.Hook_DebugPacket_GetData;
        On.Terraria.Projectile.Kill += this.Hook_Soundness_ProjectileKill;
        On.Terraria.WorldGen.clearWorld += this.Hook_TileProvider_ClearWorld;
        OTAPI.Hooks.NetMessage.SendBytes += this.Hook_Ghost_SendBytes;
        OTAPI.Hooks.NetMessage.SendBytes += this.Hook_DebugPacket_SendBytes;
        OTAPI.Hooks.MessageBuffer.GetData += this.Hook_Permission_SyncLoadout;
        OTAPI.Hooks.MessageBuffer.GetData += this.Hook_Modded_GetData;
        OTAPI.Hooks.MessageBuffer.GetData += this.Hook_Ping_GetData;
        TerrariaApi.Server.ServerApi.Hooks.NetNameCollision.Register(this, this.Hook_NameCollision);
        TerrariaApi.Server.ServerApi.Hooks.GamePostInitialize.Register(this, this.OnGamePostInitialize);
        TerrariaApi.Server.ServerApi.Hooks.GameUpdate.Register(this, this.Hook_TimeoutInterval);
        TerrariaApi.Server.ServerApi.Hooks.GameUpdate.Register(this, this.Hook_Mitigation_GameUpdate);
        TShockAPI.Hooks.PlayerHooks.PlayerCommand += this.Hook_HideCommand_PlayerCommand;
        TShockAPI.Hooks.PlayerHooks.PlayerCommand += this.Hook_Wildcard_PlayerCommand;
        TShockAPI.Hooks.PlayerHooks.PlayerPermission += this.Hook_Sudo_OnPlayerPermission;
        TShockAPI.Hooks.GeneralHooks.ReloadEvent += this.OnReload;
        TShockAPI.TShock.Initialized += this.PostTShockInitialize;
        TShockAPI.GetDataHandlers.TogglePvp.Register(this.Hook_Permission_TogglePvp);
        TShockAPI.GetDataHandlers.PlayerTeam.Register(this.Hook_Permission_PlayerTeam);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            On.Terraria.MessageBuffer.GetData -= this.Hook_PatchVersion_GetData;
            On.Terraria.GameContent.Tile_Entities.TEDisplayDoll.ctor -= this.Hook_MemoryTrim_DisplayDoll;
            On.Terraria.GameContent.Tile_Entities.TEHatRack.ctor -= this.Hook_MemoryTrim_HatRack;
            On.Terraria.NetMessage.SendData -= this.Hook_DebugPacket_SendData;
            On.Terraria.MessageBuffer.GetData -= this.Hook_DebugPacket_GetData;
            On.Terraria.NetMessage.SendData -= this.Hook_DebugPacket_CatchSend;
            On.Terraria.MessageBuffer.GetData -= this.Hook_DebugPacket_CatchGet;
            On.Terraria.Projectile.Kill -= this.Hook_Soundness_ProjectileKill;
            On.Terraria.WorldGen.clearWorld -= this.Hook_TileProvider_ClearWorld;
            OTAPI.Hooks.NetMessage.SendBytes -= this.Hook_Ghost_SendBytes;
            OTAPI.Hooks.NetMessage.SendBytes -= this.Hook_DebugPacket_SendBytes;
            OTAPI.Hooks.MessageBuffer.GetData -= this.Hook_Mitigation_GetData;
            OTAPI.Hooks.MessageBuffer.GetData -= this.Hook_Permission_SyncLoadout;
            OTAPI.Hooks.MessageBuffer.GetData -= this.Hook_Modded_GetData;
            OTAPI.Hooks.MessageBuffer.GetData -= this.Hook_Ping_GetData;
            OTAPI.Hooks.Netplay.CreateTcpListener -= this.Hook_Socket_OnCreate;
            TerrariaApi.Server.ServerApi.Hooks.NetNameCollision.Deregister(this, this.Hook_NameCollision);
            TerrariaApi.Server.ServerApi.Hooks.GamePostInitialize.Deregister(this, this.OnGamePostInitialize);
            TerrariaApi.Server.ServerApi.Hooks.GameUpdate.Deregister(this, this.Hook_TimeoutInterval);
            TerrariaApi.Server.ServerApi.Hooks.GameUpdate.Deregister(this, this.Hook_Mitigation_GameUpdate);
            TShockAPI.Hooks.PlayerHooks.PlayerCommand -= this.Hook_HideCommand_PlayerCommand;
            TShockAPI.Hooks.PlayerHooks.PlayerCommand -= this.Hook_Wildcard_PlayerCommand;
            TShockAPI.Hooks.PlayerHooks.PlayerPermission -= this.Hook_Sudo_OnPlayerPermission;
            TShockAPI.Hooks.GeneralHooks.ReloadEvent -= this.OnReload;
            TShockAPI.TShock.Initialized -= this.PostTShockInitialize;
            TShockAPI.GetDataHandlers.TogglePvp.UnRegister(this.Hook_Permission_TogglePvp);
            TShockAPI.GetDataHandlers.PlayerTeam.UnRegister(this.Hook_Permission_PlayerTeam);
            TShockAPI.GetDataHandlers.NPCAddBuff.UnRegister(this.Hook_Mitigation_NpcAddBuff);
            var asm = Assembly.GetExecutingAssembly();
            Commands.ChatCommands.RemoveAll(c => c.CommandDelegate.Method?.DeclaringType?.Assembly == asm);
            foreach (var detour in this._detours.Values)
            {
                detour.Dispose();
            }
        }
        base.Dispose(disposing);
    }

    private void OnGamePostInitialize(EventArgs args)
    {
        OTAPI.Hooks.MessageBuffer.GetData += this.Hook_Mitigation_GetData;
        On.Terraria.NetMessage.SendData += this.Hook_DebugPacket_CatchSend;
        On.Terraria.MessageBuffer.GetData += this.Hook_DebugPacket_CatchGet;
        TShockAPI.GetDataHandlers.NPCAddBuff.Register(this.Hook_Mitigation_NpcAddBuff);
    }

    private void PostTShockInitialize()
    {
        OTAPI.Hooks.Netplay.CreateTcpListener += this.Hook_Socket_OnCreate;
        Commands.ChatCommands.Add(new Command(Consts.Permissions.Whynot, this.Command_PermissionCheck, Consts.Commands.Whynot));
        Commands.ChatCommands.Add(new Command(Consts.Permissions.Admin.Ghost, this.Command_Ghost, Consts.Commands.Ghost));
        Commands.ChatCommands.Add(new Command(Consts.Permissions.Admin.SetLanguage, this.Command_Lang, Consts.Commands.SetLanguage));
        Commands.ChatCommands.Add(new Command(Consts.Permissions.PvPCommand, this.Command_PvP, Consts.Commands.SetPvp));
        Commands.ChatCommands.Add(new Command(Consts.Permissions.TeamCommand, this.Command_Team, Consts.Commands.SetTeam));
        Commands.ChatCommands.Add(new Command(new List<string> { Consts.Permissions.Admin.TriggerGarbageCollection, Permissions.maintenance },
            this.Command_GC, Consts.Commands.TriggerGarbageCollection));
        Commands.ChatCommands.Add(new Command(Consts.Permissions.Admin.DebugStat, this.Command_DebugStat, Consts.Commands.DebugStat));
        Commands.ChatCommands.Add(new Command(Consts.Permissions.Admin.MaxPlayers, this.Command_MaxPlayers, Consts.Commands.MaxPlayers));
        Commands.ChatCommands.Add(new Command(Consts.Permissions.Admin.TileProvider, this.Command_TileProvider, Consts.Commands.TileProvider));
        Commands.ChatCommands.Add(new Command(Consts.Permissions.TimeoutCommand, this.Command_SetTimeout, Consts.Commands.Timeout));
        Commands.ChatCommands.Add(new Command(Consts.Permissions.IntervalCommand, this.Command_SetInterval, Consts.Commands.Interval));
        Commands.ChatCommands.Add(new Command(Consts.Permissions.ClearInterval, this.Command_ClearInterval, Consts.Commands.ClearInterval));
        Commands.ChatCommands.Add(new Command(Consts.Permissions.ShowTimeout, this.Command_ListDelay, Consts.Commands.ShowTimeout));
        Commands.ChatCommands.Add(new Command(Consts.Permissions.Admin.RawBroadcast, this.Command_RawBroadcast, Consts.Commands.RawBroadcast));
        Commands.ChatCommands.Add(new Command(Consts.Permissions.Admin.Sudo, this.Command_Sudo, Consts.Commands.Sudo));
        Commands.ChatCommands.Add(new Command(Consts.Permissions.Admin.ListClients, this.Command_ListConnected, Consts.Commands.ListClients));
        Commands.ChatCommands.Add(new Command(Consts.Permissions.Admin.DumpBuffer, this.Command_DumpBuffer, Consts.Commands.DumpBuffer));
        Commands.ChatCommands.Add(new Command(Consts.Permissions.Admin.TerminateSocket, this.Command_TerminateSocket, Consts.Commands.TerminateSocket));
        Commands.ChatCommands.Add(new Command(Consts.Permissions.ResetCharacter, this.Command_ResetCharacter, Consts.Commands.ResetCharacter));
        Commands.ChatCommands.Add(new Command(Consts.Permissions.Ping, this.Command_Ping, Consts.Commands.Ping));
        Commands.ChatCommands.Add(new Command(new List<string> { Consts.Permissions.Chat, Permissions.canchat }, this.Command_Chat, Consts.Commands.Chat));
        this.OnReload(new ReloadEventArgs(TSPlayer.Server));
    }
}
