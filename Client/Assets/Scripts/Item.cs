public class Item
{
    #region Public Properties

    public uint DisplayId { get; set; }
    public byte InventoryType { get; set; }

    #endregion Public Properties

    #region Internal Constructors

    //internal Item(byte[] data, int readIndex = 0) : base(data, readIndex)
    //{
    //    DisplayId = ReadUInt32();
    //    InventoryType = ReadByte();
    //    ReadUInt32();
    //}

    #endregion Internal Constructors
}