using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace EasyPacketsLib;

public delegate void HandleModPacketDelegate<T>(in T packet, in SenderInfo senderInfo, ref bool handled) where T : struct, IEasyPacket<T>;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class EasyPacketLoader : ModSystem
{
    #region Static Fields and Constants

    private static readonly Dictionary<ushort, EasyPacket> PacketByNetId = new();
    private static readonly Dictionary<IntPtr, ushort> NetIdByPtr = new();
    private static readonly Dictionary<IntPtr, MulticastDelegate> HandlerByPtr = new();
    private static ushort _netIdCounter;

    #endregion

    #region Static Methods

    internal static void Register(Mod mod, Type type)
    {
        // Create a new default instance of the easy packet type
        var instance = (EasyPacket)Activator.CreateInstance(typeof(EasyPacket<>).MakeGenericType(type), true);
        if (instance == null)
        {
            // TODO
            return;
        }

        // Register the created instance
        mod.AddContent(instance);
        var netId = _netIdCounter++;
        PacketByNetId.Add(netId, instance);
        NetIdByPtr.Add(type.TypeHandle.Value, netId);

        mod.Logger.Debug($"Registered IModPacket<{type.Name}> (ID: {netId}).");
    }

    internal static EasyPacket GetPacket(ushort netId)
    {
        return PacketByNetId.GetValueOrDefault(netId);
    }

    internal static ushort GetNetId<T>() where T : struct, IEasyPacket<T>
    {
        return NetIdByPtr.GetValueOrDefault(typeof(T).TypeHandle.Value);
    }

    internal static void AddHandler<T>(HandleModPacketDelegate<T> handler) where T : struct, IEasyPacket<T>
    {
        var ptr = typeof(T).TypeHandle.Value;
        if (!HandlerByPtr.ContainsKey(ptr))
        {
            HandlerByPtr.Add(ptr, null);
        }

        HandlerByPtr[ptr] = (MulticastDelegate)Delegate.Combine(HandlerByPtr[ptr], handler);
    }

    internal static void RemoveHandler<T>(HandleModPacketDelegate<T> handler) where T : struct, IEasyPacket<T>
    {
        var ptr = typeof(T).TypeHandle.Value;
        if (!HandlerByPtr.ContainsKey(ptr))
        {
            return;
        }

        HandlerByPtr[ptr] = (MulticastDelegate)Delegate.Remove(HandlerByPtr[ptr], handler);
    }

    internal static HandleModPacketDelegate<T> GetHandler<T>() where T : struct, IEasyPacket<T>
    {
        var ptr = typeof(T).TypeHandle.Value;
        return HandlerByPtr.GetValueOrDefault(ptr) as HandleModPacketDelegate<T>;
    }

    #endregion

    #region Methods

    public override void Unload()
    {
        PacketByNetId.Clear();
        NetIdByPtr.Clear();
        HandlerByPtr.Clear();
        _netIdCounter = 0;
    }

    #endregion
}