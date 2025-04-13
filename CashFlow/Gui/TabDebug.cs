using CashFlow.Data.ExplicitStructs;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Memory;
using ECommons.Automation;
using ECommons.ExcelServices;
using ECommons.MathHelpers;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Diagnostics;
using static FFXIVClientStructs.FFXIV.Client.UI.Agent.AgentHousingPlant;

namespace CashFlow.Gui;
public static unsafe class TabDebug
{
    static Stopwatch Sw1 = new();
    static Stopwatch Sw2 = new();
    public static void Draw()
    {
        if(ImGui.CollapsingHeader("Test speed"))
        {
            ImGuiEx.Text($"""
                No raii: {Sw1.ElapsedMilliseconds}
                Raii: {Sw2.ElapsedMilliseconds}
                """);
            if(ImGui.Button("Restart"))
            {
                Sw1.Reset();
                Sw2.Reset();
            }
            var cur = ImGui.GetCursorPos();
            Sw1.Start();
            for(int i = 0; i < 10000; i++)
            {
                ImGui.SetCursorPos(cur);
                ImGui.PushStyleColor(ImGuiCol.Text, EColor.Red);
                ImGui.PushStyleColor(ImGuiCol.TextDisabled, EColor.RedDark);
                ImGui.PushFont(UiBuilder.MonoFont);
                ImGui.PushID(i);
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0.5f, 0.5f));
                ImGui.TextUnformatted("123");
                ImGui.PopStyleVar(2);
                ImGui.PopID();
                ImGui.PopFont();
                ImGui.PopStyleColor(2);
            }
            Sw1.Stop();
            Sw2.Start();
            for(int i = 0; i < 10000; i++)
            {
                ImGui.SetCursorPos(cur);
                using(_ = ImRaii.PushColor(ImGuiCol.Text, EColor.Red))
                {
                    using(_ = ImRaii.PushColor(ImGuiCol.TextDisabled, EColor.RedDark))
                    {
                        using(_ = ImRaii.PushFont(UiBuilder.MonoFont))
                        {
                            using(_ = ImRaii.PushStyle(ImGuiStyleVar.Alpha, 0.5f))
                            {
                                using(_ = ImRaii.PushStyle(ImGuiStyleVar.FramePadding, new Vector2(0.5f, 0.5f)))
                                {
                                    using(_ = ImRaii.PushId(1))
                                    {
                                        ImGui.TextUnformatted("123");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            Sw2.Stop();
        }
        if(ImGui.CollapsingHeader("Merchant"))
        {
            ImGuiEx.TextCopy($"{(nint)AgentModule.Instance()->GetAgentByInternalId(AgentId.MerchantSetting):X}");
            ImGuiEx.TextCopy($"{(nint)AgentMerchantSettingInfo.Instance():X}");
            var info = AgentMerchantSettingInfo.Instance();
            if(info != null)
            {
                //ImGuiEx.TextWrapped(MemoryHelper.ReadRaw((nint)info, 192).ToHexString());
                //ImGuiEx.TextWrapped(MemoryHelper.ReadRaw(((nint)info)+ 192 + 0xC*sizeof(AgentMerchantSettingInfo.MannequinnItem), 300).ToHexString());
                ImGuiEx.Text($"Selected items: {info->SelectedItems:B32} / {Bitmask.IsBitSet(info->SelectedItems, 0)}");
                for(int i = 0; i < info->ItemsSpan.Length; i++)
                {
                    var item = info->ItemsSpan[i];
                    ImGuiEx.Text($"""
                    {nameof(AgentMerchantSettingInfo.Items.ItemID)} {ExcelItemHelper.GetName(item.ItemID, true)}
                    {nameof(AgentMerchantSettingInfo.Items.IsHq)} {item.IsHq}
                    {nameof(AgentMerchantSettingInfo.Items.Price)} {item.Price}
                    {nameof(AgentMerchantSettingInfo.Items.Color1)} {item.Color1}
                    {nameof(AgentMerchantSettingInfo.Items.Color2)} {item.Color2}
                    {nameof(AgentMerchantSettingInfo.Items.Availability)} {item.Availability}
                    """);
                    ImGui.Separator();
                }
            }
        }
        if(ImGui.Button("Test purchase"))
        {
            if(TryGetAddonByName<AtkUnitBase>("ItemSearchResult", out var addon))
            {
                Callback.Fire(addon, true, 2, 0);
            }
        }
        if(ImGui.CollapsingHeader("Last purchased"))
        {
            var info = InfoProxyItemSearch.Instance()->LastPurchasedMarketboardItem;
            ImGuiEx.Text($"""
                {MemoryHelper.ReadRaw((nint)(&info), sizeof(LastPurchasedMarketboardItem)).ToHexString()}
                ListingId {info.ListingId}
                ItemId {info.ItemId}
                """);
        }
        ImGuiEx.Text($"Trade partner: {Utils.GetTradePartner()}");
        ref var data = ref Ref<string>.Get();
        if(ImGui.Button("Load cidmap"))
        {
            data = P.DataProvider.GetRegisteredPlayers().Select(x => $"{x.Key:X16}={x.Value}").Print("\n");
        }
        if(ImGui.Button("Load trades"))
        {
            data = P.DataProvider.GetTrades(0, 0).Print("\n-----\n");
        }
        if(data != null)
        {
            ImGuiEx.TextWrapped(data);
        }
    }
}
