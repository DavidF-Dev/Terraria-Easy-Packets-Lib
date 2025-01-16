# Easy Packets Library
[![Release](https://img.shields.io/github/v/release/DavidF-Dev/Terraria-Easy-Packets-Lib?style=flat-square)](https://github.com/DavidF-Dev/Terraria-Easy-Packets-Lib/releases/latest)
[![Downloads](https://img.shields.io/steam/downloads/2952511711?style=flat-square)](https://steamcommunity.com/sharedfiles/filedetails/?id=2952511711)
[![File Size](https://img.shields.io/steam/size/2952511711?style=flat-square)](https://steamcommunity.com/sharedfiles/filedetails/?id=2952511711)
[![Issues](https://img.shields.io/github/issues/DavidF-Dev/Terraria-Easy-Packets-Lib?style=flat-square)](https://github.com/DavidF-Dev/Terraria-Easy-Packets-Lib/issues)
[![License](https://img.shields.io/github/license/DavidF-Dev/Terraria-Easy-Packets-Lib?style=flat-square)](https://github.com/DavidF-Dev/Terraria-Easy-Packets-Lib/blob/main/LICENSE.md)

A Terraria tModLoader library mod that provides an easy solution for sending/receiving `ModPackets` with custom data.

## Usage
### Requirements
- tModLoader for `1.4.4`.
- `Side = Both` in `build.txt` (default if not specified).

### Referencing the library
- Add `modReferences = EasyPacketsLib` to your mod's `build.txt` file.
- Add `EasyPacketsLib.dll` to your project as a reference (download from [Releases](https://github.com/DavidF-Dev/Terraria-Easy-Packets-Lib/releases/latest)).
- Subscribe to the library mod on the [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=2952511711).

### Defining the packet
```csharp
public readonly struct ExamplePacket : IEasyPacket<ExamplePacket>
{
    public readonly int X;
    public readonly int Y;

    public ExamplePacket(int x, int y)
    {
        X = x;
        Y = y;
    }

    void IEasyPacket<ExamplePacket>.Serialise(BinaryWriter writer)
    {
        writer.Write(X);
        writer.Write(Y);
    }

    ExamplePacket IEasyPacket<ExamplePacket>.Deserialise(BinaryReader reader, in SenderInfo sender)
    {
        return new ExamplePacket(reader.ReadInt32(), reader.ReadInt32());
    }
}
```
- `Serialise`: Serialise the packet data using the provided writer.
- `Deserialise`: Deserialise the packet data using the provided reader.

### Sending the packet
```csharp
Mod.SendPacket(new ExamplePacket(1, 2), toClient: -1, ignoreClient: -1, forward: false);
```
- `toClient`: If non-negative, the packet will only be sent to the specified client.
- `ignoreClient`: If non-negative, the packet will not be sent to the specified client.
- `forward`: If sending from a client, the packet will be forwarded to other clients through the server.

### Receiving the packet

The packet can be received in two ways, both of which use the following parameters:
- `packet`: Packet data received.
- `sender`: Information regarding the sender of the packet.
- `handled`: An unhandled packet will raise an error.

#### Using the interface
```csharp
public readonly struct ExamplePacketHandler : IEasyPacketHandler<ExamplePacket>
{
    void IEasyPacketHandler<ExamplePacket>.Receive(in ExamplePacket packet, in SenderInfo sender, ref bool handled)
    {
        handled = true;
    }
}
```
Any struct that implements the interface will be loaded automatically.
You are free to implement `IEasyPacket<T>` and `IEasyPacketHandler<T>` on the same type.

#### Using the methods
```csharp
public override void Load()
{
    Mod.AddPacketHandler<ExamplePacket>(OnExamplePacketReceived);
}

public override void Unload()
{
    Mod.RemovePacketHandler<ExamplePacket>(OnExamplePacketReceived);
}

private void OnExamplePacketReceived(in ExamplePacket packet, in SenderInfo sender, ref bool handled)
{
    handled = true;
}
```
Adding and removing handlers can occur at any time; it is not restricted to when the mod is loading/unloading.
In the above example (in `ModSystem`), removing the handler can be omitted as this happens automatically when the mod unloads.

### Live examples
- [ExamplePacket.cs](https://github.com/DavidF-Dev/Terraria-Easy-Packets-Lib/blob/main/Code/Examples/ExamplePacket.cs)
- [Guid Player Lib](https://github.com/DavidF-Dev/Terraria-Guid-Player-Lib/blob/master/Code/Internals/GuidPlayer.cs#L69)
- [Downed NPC Library](https://github.com/DavidF-Dev/Terraria-Downed-NPC-Lib/blob/main/Code/Internals/DownedNPCPacket.cs)

## Contact & Support

If you have any questions or would like to get in contact, shoot me an email at `contact@davidfdev.com`.<br>
Alternatively, you can send me a direct message on Twitter at [@DavidF_Dev](https://twitter.com/DavidF_Dev).
