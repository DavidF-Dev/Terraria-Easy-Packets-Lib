using System.IO;
using EasyPacketsLib;

public readonly struct ExamplePacket : IEasyPacket<ExamplePacket>
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

    ExamplePacket IEasyPacket<ExamplePacket>.Deserialise(BinaryReader reader, in SenderInfo senderInfo)
    {
        return new ExamplePacket(reader.ReadInt32(), reader.ReadInt32());
    }

    #endregion
}