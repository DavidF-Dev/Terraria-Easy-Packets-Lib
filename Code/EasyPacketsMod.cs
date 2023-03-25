using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace EasyPacketsLib;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class EasyPacketsMod : Mod
{
    #region Methods

    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        // BUG: Getting 256 for some reason; might be a 1.4.4 issue
        whoAmI = Math.Clamp(whoAmI, 0, 255);

        // Get the mod that sent the packet using its net id
        var modNetId = reader.ReadInt16();
        var sentByMod = ModNet.GetMod(modNetId);
        if (sentByMod == null)
        {
            throw new Exception($"HandlePacket received an invalid mod Net ID: {modNetId}. Could not find a mod with that Net ID.");
        }

        // Get the easy packet mod type using its net id
        var packetNetId = reader.ReadUInt16();
        var packet = EasyPacketLoader.GetPacket(packetNetId);
        if (packet == null)
        {
            throw new Exception($"HandlePacket received an invalid easy mod packet with Net ID: {packetNetId}. Could not find an easy mod packet with that Net ID.");
        }

        var flags = (BitsByte)reader.ReadByte();

        // Special case if the packet was forwarded
        byte toClient = 255;
        byte ignoreClient = 255;
        if (flags[0])
        {
            if (Main.netMode == NetmodeID.Server)
            {
                // Server knows who to forward the packet to
                toClient = reader.ReadByte();
                ignoreClient = reader.ReadByte();
            }
            else
            {
                // Client knows who originally forwarded the packet
                whoAmI = reader.ReadByte();
            }
        }

        // Let the easy packet mod type receive the packet
        packet.ReceivePacket(reader, new SenderInfo(sentByMod, (byte)whoAmI, flags, toClient, ignoreClient));
    }

    public override void Load()
    {
        // Register easy packets, including from other mods
        // Order must be the same for all users, so that net ids are synced
        var c = 0;
        foreach (var mod in ModLoader.Mods
                     .Where(m => m.Side == ModSide.Both)
                     .OrderBy(m => m.Name, StringComparer.InvariantCulture))
        {
            foreach (var type in AssemblyManager.GetLoadableTypes(mod.Code)
                         .Where(t => t.IsValueType && !t.ContainsGenericParameters && typeof(IEasyPacket<>).IsAssignableFrom(t))
                         .OrderBy(t => t.FullName, StringComparer.InvariantCulture))
            {
                EasyPacketLoader.Register(mod, type);
                c++;
            }
        }

        Logger.Debug($"Registered {c} IEasyPacket<> types.");
    }

    #endregion
}