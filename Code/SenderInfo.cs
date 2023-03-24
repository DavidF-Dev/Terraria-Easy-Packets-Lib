using Terraria.ModLoader;

namespace EasyPacketsLib;

/// <summary>
///     Information regarding the sender of an easy packet that has been received.
/// </summary>
public readonly ref struct SenderInfo
{
    #region Fields

    /// <summary>
    ///     Mod that sent the packet.
    /// </summary>
    public readonly Mod Mod;

    /// <summary>
    ///     Index of the player that sent the packet, if sent by a client.
    /// </summary>
    public readonly int WhoAmI;

    #endregion

    #region Constructors

    public SenderInfo(Mod mod, int whoAmI)
    {
        Mod = mod;
        WhoAmI = whoAmI;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Packet was sent from the server, and is being received on this client.
    /// </summary>
    public bool FromServer => WhoAmI == 255;

    /// <summary>
    ///     Packet was sent from a client, and is being received on this server.
    /// </summary>
    public bool FromClient => WhoAmI < 255;

    #endregion
}