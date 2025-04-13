using ECommons.ExcelServices;
using MessagePack;

namespace CashFlow.Data;
[MessagePackObject]
public unsafe class ItemWithQuantity
{
    [Key(0)] public uint ItemID;
    [Key(1)] public int Quantity;

    public ItemWithQuantity() { }

    public ItemWithQuantity(uint itemID, int quantity)
    {
        ItemID = itemID;
        Quantity = quantity;
    }

    public override string ToString()
    {
        return $"{ExcelItemHelper.GetName(ItemID % 1000000, true)}" + (ItemID > 1000000 ? " HQ" : "") + $" x{Quantity}";
    }
}
