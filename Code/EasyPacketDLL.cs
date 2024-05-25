/*
 *  EasyPacketDLL.cs
 *  DavidFDev
 */

using System.IO;
using EasyPacketsLib.Internals;
using Terraria.ModLoader;

namespace EasyPacketsLib;

public static class EasyPacketDLL
{
    #region Static Methods

    public static void RegisterMod(Mod mod)
    {
        // TODO: Does this still work if the library mod is enabled?
        // TODO: Will the library mod detect types in this mod when it shouldn't?
        
        // Only register the mod if using a DLL reference
        if (ModContent.GetInstance<EasyPacketsLibMod>() != null)
        {
            return;
        }

        EasyPacketLoader.RegisterMod(mod);
    }

    public static void HandlePacket(BinaryReader reader, int whoAmI)
    {
        // Only register the mod if using a DLL reference
        if (ModContent.GetInstance<EasyPacketsLibMod>() != null)
        {
            return;
        }

        EasyPacketExtensions.HandlePacket_Internal(reader, whoAmI);
    }

    #endregion
}