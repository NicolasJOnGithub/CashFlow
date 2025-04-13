using CashFlow.Data.ExplicitStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
