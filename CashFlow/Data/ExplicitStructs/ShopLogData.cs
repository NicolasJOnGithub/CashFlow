using Lumina.Excel;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashFlow.Data.ExplicitStructs;
[StructLayout(LayoutKind.Explicit)]
public unsafe struct ShopLogData
{
    [FieldOffset(0)] public uint ItemId;
    [FieldOffset(4)] public uint Quantity;
    [FieldOffset(8)] public uint Price;

    public RowRef<Item> Item => new(Svc.Data.Excel, ItemId % 1000000);
    public bool IsHQ => ItemId > 1000000;
}
