/*
 *  EasyPacketDLL.cs
 *  DavidFDev
 */

using System;
using System.IO;
using EasyPacketsLib.Internals;
using Terraria.ModLoader;

namespace EasyPacketsLib;

public static class EasyPacketDLL
{
    #region Static Methods

    public static void RegisterMod(Mod mod)
    {
        // NOTE in XML: if registering multiple mods for some reason, order is important
        
        // Ensure this mod is using a DLL reference only
        if (ModContent.GetInstance<EasyPacketsLibMod>() != null)
        {
            mod.Logger.Warn($"IGNORE REGISTER MOD FOR: " + mod.Name);
            return;
        }

        if (mod.Side is not ModSide.Both)
        {
            mod.Logger.Warn($"MUST BE BOTH: " + mod.Name);
            return;
        }

        EasyPacketLoader.RegisterMod(mod);
    }

    public static void HandlePacket(BinaryReader reader, int whoAmI)
    {
        // Ensure this mod is using a DLL reference only
        if (ModContent.GetInstance<EasyPacketsLibMod>() != null)
        {
            return;
        }

        EasyPacketExtensions.HandlePacket_Internal(reader, whoAmI);
    }

    #endregion
}