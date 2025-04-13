using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashFlow.Data.ExplicitStructs;
[StructLayout(LayoutKind.Explicit)]
public unsafe struct AgentMerchantSettingInfo
{
    public static AgentMerchantSettingInfo* Instance() => (AgentMerchantSettingInfo*)*(nint*)(((nint)AgentModule.Instance()->GetAgentByInternalId(AgentId.MerchantSetting)) + 40);

    [FieldOffset(192)] public MannequinnItem Items;
    [FieldOffset(1912)] public uint SelectedItems;

    public readonly Span<MannequinnItem> ItemsSpan
    {
        get
        {
            fixed(MannequinnItem* ptr = &Items)
            {
                return new(ptr, 0xC);
            }
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 152)]
    public unsafe struct MannequinnItem
    {
        [FieldOffset(0)] public uint ItemIDWithQuality;
        public readonly uint ItemID => ItemIDWithQuality % 1000000;
        public readonly bool IsHq => ItemIDWithQuality > 1000000;
        [FieldOffset(42)] public byte Color1;
        [FieldOffset(43)] public byte Color2;
        [FieldOffset(24)] public nint Price;
        [FieldOffset(45)] public byte Availability;
    }
}
