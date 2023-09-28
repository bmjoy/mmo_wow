using Framework.Constants;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Game.Networking.Packets
{
    class AuthLogonChallengeRequest : ClientPacket
    {
        public AuthLogonChallengeRequest(string account) : base(AuthCmd.AUTH_LOGON_CHALLENGE)
        {
            this.cmd = (byte)AuthCmd.AUTH_LOGON_CHALLENGE;
            this.account = account;
            this.account_length = (byte)account.Length;
            this.size = (ushort)(this.account_length + 30);
        }

        public override void Write()
        {
            outPacket.WriteUInt8(cmd);
            outPacket.WriteUInt8(error);
            outPacket.WriteUInt16((ushort)(account.Length + 30));
            outPacket.WriteBytes(gamename.ToCString());
            outPacket.WriteBytes(versions);
            outPacket.WriteUInt16(build);
            outPacket.WriteBytes(platform.ToCString());
            outPacket.WriteBytes(os.ToCString());
            outPacket.WriteBytes(Encoding.ASCII.GetBytes(country));
            outPacket.WriteUInt32(timezone_bias);
            outPacket.WriteUInt32(ip);
            outPacket.WriteUInt8((byte)account.Length);
            outPacket.WriteBytes(Encoding.ASCII.GetBytes(account));
        }

        public byte cmd;
        public byte error = 6;
        public ushort size;//30+account_length
        public string gamename = "WoW";
        public byte[] versions = { 3, 3, 5 };
        public ushort build = 12340;
        public string platform = "68x";
        public string os = "niW";
        public string country = "SUne";
        public uint timezone_bias = 60;
        public uint ip = BitConverter.ToUInt32(System.Text.Encoding.ASCII.GetBytes("127.0.0.1"));
        public byte account_length;
        public string account;
    }

    public class AuthLogonChallengeResponse : ServerPacket
    {
        public AuthLogonChallengeResponse(InPacket inPacket) : base(inPacket)
        {

        }

        public override void Read()
        {
            cmd = inPacket.ReadUInt8();
            unk2 = inPacket.ReadUInt8();
            error = (AuthResult)inPacket.ReadUInt8();
            if(error != AuthResult.WOW_SUCCESS)
            {
                return;
            }
            b = inPacket.ReadBytes(32).ToBigInteger();
            g_length = inPacket.ReadUInt8();
            g = inPacket.ReadBytes(1).ToBigInteger();
            n_length = inPacket.ReadUInt8();
            n = inPacket.ReadBytes(32).ToBigInteger();
            salt = inPacket.ReadBytes(32).ToBigInteger();
            version_challenge = inPacket.ReadBytes(16).ToBigInteger();
            security_flags = inPacket.ReadUInt8();
        }

        public byte cmd;
        public byte unk2;
        public AuthResult error;
        public BigInteger b;
        public byte g_length;
        public BigInteger g;
        public byte n_length;
        public BigInteger n;
        public BigInteger salt;
        public BigInteger version_challenge;
        public byte security_flags;
    }

    public class AuthLogonProofRequest : ClientPacket
    {
        public AuthLogonProofRequest(BigInteger a, byte[] hash) : base(AuthCmd.AUTH_LOGON_PROOF)
        {
            this.cmd = (byte)AuthCmd.AUTH_LOGON_PROOF;
            this.a = a;
            this.hash = hash;
        }

        public override void Write()
        {
            outPacket.WriteUInt8(cmd);
            outPacket.WriteBytes(a.ToCleanByteArray());
            outPacket.WriteBytes(hash);
            outPacket.WriteBytes(crc);
            outPacket.WriteUInt8(number_of_keys);
            outPacket.WriteUInt8(security_flags);
        }

        public byte cmd;
        public BigInteger a;
        public byte[] hash;
        public byte[] crc = new byte[20];
        public byte number_of_keys = 0;
        public byte security_flags = 0;
    }

    public class AuthLogonProofResponse : ServerPacket
    {
        public AuthLogonProofResponse(InPacket inPacket) : base(inPacket)
        {

        }

        public override void Read()
        {
            cmd = inPacket.ReadUInt8();
            error = (AuthResult)inPacket.ReadUInt8();
            if (error != AuthResult.WOW_SUCCESS)
            {
                return;
            }
            m2 = inPacket.ReadBytes(20);
            account_flags = inPacket.ReadUInt32();
            survey_id = inPacket.ReadUInt32();
            login_flags = inPacket.ReadUInt16();
        }

        public byte cmd;
        public AuthResult error;
        public byte[] m2;
        public uint account_flags;
        public uint survey_id;
        public ushort login_flags;
    }

    public class RealmListRequest : ClientPacket
    {
        public RealmListRequest() : base(AuthCmd.REALM_LIST)
        {
            cmd = (byte)AuthCmd.REALM_LIST;
        }

        public override void Write()
        {
            outPacket.WriteUInt8(cmd);
            outPacket.WriteUInt32(account_id);
        }

        public byte cmd;
        public uint account_id = 0;
    }

    public class RealmListResponse : ServerPacket
    {
        public RealmListResponse(InPacket inPacket) : base(inPacket)
        {
        }

        public override void Read()
        {
            this.cmd = inPacket.ReadUInt8();
            this.size = inPacket.ReadUInt16();
            this.uk = inPacket.ReadUInt32();
            this.realm_list_size = inPacket.ReadUInt16();
            this.Realms = new List<WorldServerInfo>();

            if (0 < realm_list_size)
            {
                WorldServerInfo worldServerInfo = new WorldServerInfo();
                worldServerInfo.Type = inPacket.ReadUInt8();
                worldServerInfo.Locked = inPacket.ReadUInt8();
                worldServerInfo.Flags = inPacket.ReadUInt8();
                worldServerInfo.Name = inPacket.ReadCString();

                string address = inPacket.ReadCString();
                string[] tokens = address.Split(':');

                worldServerInfo.Address = tokens[0];
                worldServerInfo.Port = tokens.Length > 1 ? int.Parse(tokens[1]) : 8085;

                worldServerInfo.Population = inPacket.ReadFloat();
                worldServerInfo.Load = inPacket.ReadUInt8();
                worldServerInfo.Timezone = inPacket.ReadUInt8();
                worldServerInfo.Id = inPacket.ReadUInt8();
                if (0 != (worldServerInfo.Flags & 4))
                {
                    worldServerInfo.VersionMajor = inPacket.ReadUInt8();
                    worldServerInfo.VersionMinor = inPacket.ReadUInt8();
                    worldServerInfo.VersionBugFix = inPacket.ReadUInt8();
                    worldServerInfo.Build = inPacket.ReadUInt16();
                }

                this.Realms.Add(worldServerInfo);
            }
        }

        public byte cmd;
        public ushort size;
        public uint uk;
        public ushort realm_list_size;

        internal List<WorldServerInfo> Realms
        {
            get;
            private set;
        }
    }
}
