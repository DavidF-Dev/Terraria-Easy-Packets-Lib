using System;
using System.IO;
using Terraria.ModLoader;

namespace EasyPacketsLib;

public static class EasyPacketExtensions
{
    #region Static Methods

    public static void SendPacket<T>(this Mod mod, in T packet, int toClient = -1, int ignoreClient = -1) where T : struct, IEasyPacket<T>
    {
        var modPacket = mod.GetPacket();
        modPacket.Write(mod.NetID);
        modPacket.Write(typeof(T).TypeHandle.Value.ToInt64());
        packet.Serialise(modPacket);
        modPacket.Send(toClient, ignoreClient);
    }

    public static void AddPacketHandler<T>(this Mod mod, HandleModPacketDelegate<T> handler) where T : struct, IEasyPacket<T>
    {
        EasyPacketLoader.AddHandler(handler);
    }

    public static void RemovePacketHandler<T>(this Mod mod, HandleModPacketDelegate<T> handler) where T : struct, IEasyPacket<T>
    {
        EasyPacketLoader.RemoveHandler(handler);
    }

    internal static void ReceivePacket(BinaryReader reader, int sender)
    {
        var modNetId = reader.ReadInt16();
        var ptr = new IntPtr(reader.ReadInt64());
        EasyPacketLoader.Get(ptr).ReceivePacket(reader, new SenderInfo(null, sender));
    }

    #endregion
}