using CashFlow.Data.ExplicitStructs;

namespace CashFlow.Data;
public unsafe class MerchantInfo
{
    public AgentMerchantSettingInfo Data;
    public string Name;

    public MerchantInfo(AgentMerchantSettingInfo data, string name)
    {
        Data = data;
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}
