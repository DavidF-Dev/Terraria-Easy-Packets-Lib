/*
 *  IEasyPacketHandler.cs
 *  DavidFDev
*/

using Terraria.ModLoader;

namespace EasyPacketsLib;

/// <summary>
///     Implemented by <see cref="EasyPacketHandler{T1,T2}" /> as a non-generic wrapper for handling a packet.
/// </summary>
internal interface IEasyPacketHandler
{
    #region Methods

    void Register(Mod mod);

    #endregion
}

/// <summary>
///     An easy solution for handled received easy packets.
///     Implement on a struct, preferably a readonly struct.
/// </summary>
/// <example>
///     <code>
///         public readonly struct ExamplePacketHandler : IEasyPacketHandler{ExamplePacket}
///         {
///             void IEasyPacketHandler{ExamplePacket}.Receive(in ExamplePacket packet, in SenderInfo sender, ref bool handled)
///             {
///                 sender.Mod.Logger.Debug($"X: {packet.X}, Y: {packet.Y}");
///                 handled = true;
///             }
///         }
///     </code>
/// </example>
public interface IEasyPacketHandler<T> where T : struct, IEasyPacket<T>
{
    #region Methods

    /// <summary>
    ///     Handle a received easy mod packet.
    ///     If a packet is received but is unhandled, an error is raised.
    /// </summary>
    /// <param name="packet">Packet received.</param>
    /// <param name="sender">Information regarding the sender of the packet.</param>
    /// <param name="handled">An unhandled packet will raise an error.</param>
    /// <example>
    ///     <code>
    ///         void IEasyPacketHandler{ExamplePacket}.Receive(in ExamplePacket packet, in SenderInfo sender, ref bool handled)
    ///         {
    ///             sender.Mod.Logger.Debug($"X: {packet.X}, Y: {packet.Y}");
    ///             handled = true;
    ///         }
    ///     </code>
    /// </example>
    void Receive(in T packet, in SenderInfo sender, ref bool handled);

    #endregion
}