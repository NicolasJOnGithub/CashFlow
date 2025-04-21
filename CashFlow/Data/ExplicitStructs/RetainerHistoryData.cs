using Dalamud.Memory;

namespace CashFlow.Data.ExplicitStructs;
[StructLayout(LayoutKind.Explicit, Size = 52)]
public unsafe struct RetainerHistoryData
{
    [FieldOffset(0)] public uint ItemID;
    [FieldOffset(4)] public uint Price;
    [FieldOffset(8)] public uint UnixTimeSeconds;
    [FieldOffset(12)] public uint Quantity;
    [FieldOffset(16)][MarshalAs(UnmanagedType.I1)] public bool IsHQ;
    [FieldOffset(17)] public byte Unk17;
    [FieldOffset(18)][MarshalAs(UnmanagedType.I1)] public bool IsMannequinn;
    [FieldOffset(19)] private fixed byte BuyerNamePtr[32];
    public string BuyerName
    {
        get
        {
            fixed(byte* ptr = BuyerNamePtr)
            {
                return MemoryHelper.ReadString((nint)ptr, 32);
            }
        }
        set
        {
            fixed(byte* ptr = BuyerNamePtr)
            {
                if(value.Length > 32) value = value[..32];
                MemoryHelper.WriteString((nint)ptr, value);
            }
        }
    }
}
