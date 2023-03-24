using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace EasyPacketsLib;

public delegate void HandleModPacketDelegate<T>(in T packet, in SenderInfo senderInfo, ref bool handled) where T : struct, IEasyPacket<T>;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class EasyPacketLoader : ModSystem
{
    #region Static Fields and Constants

    private static readonly Dictionary<IntPtr, EasyPacket> ContentByPtr = new();
    private static readonly Dictionary<IntPtr, MulticastDelegate> HandlerByPtr = new();

    #endregion

    #region Static Methods

    internal static void Register<T>(Mod mod) where T : struct, IEasyPacket<T>
    {
        var instance = new EasyPacket<T>();
        mod.AddContent(instance);
        ContentByPtr.Add(typeof(T).TypeHandle.Value, instance);
        mod.Logger.Debug($"Registered IModPacket: {instance.Name}.");
    }

    internal static EasyPacket Get(IntPtr ptr)
    {
        return ContentByPtr.GetValueOrDefault(ptr);
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
        ContentByPtr.Clear();
        HandlerByPtr.Clear();
    }

    #endregion
}