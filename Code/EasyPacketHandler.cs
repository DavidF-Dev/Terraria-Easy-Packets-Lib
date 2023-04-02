using Terraria.ModLoader;

namespace EasyPacketsLib;

/// <summary>
///     Generic wrapper for an <see cref="IEasyPacketHandler{T}" /> type.
/// </summary>
internal readonly struct EasyPacketHandler<THandler, TPacket> : IEasyPacketHandler where THandler : struct, IEasyPacketHandler<TPacket> where TPacket : struct, IEasyPacket<TPacket>
{
    #region Methods

    void IEasyPacketHandler.Register(Mod mod)
    {
        mod.AddPacketHandler(static (in TPacket packet, in SenderInfo sender, ref bool handled) => { new THandler().Receive(in packet, in sender, ref handled); });
    }

    #endregion
}