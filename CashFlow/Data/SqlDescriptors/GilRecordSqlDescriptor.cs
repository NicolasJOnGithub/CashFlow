using SqlKata;

namespace CashFlow.Data.SqlDescriptors;
public unsafe class GilRecordSqlDescriptor : IDescriptorBase
{
    private long Cid { get; set; }
    public long GilPlayer { get; set; }
    public long GilRetainer { get; set; }
    public long UnixTime { get; set; }

    [Ignore] public long Diff { get; set; }

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
}