/*
 *  HandleEasyPacketDelegate.cs
 *  DavidFDev
*/

namespace EasyPacketsLib;

/// <summary>
///     Handle a received easy mod packet.
///     If a packet is received but is unhandled, an error is raised.
/// </summary>
/// <param name="packet">Packet received.</param>
/// <param name="senderInfo">Information regarding the sender of the packet.</param>
/// <param name="handled">An unhandled packet will raise an error.</param>
/// <typeparam name="T">Type that implements <see cref="IEasyPacket{T}" />.</typeparam>
/// <example>
///     <code>
///         private void OnExamplePacketReceived(in ExamplePacket packet, in SenderInfo senderInfo, ref bool handled)
///         {
///             senderInfo.Mod.Logger.Debug($"X: {packet.X}, Y: {packet.Y}");
///             handled = true;
///         }
///     </code>
/// </example>
public delegate void HandleEasyPacketDelegate<T>(in T packet, in SenderInfo senderInfo, ref bool handled) where T : struct, IEasyPacket<T>;