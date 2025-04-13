using SqlKata;

namespace CashFlow.Data.SqlDescriptors;
public unsafe class ShopPurchaseSqlDescriptor : IDescriptorBase
{
    private long Cid { get; set; }
    public string RetainerName { get; set; }
    public int Item { get; set; }
    public int Price { get; set; }
    public int Quantity { get; set; }
    private int IsMannequinn { get; set; }
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
    internal bool IsMannequinnBool
    {
        get
        {
            return IsMannequinn != 0;
        }
        set
        {
            IsMannequinn = value ? 1 : 0;
        }
    }
}
