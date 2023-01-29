﻿using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Concurrent;
using System.Net;
using System.Runtime.CompilerServices;
using TerrariaApi.Server;

namespace Chireiden.TShock.Omni;

public partial class Plugin : TerrariaPlugin
{
    internal static class Mitigations
    {
        internal static bool HandleInventorySlotPE(byte player, Span<byte> data)
        {
            if (data.Length != 8)
            {
                return true;
            }

            if (data[0] != player)
            {
                return true;
            }

            var slot = BitConverter.ToInt16(data.Slice(1, 2));
            var stack = BitConverter.ToInt16(data.Slice(3, 2));
            var prefix = data[5];
            var type = BitConverter.ToInt16(data.Slice(6, 2));

            var p = Terraria.Main.player[player];
            var existingItem = slot switch
            {
                short when Terraria.ID.PlayerItemSlotID.Loadout3_Dye_0 + 10 > slot && slot >= Terraria.ID.PlayerItemSlotID.Loadout3_Dye_0
                    => p.Loadouts[2].Dye[slot - Terraria.ID.PlayerItemSlotID.Loadout3_Dye_0],
                short when Terraria.ID.PlayerItemSlotID.Loadout3_Dye_0 > slot && slot >= Terraria.ID.PlayerItemSlotID.Loadout3_Armor_0
                    => p.Loadouts[2].Armor[slot - Terraria.ID.PlayerItemSlotID.Loadout3_Armor_0],
                short when Terraria.ID.PlayerItemSlotID.Loadout3_Armor_0 > slot && slot >= Terraria.ID.PlayerItemSlotID.Loadout2_Dye_0
                    => p.Loadouts[1].Dye[slot - Terraria.ID.PlayerItemSlotID.Loadout2_Dye_0],
                short when Terraria.ID.PlayerItemSlotID.Loadout2_Dye_0 > slot && slot >= Terraria.ID.PlayerItemSlotID.Loadout2_Armor_0
                    => p.Loadouts[1].Armor[slot - Terraria.ID.PlayerItemSlotID.Loadout2_Armor_0],
                short when Terraria.ID.PlayerItemSlotID.Loadout2_Armor_0 > slot && slot >= Terraria.ID.PlayerItemSlotID.Loadout1_Dye_0
                    => p.Loadouts[0].Dye[slot - Terraria.ID.PlayerItemSlotID.Loadout1_Dye_0],
                short when Terraria.ID.PlayerItemSlotID.Loadout1_Dye_0 > slot && slot >= Terraria.ID.PlayerItemSlotID.Loadout1_Armor_0
                    => p.Loadouts[0].Armor[slot - Terraria.ID.PlayerItemSlotID.Loadout1_Armor_0],
                short when Terraria.ID.PlayerItemSlotID.Loadout1_Armor_0 > slot && slot >= Terraria.ID.PlayerItemSlotID.Bank4_0
                    => p.bank4.item[slot - Terraria.ID.PlayerItemSlotID.Bank4_0],
                short when Terraria.ID.PlayerItemSlotID.Bank4_0 > slot && slot >= Terraria.ID.PlayerItemSlotID.Bank3_0
                    => p.bank3.item[slot - Terraria.ID.PlayerItemSlotID.Bank3_0],
                short when Terraria.ID.PlayerItemSlotID.Bank3_0 > slot && slot >= Terraria.ID.PlayerItemSlotID.TrashItem
                    => p.trashItem,
                short when Terraria.ID.PlayerItemSlotID.TrashItem > slot && slot >= Terraria.ID.PlayerItemSlotID.Bank2_0
                    => p.bank2.item[slot - Terraria.ID.PlayerItemSlotID.Bank2_0],
                short when Terraria.ID.PlayerItemSlotID.Bank2_0 > slot && slot >= Terraria.ID.PlayerItemSlotID.Bank1_0
                    => p.bank.item[slot - Terraria.ID.PlayerItemSlotID.Bank1_0],
                short when Terraria.ID.PlayerItemSlotID.Bank1_0 > slot && slot >= Terraria.ID.PlayerItemSlotID.MiscDye0
                    => p.miscDyes[slot - Terraria.ID.PlayerItemSlotID.MiscDye0],
                short when Terraria.ID.PlayerItemSlotID.MiscDye0 > slot && slot >= Terraria.ID.PlayerItemSlotID.Misc0
                    => p.miscEquips[slot - Terraria.ID.PlayerItemSlotID.Misc0],
                short when Terraria.ID.PlayerItemSlotID.Misc0 > slot && slot >= Terraria.ID.PlayerItemSlotID.Dye0
                    => p.dye[slot - Terraria.ID.PlayerItemSlotID.Dye0],
                short when Terraria.ID.PlayerItemSlotID.Dye0 > slot && slot >= Terraria.ID.PlayerItemSlotID.Armor0
                    => p.armor[slot - Terraria.ID.PlayerItemSlotID.Armor0],
                short when Terraria.ID.PlayerItemSlotID.Armor0 > slot && slot >= Terraria.ID.PlayerItemSlotID.Inventory0
                    => p.inventory[slot - Terraria.ID.PlayerItemSlotID.Inventory0],
                _ => throw new System.Runtime.CompilerServices.SwitchExpressionException($"Unexpected slot: {slot}")
            };

            if (existingItem != null)
            {
                if ((existingItem.netID == 0 || existingItem.stack == 0) && (type == 0 || stack == 0))
                {
                    return true;
                }
                if (existingItem.netID == type && existingItem.stack == stack && existingItem.prefix == prefix)
                {
                    return true;
                }
            }
            return false;
        }
    }

    private readonly bool[] _BuffUpdateNPC = new bool[Terraria.Main.npc.Length];

    private void Hook_Mitigation_GetData(object? sender, OTAPI.Hooks.MessageBuffer.GetDataEventArgs args)
    {
        if (args.Result == OTAPI.HookResult.Cancel)
        {
            return;
        }

        var mitigation = this.config.Mitigation;
        if (!mitigation.Enabled)
        {
            return;
        }

        switch (args.PacketId)
        {
            case (int) PacketTypes.PlayerSlot:
            {
                if (!mitigation.InventorySlotPE)
                {
                    break;
                }
                var index = args.Instance.whoAmI;
                if (Mitigations.HandleInventorySlotPE((byte) index, args.Instance.readBuffer.AsSpan(args.ReadOffset, args.Length - 1)))
                {
                    args.Result = OTAPI.HookResult.Cancel;
                    this.Statistics.MitigationSlotPE++;
                    var player = TShockAPI.TShock.Players[index];
                    if (player == null)
                    {
                        return;
                    }
                    var value = this[player].DetectPE;
                    this[player].DetectPE = value + 1;
                    if (value % 500 == 0)
                    {
                        var currentLoadoutIndex = Terraria.Main.player[index].CurrentLoadoutIndex;
                        Terraria.NetMessage.TrySendData((int) PacketTypes.SyncLoadout, -1, -1, null, index, (currentLoadoutIndex + 1) % 3);
                        Terraria.NetMessage.TrySendData((int) PacketTypes.SyncLoadout, -1, -1, null, index, currentLoadoutIndex);
                        this[player].IsPE = true;
                    }
                }
                else
                {
                    this.Statistics.MitigationSlotPEAllowed++;
                }
                break;
            }
            case (int) PacketTypes.EffectHeal:
            {
                if (!mitigation.PotionSicknessPE)
                {
                    break;
                }
                var index = args.Instance.whoAmI;
                if (args.Instance.readBuffer[args.ReadOffset] != index)
                {
                    args.Result = OTAPI.HookResult.Cancel;
                    break;
                }
                if (Terraria.Main.player[index].inventory[Terraria.Main.player[index].selectedItem].potion)
                {
                    var amount = BitConverter.ToInt16(args.Instance.readBuffer.AsSpan(args.ReadOffset + 1, 2));
                    this[index].PendingRevertHeal = amount;
                }
                break;
            }
            case (int) PacketTypes.PlayerBuff:
            {
                if (!mitigation.PotionSicknessPE)
                {
                    break;
                }
                var index = args.Instance.whoAmI;
                if (args.Instance.readBuffer[args.ReadOffset] != index)
                {
                    args.Result = OTAPI.HookResult.Cancel;
                    break;
                }
                var buffcount = (args.Length - 1) / 2;
                for (var i = 0; i < buffcount; i++)
                {
                    var buff = BitConverter.ToInt16(args.Instance.readBuffer.AsSpan(args.ReadOffset + 1 + (i * 2), 2));
                    if (buff == Terraria.ID.BuffID.PotionSickness)
                    {
                        this[index].PendingRevertHeal = 0;
                    }
                }
                break;
            }
            case (int) PacketTypes.ClientSyncedInventory:
            {
                if (!mitigation.PotionSicknessPE)
                {
                    break;
                }
                var index = args.Instance.whoAmI;
                var pending = this[index].PendingRevertHeal;
                if (pending > 0)
                {
                    this.Statistics.MitigationRejectedSicknessHeal++;
                    this[index].PendingRevertHeal = 0;
                    Terraria.Main.player[index].statLife -= pending;
                    Terraria.NetMessage.TrySendData((int) PacketTypes.PlayerHp, -1, -1, null, index);
                }
                break;
            }
            case (int) PacketTypes.PlayerUpdate:
            {
                if (!mitigation.SwapWhileUsePE)
                {
                    break;
                }
                var index = args.Instance.whoAmI;
                if (args.Instance.readBuffer[args.ReadOffset] != index)
                {
                    args.Result = OTAPI.HookResult.Cancel;
                    break;
                }
                Terraria.BitsByte control = args.Instance.readBuffer[args.ReadOffset + 1];
                var selectedItem = args.Instance.readBuffer[args.ReadOffset + 5];
                if (Terraria.Main.player[index].controlUseItem && control[5] && selectedItem != Terraria.Main.player[index].selectedItem)
                {
                    this.Statistics.MitigationRejectedSwapWhileUse++;
                    args.Result = OTAPI.HookResult.Cancel;
                    Terraria.Main.player[index].controlUseItem = false;
                    Terraria.NetMessage.TrySendData((int) PacketTypes.PlayerUpdate, -1, -1, null, index);
                    break;
                }
                break;
            }
            case (int) PacketTypes.LoadNetModule:
            {
                var type = BitConverter.ToUInt16(args.Instance.readBuffer.AsSpan(args.ReadOffset, 2));
                if (type == Terraria.Net.NetManager.Instance.GetId<Terraria.GameContent.NetModules.NetTextModule>())
                {
                    var index = args.Instance.whoAmI;
                    var player = TShockAPI.TShock.Players[index];
                    if (player == null)
                    {
                        break;
                    }

                    for (var i = 0; i < this.config.Mitigation.ChatSpamRestrict.Count; i++)
                    {
                        var limiter = this.config.Mitigation.ChatSpamRestrict[i];
                        var tat = Math.Max(this._updateCounter, this[player].ChatSpamRestrict[i]) + limiter.RateLimit;
                        if (tat > this._updateCounter + limiter.Maximum)
                        {
                            this.Statistics.MitigationRejectedChat++;
                            args.Result = OTAPI.HookResult.Cancel;
                            // FIXME: TSAPI is not respecting args.Result, so we have to craft invalid packet.
                            args.PacketId = byte.MaxValue;
                            break;
                        }
                        this[player].ChatSpamRestrict[i] = tat;
                    }
                }
                break;
            }
            default:
                break;
        }
    }

    private void Hook_Mitigation_NpcAddBuff(object? sender, TShockAPI.GetDataHandlers.NPCAddBuffEventArgs args)
    {
        if (args.Handled)
        {
            return;
        }

        var mitigation = this.config.Mitigation;
        if (!mitigation.Enabled)
        {
            return;
        }

        if (!mitigation.NpcUpdateBuffRateLimit)
        {
            return;
        }

        this._BuffUpdateNPC[args.ID] = true;
        Terraria.Main.npc[args.ID].AddBuff(args.Type, args.Time, quiet: true);
        args.Handled = true;
    }

    private void Hook_Mitigation_GameUpdate(EventArgs args)
    {
        var mitigation = this.config.Mitigation;
        if (!mitigation.Enabled)
        {
            return;
        }

        if (mitigation.NpcUpdateBuffRateLimit)
        {
            if (this._updateCounter % 10 == 0)
            {
                for (var i = 0; i < Terraria.Main.npc.Length; i++)
                {
                    if (this._BuffUpdateNPC[i])
                    {
                        Terraria.NetMessage.TrySendData((int) PacketTypes.NpcUpdateBuff, -1, -1, null, i);
                        this._BuffUpdateNPC[i] = false;
                    }
                }
            }
        }
    }

    private static readonly bool ShouldSuppressTitle = !System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)
        && !(Environment.GetEnvironmentVariable("TERM")?.Contains("xterm") ?? false);
    private void Detour_Mitigation_SetTitle(Action<TShockAPI.Utils, bool> orig, TShockAPI.Utils self, bool empty)
    {
        if (this.config.Mitigation.Enabled && this.config.Mitigation.SuppressTitle)
        {
            if (ShouldSuppressTitle)
            {
                return;
            }
        }

        orig(self, empty);
    }

    internal class ConnectionStore
    {
        public ConcurrentDictionary<string, Connection> Connections { get; } = new ConcurrentDictionary<string, Connection>();
        public ConditionalWeakTable<Terraria.Net.Sockets.ISocket, Float64Object> ConnectTime { get; } = new ConditionalWeakTable<Terraria.Net.Sockets.ISocket, Float64Object>();

        internal class Connection
        {
            public required IPAddress Address { get; set; }
            public ConcurrentDictionary<int, double> Limit { get; } = new ConcurrentDictionary<int, double>();
        }

        public void PurgeCache()
        {
            var alive = this.ConnectTime
                .Select((kv) => kv.Key.GetRemoteAddress() is Terraria.Net.TcpAddress tcpa ? tcpa.Address : null)
                .Where((a) => a != null)
                .Select(a => a!)
                .ToArray();
            var tbr = this.Connections
                .Where((kv) => !alive.Any(a => a.Equals(kv.Value.Address)))
                .Select((kv) => kv.Key)
                .ToList();
            foreach (var k in tbr)
            {
                this.Connections.TryRemove(k, out _);
            }
        }
    }

    internal class Float64Object
    {
        public double Value;
    }

    private readonly ConnectionStore _connPool = new ConnectionStore();
    private void Hook_Mitigation_OnConnectionAccepted(On.Terraria.Netplay.orig_OnConnectionAccepted orig, Terraria.Net.Sockets.ISocket client)
    {
        var mitigation = this.config.Mitigation;
        if (mitigation.Enabled && client.GetRemoteAddress() is Terraria.Net.TcpAddress tcpa)
        {
            if (mitigation.ConnectionLimit.Count != 0 && Utils.PublicIPv4Address(tcpa.Address))
            {
                var addrs = tcpa.Address.ToString();
                var cd = this._connPool.Connections.GetOrAdd(addrs, (_k) => new ConnectionStore.Connection
                {
                    Address = tcpa.Address,
                });
                var time = new TimeSpan(DateTime.Now.Ticks).TotalSeconds;
                this._connPool.ConnectTime.Add(client, new Float64Object
                {
                    Value = time,
                });
                for (var i = 0; i < mitigation.ConnectionLimit.Count; i++)
                {
                    var limiter = mitigation.ConnectionLimit[i];
                    var lb = time + limiter.RateLimit;
                    if (cd.Limit.AddOrUpdate(i, (_k) => lb, (k, v) =>
                    {
                        var tat = lb = Math.Max(v + limiter.RateLimit, lb);
                        return tat > time + limiter.Maximum ? v : tat;
                    }) != lb)
                    {
                        Interlocked.Increment(ref this.Statistics.MitigationRejectedConnection);
                        client.Close();
                        TShockAPI.TShock.Log.ConsoleInfo($"Connection from {tcpa.Address} ({tcpa.Port}) rejected due to connection limit.");
                        return;
                    }
                }
                this.CheckConnectionTimeout();
            }
        }
        orig(client);
    }

    private void CheckConnectionTimeout()
    {
        var count = 0;
        for (var i = 0; i < Terraria.Main.maxNetPlayers; i++)
        {
            if (Terraria.Netplay.Clients[i].IsConnected())
            {
                count += 1;
            }
        }

        if (count <= Terraria.Main.maxNetPlayers * 0.6)
        {
            return;
        }

        for (var i = 0; i < Terraria.Main.maxNetPlayers; i++)
        {
            if (Terraria.Netplay.Clients[i].IsConnected()
                && Terraria.Netplay.Clients[i].Socket.GetRemoteAddress() is Terraria.Net.TcpAddress tcpa)
            {
                if (!this._connPool.ConnectTime.TryGetValue(Terraria.Netplay.Clients[i].Socket, out var ct))
                {
                    throw new Exception("Connection time not found");
                }

                var time = new TimeSpan(DateTime.Now.Ticks).TotalSeconds;

                foreach (var (state, timeout) in this.config.Mitigation.ConnectionStateTimeout)
                {
                    var elapsed = time - ct.Value;
                    if (Terraria.Netplay.Clients[i].State == state && elapsed > timeout)
                    {
                        Interlocked.Increment(ref this.Statistics.MitigationTerminatedConnection);
                        Terraria.Netplay.Clients[i].Socket.Close();
                        TShockAPI.TShock.Log.ConsoleInfo($"Connection from {tcpa.Address} ({tcpa.Port}, state {state} for {Math.Round(time - ct.Value, 1):G}s) disconnected due to connection state timeout.");
                        break;
                    }
                }
            }
        }

        this._connPool.PurgeCache();
    }

    private void ILHook_Mitigation_DisabledInvincible(ILContext context)
    {
        var mitigation = this.config.Mitigation;
        if (mitigation.Enabled)
        {
            var cursor = new ILCursor(context);
            cursor.GotoNext(MoveType.After, (i) => i.MatchCallvirt<TShockAPI.TSPlayer>(nameof(TShockAPI.TSPlayer.IsBeingDisabled)));
            switch (mitigation.DisabledDamageHandler)
            {
                case Config.MitigationSettings.DisabledDamageAction.AsIs:
                    break;
                case Config.MitigationSettings.DisabledDamageAction.Preset:
                case Config.MitigationSettings.DisabledDamageAction.Hurt:
                    cursor.Emit(OpCodes.Pop);
                    cursor.Emit(OpCodes.Ldc_I4_0);
                    break;
                case Config.MitigationSettings.DisabledDamageAction.Ghost:
                    break;
            }
        }
    }
}
