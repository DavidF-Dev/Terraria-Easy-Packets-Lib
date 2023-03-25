using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EasyPacketsLib;

internal abstract class EasyPacket : ModType
{
    #region Methods

    protected internal abstract void ReceivePacket(BinaryReader reader, in SenderInfo senderInfo);

    protected sealed override void Register()
    {
    }

    #endregion
}

internal sealed class EasyPacket<T> : EasyPacket where T : struct, IEasyPacket<T>
{
    #region Properties

    public override string Name => typeof(T).Name;

    #endregion

    #region Methods

    protected internal override void ReceivePacket(BinaryReader reader, in SenderInfo senderInfo)
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
            Mod.Logger.Error($"Unhandled packet: {Name}.");
        }
    }

    #endregion
}