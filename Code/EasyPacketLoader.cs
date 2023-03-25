/*
 *  EasyPacketLoader.cs
 *  DavidFDev
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace EasyPacketsLib;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class EasyPacketLoader : ModSystem
{
    #region Static Fields and Constants

    private static readonly Dictionary<ushort, IEasyPacket> PacketByNetId = new();
    private static readonly Dictionary<IntPtr, ushort> NetIdByPtr = new();
    private static readonly Dictionary<IntPtr, MulticastDelegate> HandlerByPtr = new();
    private static ushort _netIdCounter;

    #endregion

    #region Static Methods

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
    internal static IEasyPacket GetPacket(ushort netId)
    {
        return PacketByNetId.GetValueOrDefault(netId);
    }

    /// <summary>
    ///     Get the registered net ID of an easy packet.
    /// </summary>
    internal static ushort GetNetId<T>() where T : struct, IEasyPacket<T>
    {
        return NetIdByPtr.GetValueOrDefault(typeof(T).TypeHandle.Value);
    }

    /// <summary>
    ///     Add an easy packet handler.
    /// </summary>
    internal static void AddHandler<T>(HandleEasyPacketDelegate<T> handler) where T : struct, IEasyPacket<T>
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
    internal static void RemoveHandler<T>(HandleEasyPacketDelegate<T> handler) where T : struct, IEasyPacket<T>
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
    internal static HandleEasyPacketDelegate<T> GetHandler<T>() where T : struct, IEasyPacket<T>
    {
        return HandlerByPtr.GetValueOrDefault(typeof(T).TypeHandle.Value) as HandleEasyPacketDelegate<T>;
    }

    #endregion

    #region Methods

    public override void Load()
    {
        // Register easy packets, including from other mods
        // Order must be the same for all users, so that net ids are synced
        foreach (var mod in ModLoader.Mods
                     .Where(m => m.Side == ModSide.Both)
                     .OrderBy(m => m.Name, StringComparer.InvariantCulture))
        {
            foreach (var type in AssemblyManager.GetLoadableTypes(mod.Code)
                         .Where(t => t.IsValueType && !t.ContainsGenericParameters && typeof(IEasyPacket<>).IsAssignableFrom(t))
                         .OrderBy(t => t.FullName, StringComparer.InvariantCulture))
            {
                Register(type);
            }
        }
    }

    public override void Unload()
    {
        // Ensure the static fields are cleared
        PacketByNetId.Clear();
        NetIdByPtr.Clear();
        HandlerByPtr.Clear();
        _netIdCounter = 0;
    }

    /// <summary>
    ///     Register an easy packet.
    /// </summary>
    /// <param name="type">Type that implements <see cref="IEasyPacket{T}" />.</param>
    private void Register(Type type)
    {
        // Create a new default instance of the easy packet type
        // https://stackoverflow.com/a/1151470/20943906
        var instance = (IEasyPacket)Activator.CreateInstance(typeof(EasyPacket<>).MakeGenericType(type), true);
        if (instance == null)
        {
            throw new Exception($"Failed to register easy packet type: {type.Name}.");
        }

        // Register the created instance, assigning a unique net id
        var netId = _netIdCounter++;
        PacketByNetId.Add(netId, instance);
        NetIdByPtr.Add(type.TypeHandle.Value, netId);

        Mod.Logger.Debug($"Registered IModPacket<{type.Name}> (ID: {netId}).");
    }

    #endregion
}