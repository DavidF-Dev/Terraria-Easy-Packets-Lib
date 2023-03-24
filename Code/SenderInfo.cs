using Terraria.ModLoader;

namespace EasyPacketsLib;

public readonly ref struct SenderInfo
{
    #region Fields

    public readonly Mod Mod;

    public readonly int Sender;

    #endregion

    #region Constructors

    public SenderInfo(Mod mod, int sender)
    {
        Mod = mod;
        Sender = sender;
    }

    #endregion

    #region Properties

    public bool FromServer => Sender < 0;

    public bool FromClient => Sender >= 0;

    #endregion
}