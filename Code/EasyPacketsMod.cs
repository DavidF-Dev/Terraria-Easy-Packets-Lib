using System;
using System.IO;
using Terraria.ModLoader;

namespace EasyPacketsLib;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class EasyPacketsMod : Mod
{
    #region Methods

    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        // Get the mod that sent the packet
        var modNetId = reader.ReadInt16();
        Mod sentByMod = null;
        foreach (var mod in ModLoader.Mods)
        {
            if (mod.NetID != modNetId)
            {
                continue;
            }

            sentByMod = mod;
            break;
        }

        if (sentByMod == null)
        {
            Logger.Error("Something went wrong.");
            return;
        }

        // Get the easy packet mod type
        var ptr = new IntPtr(reader.ReadInt64());
        var packet = EasyPacketLoader.Get(ptr);
        if (packet == null)
        {
            Logger.Error("Something went wrong.");
            return;
        }

        // Let the easy packet mod type receive the packet
        packet.ReceivePacket(reader, new SenderInfo(sentByMod, whoAmI));
    }

    #endregion
}