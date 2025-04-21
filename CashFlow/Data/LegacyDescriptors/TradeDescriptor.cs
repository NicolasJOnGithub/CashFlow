namespace CashFlow.Data.LegacyDescriptors;

public class TradeDescriptor : IDescriptorBase
{
    internal string ID = Guid.NewGuid().ToString();
    public ulong CidUlong { get; set; }
    public ulong TradePartnerCID;
    public int ReceivedGil;
    public ItemWithQuantity[] ReceivedItems = [];
    public long UnixTime { get; set; }

    public TradeDescriptor()
    {
    }

    public override string ToString()
    {
        return $"""
            TradePartnerCID: {TradePartnerCID:X16},
            Gil: {ReceivedGil};
            Items: 
            {ReceivedItems?.Select(x => $"    {x}").Print("\n")}
            """;
    }
}
