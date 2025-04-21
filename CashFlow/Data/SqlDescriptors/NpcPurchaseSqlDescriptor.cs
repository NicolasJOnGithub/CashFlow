using SqlKata;

namespace CashFlow.Data.SqlDescriptors;
public unsafe class NpcPurchaseSqlDescriptor : IDescriptorBase
{
    private long Cid { get; set; }
    public int Item { get; set; }
    public int Price { get; set; }
    public int Quantity { get; set; }
    private int IsBuyback { get; set; }
    public long UnixTime { get; set; }

    [Ignore]
    public ulong CidUlong
    {
        get
        {
            var cid = Cid;
            return *(ulong*)&cid;
        }
        set
        {
            Cid = *(long*)&value;
        }
    }


    [Ignore]
    public bool IsBuybackBool
    {
        get
        {
            return IsBuyback != 0;
        }
        set
        {
            IsBuyback = value ? 1 : 0;
        }
    }
}
