/*
 *  ExamplePacket.cs
 *  DavidFDev
*/

using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.Localization;
using Terraria.ModLoader;

namespace EasyPacketsLib.Examples;

internal readonly struct ExamplePacket : IEasyPacket<ExamplePacket>, IEasyPacketHandler<ExamplePacket>
{
    #region Fields

    public readonly int X;
    public readonly int Y;

    #endregion

    #region Constructors

    public ExamplePacket(int x, int y)
    {
        X = x;
        Y = y;
    }

    #endregion

    #region Methods

    void IEasyPacket<ExamplePacket>.Serialise(BinaryWriter writer)
    {
        writer.Write(X);
        writer.Write(Y);
    }

    ExamplePacket IEasyPacket<ExamplePacket>.Deserialise(BinaryReader reader, in SenderInfo sender)
    {
        return new ExamplePacket(reader.ReadInt32(), reader.ReadInt32());
    }

    void IEasyPacketHandler<ExamplePacket>.Receive(in ExamplePacket packet, in SenderInfo sender, ref bool handled)
    {
        ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"{nameof(ExamplePacket)}: Received example packet from {sender.WhoAmI}: ({packet.X}, {packet.Y})."), Color.White);
        handled = true;
    }

    #endregion
}

// ReSharper disable once UnusedType.Global
internal readonly struct ExamplePacketHandler : IEasyPacketHandler<ExamplePacket>
{
    #region Methods

    void IEasyPacketHandler<ExamplePacket>.Receive(in ExamplePacket packet, in SenderInfo sender, ref bool handled)
    {
        ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"{nameof(ExamplePacketHandler)}: Received example packet from {sender.WhoAmI}: ({packet.X}, {packet.Y})."), Color.White);
        handled = true;
    }

    #endregion
}

// ReSharper disable once UnusedType.Global
internal sealed class ExamplePacketSystem : ModSystem
{
    #region Methods

    public override void Load()
    {
        Mod.AddPacketHandler<ExamplePacket>(OnExamplePacketReceived);
    }

    public override void Unload()
    {
        Mod.RemovePacketHandler<ExamplePacket>(OnExamplePacketReceived);
    }

    private void OnExamplePacketReceived(in ExamplePacket packet, in SenderInfo sender, ref bool handled)
    {
        ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"{nameof(ExamplePacketSystem)}: Received example packet from {sender.WhoAmI}: ({packet.X}, {packet.Y})."), Color.White);
        handled = true;
    }

    #endregion
}

// ReSharper disable once UnusedType.Global
internal sealed class ExamplePacketCommand : ModCommand
{
    #region Properties

    public override string Command => "expacket";

    public override CommandType Type => CommandType.Chat;

    #endregion

    #region Methods

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"Sending example packet from {Main.myPlayer}."), Color.White);
        Mod.SendPacket(new ExamplePacket(10, 25));
    }

    #endregion
}