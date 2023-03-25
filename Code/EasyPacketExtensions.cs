/*
 *  EasyPacketExtensions.cs
 *  DavidFDev
*/

using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EasyPacketsLib;

public static class EasyPacketExtensions
{
    #region Static Methods

    /// <summary>
    ///     Send an easy packet.
    ///     If a packet is received but is unhandled, an error is raised.
    /// </summary>
    /// <example>
    ///     <code>Mod.SendPacket(new ExamplePacket(10, 20));</code>
    /// </example>
    /// <param name="mod">Mod sending the packet.</param>
    /// <param name="packet">Packet instance that implements <see cref="IEasyPacket{T}" />.</param>
    /// <param name="toClient">If non-negative, this packet will only be sent to the specified client.</param>
    /// <param name="ignoreClient">If non-negative, this packet will not be sent to the specified client.</param>
    /// <param name="forward">If sending from a client, the packet will be forwarded to other clients through the server.</param>
    /// <typeparam name="T">Type that implements <see cref="IEasyPacket{T}" />.</typeparam>
    public static void SendPacket<T>(this Mod mod, in T packet, int toClient = -1, int ignoreClient = -1, bool forward = false) where T : struct, IEasyPacket<T>
    {
        forward = forward && Main.netMode == NetmodeID.MultiplayerClient;
        SendPacket(mod, in packet, (byte)Main.myPlayer, toClient, ignoreClient, forward);
    }

    /// <summary>
    ///     An easy packet handler is invoked when the packet is received.
    ///     If a packet is received but is unhandled, an error is raised.
    /// </summary>
    /// <example>
    ///     <code>
    ///         public class ExamplePacketHandler : ModSystem
    ///         {
    ///             public override void Load()
    ///             {
    ///                 Mod.AddPacketHandler{ExamplePacket}(OnExamplePacketReceived);
    ///             }
    /// 
    ///             public override void Unload()
    ///             {
    ///                 Mod.RemovePacketHandler{ExamplePacket}(OnExamplePacketReceived);
    ///             }
    /// 
    ///             private void OnExamplePacketReceived(in ExamplePacket packet, in SenderInfo senderInfo, ref bool handled)
    ///             {
    ///                 Mod.Logger.Debug($"X: {packet.X}, Y: {packet.Y}");
    ///                 handled = true;
    ///             }
    ///         }
    ///     </code>
    /// </example>
    /// <param name="mod">Mod handling the packet.</param>
    /// <param name="handler">Method handling the packet.</param>
    /// <typeparam name="T">Type that implements <see cref="IEasyPacket{T}" />.</typeparam>
    public static void AddPacketHandler<T>(this Mod mod, HandleEasyPacketDelegate<T> handler) where T : struct, IEasyPacket<T>
    {
        EasyPacketLoader.AddHandler(handler);
    }

    /// <inheritdoc cref="AddPacketHandler{T}" />
    public static void RemovePacketHandler<T>(this Mod mod, HandleEasyPacketDelegate<T> handler) where T : struct, IEasyPacket<T>
    {
        EasyPacketLoader.RemoveHandler(handler);
    }

    internal static void SendPacket<T>(this Mod mod, in T packet, byte whoAmI, int toClient, int ignoreClient, bool forward) where T : struct, IEasyPacket<T>
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            throw new Exception("SendPacket called in single-player.");
        }

        if (!EasyPacketLoader.IsRegistered<T>())
        {
            throw new Exception($"SendPacket called on an unregistered type: {typeof(T).Name}.");
        }

        // Important that the packet is sent by this mod, so that it is received correctly
        var modPacket = ModContent.GetInstance<EasyPacketsMod>().GetPacket();

        // Mod's net id is synced across server and clients
        modPacket.Write(mod.NetID);

        // Easy packet type's net id is synced across server and clients
        modPacket.Write(EasyPacketLoader.GetNetId<T>());

        // Write any additional flags
        var flags = new BitsByte {[0] = forward};
        modPacket.Write(flags);

        // Special case if the packet is to be forwarded
        if (forward)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // Send this so that the server knows who to forward the packet to
                modPacket.Write(toClient < 0 ? (byte)255 : (byte)toClient);
                modPacket.Write(ignoreClient < 0 ? (byte)255 : (byte)ignoreClient);
            }
            else
            {
                // Send this so that the receiving client knows who originally forwarded the packet
                modPacket.Write(whoAmI);
            }
        }

        // Let the easy packet serialise itself
        packet.Serialise(modPacket);

        // Finally, send the packet
        modPacket.Send(toClient, ignoreClient);
    }

    #endregion
}