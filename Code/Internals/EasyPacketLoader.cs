/*
 *  EasyPacketLoader.cs
 *  DavidFDev
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace EasyPacketsLib.Internals;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class EasyPacketLoader : ModSystem
{
    #region Static Fields and Constants

    private static readonly Dictionary<ushort, IEasyPacket> PacketByNetId = new();
    private static readonly Dictionary<IntPtr, ushort> NetIdByPtr = new();
    private static readonly Dictionary<IntPtr, MulticastDelegate> HandlerByPtr = new();
    private static readonly HashSet<Mod> RegisteredMods = [];
    private static readonly string EasyPacketFullName;
    private static readonly string EasyPacketHandlerFullName;

    #endregion

    #region Static Methods

    /// <summary>
    ///     Check if an easy packet is registered.
    /// </summary>
    public static bool IsRegistered<T>() where T : struct, IEasyPacket<T>
    {
        return NetIdByPtr.ContainsKey(typeof(T).TypeHandle.Value);
    }

    /// <summary>
    ///     Get an easy packet type by its registered net ID.
    /// </summary>
    public static IEasyPacket GetPacket(ushort netId)
    {
        return PacketByNetId.GetValueOrDefault(netId);
    }

    /// <summary>
    ///     Get the registered net ID of an easy packet.
    /// </summary>
    public static ushort GetNetId<T>() where T : struct, IEasyPacket<T>
    {
        return NetIdByPtr.GetValueOrDefault(typeof(T).TypeHandle.Value);
    }

    /// <summary>
    ///     Add an easy packet handler.
    /// </summary>
    public static void AddHandler<T>(HandleEasyPacketDelegate<T> handler) where T : struct, IEasyPacket<T>
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
    public static void RemoveHandler<T>(HandleEasyPacketDelegate<T> handler) where T : struct, IEasyPacket<T>
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
    public static HandleEasyPacketDelegate<T> GetHandler<T>() where T : struct, IEasyPacket<T>
    {
        return HandlerByPtr.GetValueOrDefault(typeof(T).TypeHandle.Value) as HandleEasyPacketDelegate<T>;
    }

    /// <summary>
    ///     Register easy packets and handlers of the provided mod.
    /// </summary>
    public static void RegisterMod(Mod mod)
    {
        if (!RegisteredMods.Add(mod))
        {
            // Already registered
            return;
        }

        // The interface is checked by name (not type), so we must also check which assembly it is defined in
        var assembly = ModContent.GetInstance<EasyPacketsLibMod>()?.Code ?? Assembly.GetExecutingAssembly();

        // Register easy packets
        var loadableTypes = AssemblyManager.GetLoadableTypes(mod.Code);
        foreach (var type in loadableTypes
                     .Where(t => t.IsValueType && !t.ContainsGenericParameters && t.GetInterface(EasyPacketFullName)?.Assembly == assembly)
                     .OrderBy(t => t.FullName, StringComparer.InvariantCulture))
        {
            RegisterPacket(mod, type);
        }

        // Register handlers
        foreach (var type in loadableTypes
                     .Where(t => t.IsValueType && !t.ContainsGenericParameters && t.GetInterface(EasyPacketHandlerFullName)?.Assembly == assembly)
                     .OrderBy(t => t.FullName, StringComparer.InvariantCulture))
        {
            RegisterHandler(mod, type);
        }
    }

    /// <summary>
    ///     Register an easy packet.
    /// </summary>
    /// <param name="mod">Mod that defined the easy packet.</param>
    /// <param name="type">Type that implements <see cref="IEasyPacket{T}" />.</param>
    private static void RegisterPacket(Mod mod, Type type)
    {
        // Ensure the interface generic argument matches the type implementing it
        // This is not enforced by the code, so we must check it here and explain why in detail
        var genericArg = type.GetInterface(EasyPacketFullName)!.GetGenericArguments()[0];
        if (genericArg != type)
        {
            throw new Exception($"Failed to register easy packet type: {type.Name}." +
                                $"\nActual:\n   struct {type.Name} : IEasyPacket<[c/{Color.Red.Hex3()}:{genericArg.Name}]>" +
                                $"\nExpected:\n   struct {type.Name} : IEasyPacket<[c/{Color.Green.Hex3()}:{type.Name}]>" +
                                "\nPlease fix the struct definition so that the interface generic argument matches the type implementing it." +
                                $"\nDefined in mod: [c/{Color.Yellow.Hex3()}:{mod.Name}].\n");
        }

        // Create a new default instance of the easy packet type
        // https://stackoverflow.com/a/1151470/20943906
        var instance = (IEasyPacket)Activator.CreateInstance(typeof(EasyPacket<>).MakeGenericType(genericArg), true);
        if (instance == null)
        {
            throw new Exception($"Failed to register easy packet type: {type.Name}.");
        }

        // Register the created instance, assigning a unique net id
        var netId = NetEasyPacketCount++;
        PacketByNetId.Add(netId, instance);
        NetIdByPtr.Add(type.TypeHandle.Value, netId);

        (ModContent.GetInstance<EasyPacketsLibMod>() ?? mod).Logger.Debug($"Registered IEasyPacket<{type.Name}> (Mod: {mod.Name}, ID: {netId})");
    }

    /// <summary>
    ///     Register an easy packet handler.
    /// </summary>
    /// <param name="mod">Mod that defined the easy packet handler.</param>
    /// <param name="type">Type that implements <see cref="IEasyPacketHandler{T}" />.</param>
    private static void RegisterHandler(Mod mod, Type type)
    {
        // Create a new default instance of the easy packet handler type, and allow it to register it instead
        var instance = (IEasyPacketHandler)Activator.CreateInstance(typeof(EasyPacketHandler<,>).MakeGenericType(type, type.GetInterface(EasyPacketHandlerFullName)!.GetGenericArguments()[0]), true);
        if (instance == null)
        {
            throw new Exception($"Failed to register easy packet type: {type.Name}.");
        }

        // The instance is thrown away because its only purpose is to register itself as a handler
        instance.Register(mod);

        (ModContent.GetInstance<EasyPacketsLibMod>() ?? mod).Logger.Debug($"Registered IEasyPacketHandler<{type.Name}> (Mod: {mod.Name})");
    }

    #endregion

    #region Constructors

    static EasyPacketLoader()
    {
        // Cache the full interface type definitions to be used during loading
        EasyPacketFullName = typeof(IEasyPacket<>).GetGenericTypeDefinition().FullName;
        EasyPacketHandlerFullName = typeof(IEasyPacketHandler<>).GetGenericTypeDefinition().FullName;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Total number of easy packets registered across all registered mods.
    /// </summary>
    public static ushort NetEasyPacketCount { get; private set; }

    #endregion

    #region Methods

    public override void Load()
    {
        // Register loaded mods; order must be the same for all users, so that net ids are synced
        foreach (var mod in ModLoader.Mods.Where(static m => m.Side is ModSide.Both).OrderBy(static m => m.Name, StringComparer.InvariantCulture))
        {
#if RELEASE
            // Ignore example packets
            if (mod == Mod)
            {
                continue;
            }
#endif

            RegisterMod(mod);
        }
    }

    public override void Unload()
    {
        // Ensure the static fields are cleared
        PacketByNetId.Clear();
        NetIdByPtr.Clear();
        HandlerByPtr.Clear();
        RegisteredMods.Clear();
        NetEasyPacketCount = 0;
    }

    #endregion
}