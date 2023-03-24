using Terraria.ModLoader;

namespace EasyPacketsLib;

public static class EasyPacketExtensions
{
    #region Static Methods

    /// <summary>
    ///     Send an easy packet. Use AddPacketHandler to handle receiving the packet.
    /// </summary>
    /// <param name="mod">Mod sending the packet.</param>
    /// <param name="packet">Packet instance that implements <see cref="IEasyPacket{T}" />.</param>
    /// <param name="toClient">If non-negative, this packet will only be sent to the specified client.</param>
    /// <param name="ignoreClient">If non-negative, this packet will not be sent to the specified client.</param>
    /// <typeparam name="T">Type that implements <see cref="IEasyPacket{T}" />.</typeparam>
    public static void SendPacket<T>(this Mod mod, in T packet, int toClient = -1, int ignoreClient = -1) where T : struct, IEasyPacket<T>
    {
        var modPacket = ModContent.GetInstance<EasyPacketsMod>().GetPacket();
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

    #endregion
}