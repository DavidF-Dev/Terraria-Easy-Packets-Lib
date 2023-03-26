# Easy Packets Library
A Terraria tModLoader library mod that provides an easy solution for sending/receiving `ModPackets` with custom data.

## Usage
### Requirements
- tModLoader for `1.4.4`.
- `Side = Both` in `build.txt` (default if not specified).

### Referencing the library
- Add `modReferences = EasyPacketsLib` to your mod's `build.txt` file.
- Add `EasyPacketsLib.dll` to your project as a reference (download from [Releases](https://github.com/DavidF-Dev/Terraria-Easy-Packets-Lib/releases)).
- Subscribe to the library mod on the [Steam Workshop]().

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
- `packet`: Packet data received.
- `sender`: Information regarding the sender of the packet.
- `handled`: An unhandled packet will raise an error.

### Planned features
- Automatically split large packets into multiple sub-packets.
- Mod calls so that a strong reference is not absolutely required.

## Contact & Support

If you have any questions or would like to get in contact, shoot me an email at `contact@davidfdev.com`.<br>
Alternatively, you can send me a direct message on Twitter at [@DavidF_Dev](https://twitter.com/DavidF_Dev).