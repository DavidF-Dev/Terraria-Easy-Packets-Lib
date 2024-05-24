/*
 *  EasyPacketsMod.cs
 *  DavidFDev
*/

using System.IO;
using Terraria.ModLoader;

namespace EasyPacketsLib.Internals;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class EasyPacketsLibMod : Mod
{
    #region Methods

    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        EasyPacketExtensions.HandlePacket_Internal(reader, whoAmI);
    }

    #endregion
}