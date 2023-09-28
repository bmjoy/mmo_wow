using Framework.Constants;

namespace Game.Networking.Packets
{
    //class Ping : ClientPacket
    //{
    //    public Ping(uint serial, uint latency) : base(ClientOpcodes.Ping) 
    //    {
    //        Serial = serial;
    //        Latency = latency;
    //    }

    //    public override void Write()
    //    {
    //        outPacket.WriteUInt32(Serial);
    //        outPacket.WriteUInt32(Latency);
    //    }

    //    public uint Serial;
    //    public uint Latency;
    //}

    //class Pong : ServerPacket
    //{
    //    public Pong(InPacket inPacket) : base(inPacket)
    //    {
    //    }

    //    public override void Read()
    //    {
    //        Serial = inPacket.ReadUInt32();
    //    }

    //    uint Serial;
    //}
}
