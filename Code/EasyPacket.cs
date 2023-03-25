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

    public void ReceivePacket(BinaryReader reader, in SenderInfo sender)
    {
        var packet = default(T).Deserialise(reader, in sender);

        // Check if the packet should be automatically forwarded to clients
        if (Main.netMode == NetmodeID.Server && sender.Forwarded)
        {
            sender.Mod.SendPacket(in packet, sender.WhoAmI, sender.ToClient, sender.IgnoreClient, true);
            return;
        }

        // Let any handlers handle the received packet
        var handler = EasyPacketLoader.GetHandler<T>();
        var handled = false;
        handler?.Invoke(in packet, in sender, ref handled);

        if (!handled)
        {
            sender.Mod.Logger.Error($"Unhandled packet: {typeof(T).Name}.");
        }
    }

    #endregion
}