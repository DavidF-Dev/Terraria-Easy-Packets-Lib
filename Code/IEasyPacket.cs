using System.Diagnostics.Contracts;
using System.IO;
using Terraria.ModLoader;

namespace EasyPacketsLib;

public interface IEasyPacket<T> : ILoadable where T : struct, IEasyPacket<T>
{
    #region Methods

    [Pure]
    void Serialise(BinaryWriter writer);

    [Pure]
    T Deserialise(BinaryReader reader, in SenderInfo senderInfo);

    void ILoadable.Load(Mod mod)
    {
        EasyPacketLoader.Register<T>(mod);
    }

    void ILoadable.Unload()
    {
    }

    #endregion
}