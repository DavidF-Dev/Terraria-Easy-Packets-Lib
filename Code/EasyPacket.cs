/*
 *  EasyPacket.cs
 *  DavidFDev
*/

using System.IO;
using Terraria;
using Terraria.ID;

namespace EasyPacketsLib;

/// <summary>
///     Generic wrapper for an <see cref="IEasyPacket{T}" /> type.
///     Used to receive an incoming packet and detour it to the struct.
/// </summary>
internal sealed class EasyPacket<T> : IEasyPacket where T : struct, IEasyPacket<T>
{
    #region Methods

    public void ReceivePacket(BinaryReader reader, in SenderInfo senderInfo)
    {
        var packet = default(T).Deserialise(reader, in senderInfo);

        // Check if the packet should be automatically forwarded to clients
        if (Main.netMode == NetmodeID.Server && senderInfo.Forwarded)
        {
            senderInfo.Mod.SendPacket(in packet, senderInfo.WhoAmI, senderInfo.ToClient, senderInfo.IgnoreClient, true);
            return;
        }

        // Let any handlers handle the received packet
        var handler = EasyPacketLoader.GetHandler<T>();
        var handled = false;
        handler?.Invoke(in packet, in senderInfo, ref handled);

        if (!handled)
        {
            senderInfo.Mod.Logger.Error($"Unhandled packet: {typeof(T).Name}.");
        }
    }

    #endregion
}