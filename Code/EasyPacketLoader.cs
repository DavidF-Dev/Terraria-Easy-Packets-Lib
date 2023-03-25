/*
 *  EasyPacketLoader.cs
 *  DavidFDev
*/

using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace EasyPacketsLib;

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

    /// <summary>
    ///     Register an easy packet.
    /// </summary>
    /// <param name="mod">Mod to add the content to.</param>
    /// <param name="type">Type that implements <see cref="IEasyPacket{T}" />.</param>
    internal static void Register(Mod mod, Type type)
    {
        // Create a new default instance of the easy packet type
        // https://stackoverflow.com/a/1151470/20943906
        var instance = (EasyPacket)Activator.CreateInstance(typeof(EasyPacket<>).MakeGenericType(type), true);
        if (instance == null)
        {
            throw new Exception($"Failed to register easy packet type: {type.Name}.");
        }

        // Register the created instance
        mod.AddContent(instance);
        var netId = _netIdCounter++;
        PacketByNetId.Add(netId, instance);
        NetIdByPtr.Add(type.TypeHandle.Value, netId);

        mod.Logger.Debug($"Registered IModPacket<{type.Name}> (ID: {netId}).");
    }

    /// <summary>
    ///     Check if an easy packet is registered.
    /// </summary>
    internal static bool IsRegistered<T>() where T : struct, IEasyPacket<T>
    {
        return NetIdByPtr.ContainsKey(typeof(T).TypeHandle.Value);
    }

    /// <summary>
    ///     Get an easy packet type by its registered net ID.
    /// </summary>
    internal static EasyPacket GetPacket(ushort netId)
    {
        return PacketByNetId.GetValueOrDefault(netId);
    }

    /// <summary>
    ///     Get the registered net ID of an easy packet.
    /// </summary>
    internal static ushort GetNetId<T>() where T : struct, IEasyPacket<T>
    {
        var ptr = typeof(T).TypeHandle.Value;
        return NetIdByPtr.GetValueOrDefault(ptr);
    }

    /// <summary>
    ///     Add an easy packet handler.
    /// </summary>
    internal static void AddHandler<T>(HandleModPacketDelegate<T> handler) where T : struct, IEasyPacket<T>
    {
        var ptr = typeof(T).TypeHandle.Value;
        if (!HandlerByPtr.ContainsKey(ptr))
        {
            HandlerByPtr.Add(ptr, null);
        }

        HandlerByPtr[ptr] = (MulticastDelegate)Delegate.Combine(HandlerByPtr[ptr], handler);
    }

    /// <summary>
    ///     Remove an easy packet handler.
    /// </summary>
    internal static void RemoveHandler<T>(HandleModPacketDelegate<T> handler) where T : struct, IEasyPacket<T>
    {
        var ptr = typeof(T).TypeHandle.Value;
        if (!HandlerByPtr.ContainsKey(ptr))
        {
            return;
        }

        HandlerByPtr[ptr] = (MulticastDelegate)Delegate.Remove(HandlerByPtr[ptr], handler);
    }

    /// <summary>
    ///     Get the handler for an easy packet.
    /// </summary>
    internal static HandleModPacketDelegate<T> GetHandler<T>() where T : struct, IEasyPacket<T>
    {
        var ptr = typeof(T).TypeHandle.Value;
        return HandlerByPtr.GetValueOrDefault(ptr) as HandleModPacketDelegate<T>;
    }

    #endregion

    #region Methods

    public override void Unload()
    {
        // Ensure the static fields are cleared
        PacketByNetId.Clear();
        NetIdByPtr.Clear();
        HandlerByPtr.Clear();
        _netIdCounter = 0;
    }

    #endregion
}