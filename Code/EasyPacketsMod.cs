using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace EasyPacketsLib;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class EasyPacketsMod : Mod
{
    #region Methods

    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        // Get the mod that sent the packet using its net id
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
            // TODO
            Logger.Error("Something went wrong.");
            return;
        }

        // Get the easy packet mod type using its net id
        var packetNetId = reader.ReadUInt16();
        var packet = EasyPacketLoader.GetPacket(packetNetId);
        if (packet == null)
        {
            // TODO
            Logger.Error("Something went wrong.");
            return;
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