using Framework.Constants;
using Framework.IO;
using System;

namespace Game.Networking
{
    public abstract class ClientPacket
    {
        private byte[] buffer;
        protected OutPacket outPacket;

        protected ClientPacket(AuthCmd opcode)
        {
            outPacket = new OutPacket(opcode);
        }

        public void Clear()
        {
            outPacket.Clear();
            buffer = null;
        }

        public byte[] GetData()
        {
            return buffer;
        }

        public abstract void Write();

        public void WritePacketData()
        {
            if (buffer != null)
            {
                return;
            }
            Write();

            buffer = outPacket.GetData();
            outPacket.Dispose();
        }
    }

    public abstract class ServerPacket : IDisposable
    {
        protected InPacket inPacket;

        protected ServerPacket(InPacket inPacket)
        {
            this.inPacket = inPacket;
        }

        public abstract void Read();

        public void Dispose()
        {
            inPacket.Dispose();
        }

        //public ServerOpcodes GetOpcode()
        //{
        //    return (ServerOpcodes)inPacket.GetOpcode();
        //}
    }

    public class OutPacket : ByteBuffer
    {
        uint opcode;
        public uint GetOpcode() { return opcode; }

        public OutPacket(AuthCmd opcode)
        {
            this.opcode = (uint)opcode;
        }
    }

    public class InPacket : ByteBuffer
    {
        public InPacket(byte[] data) : base(data)
        {
        }
    }
}
