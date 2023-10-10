public class Character
{
    #region Public Properties

    public byte[] Bytes { get; }
    public Class Class { get; set; }
    public uint Flags { get; set; }
    public Gender Gender { get; set; }
    public ulong GUID { get; set; }
    public uint GuildId { get; set; }
    public Item[] Items { get; set; }
    public byte Level { get; set; }
    public uint MapId { get; set; }
    public string Name { get; set; }
    public float O { get; set; }
    public uint PetFamilyId { get; set; }
    public uint PetInfoId { get; set; }
    public uint PetLevel { get; set; }
    public Race Race { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public uint ZoneId { get; set; }

    #endregion Public Properties

    // 5

    #region Internal Constructors

    internal Character()
    {
        Items = new Item[19];
    }

    //internal Character(byte[] data, int readIndex = 0) : base(data, readIndex)
    //{
    //    Items = new Item[19];
    //    GUID = ReadUInt64();
    //    Name = ReadCString();
    //    Race = (Race)ReadByte();
    //    Class = (Class)ReadByte();
    //    Gender = (Gender)ReadByte();
    //    Bytes = ReadBytes(5);
    //    Level = ReadByte();
    //    ZoneId = ReadUInt32();
    //    MapId = ReadUInt32();
    //    X = ReadSingle();
    //    Y = ReadSingle();
    //    Z = ReadSingle();
    //    GuildId = ReadUInt32();
    //    Flags = ReadUInt32();
    //    ReadUInt32(); // customize (rename, etc)
    //    ReadByte(); // first login
    //    PetInfoId = ReadUInt32();
    //    PetLevel = ReadUInt32();
    //    PetFamilyId = ReadUInt32();

    //    // read items
    //    for (int i = 0; i < Items.Length; ++i)
    //    {
    //        Items[i] = new Item(data, ReadIndex);
    //        ReadIndex = Items[i].ReadIndex;
    //    }

    //    // read bags
    //    for (int i = 0; i < 4; ++i)
    //    {
    //        ReadUInt32();
    //        ReadByte();
    //        ReadUInt32();
    //    }
    //}

    #endregion Internal Constructors

    #region Public Methods

    public override string ToString()
    {
        return Name;
    }

    #endregion Public Methods
}
