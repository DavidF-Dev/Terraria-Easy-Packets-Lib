using System.Diagnostics.Contracts;
using System.IO;

namespace EasyPacketsLib;

public interface IEasyPacket<T> where T : struct, IEasyPacket<T>
{
    #region Methods

    [Pure]
    void Serialise(BinaryWriter writer);

    [Pure]
    T Deserialise(BinaryReader reader, in SenderInfo senderInfo);

    #endregion
}