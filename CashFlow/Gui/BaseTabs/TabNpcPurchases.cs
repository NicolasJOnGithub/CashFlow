using CashFlow.Data.SqlDescriptors;
using ECommons.ExcelServices;
using NightmareUI.Censoring;

namespace CashFlow.Gui.BaseTabs;
public unsafe class TabNpcPurchases : BaseTab<NpcPurchaseSqlDescriptor>
{
    public Dictionary<uint, long> ItemValues = [];

    public override void DrawTable()
    {
        if(ImGuiEx.BeginDefaultTable(["Your Character", "Paid", "~Item Name", "Qty", "Date"]))
        {
            for(var i = IndexBegin; i < IndexEnd; i++)
            {
                var t = Data[i];
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                {
                    ImGuiEx.Text(S.MainWindow.CIDMap.TryGetValue(t.CidUlong, out var s) ? Censor.Character(s.ToString()) : Censor.Hide($"{t.CidUlong:X16}"));
                }

                ImGui.TableNextColumn();
                Utils.DrawGilDecrease();
                ImGuiEx.Text($" {t.Price:N0}");
                if(t.IsBuybackBool)
                {
                    ImGui.SameLine(0, 2);
                    ImGui.PushFont(UiBuilder.IconFont);
                    ImGuiEx.Text(FontAwesomeIcon.FastBackward.ToIconString());
                    ImGui.PopFont();
                    ImGuiEx.Tooltip("This is a buyback");
                }

                ImGui.TableNextColumn();
                ImGuiEx.Text(ExcelItemHelper.GetName((uint)(t.Item % 1000000)));

                ImGui.TableNextColumn();
                ImGuiEx.Text($"x{t.Quantity}" + (t.Item > 1000000 ? "" : ""));

                ImGui.TableNextColumn();
                ImGuiEx.Text(DateTimeOffset.FromUnixTimeMilliseconds(t.UnixTime).ToPreferredTimeString());
            }
            ImGui.EndTable();
        }
    }

    public override List<NpcPurchaseSqlDescriptor> LoadData()
    {

        ItemValues.Clear();
        return P.DataProvider.GetNpcPurchases();
    }

    public override void AddData(NpcPurchaseSqlDescriptor data, List<NpcPurchaseSqlDescriptor> list)
    {
        var id = (uint)data.Item % 1_000_000u;
        if(!ItemValues.ContainsKey(id)) ItemValues[id] = 0;
        ItemValues[id] += data.Price * data.Quantity;
        base.AddData(data, list);
    }

    public override bool ProcessSearchByItem(NpcPurchaseSqlDescriptor x)
    {
        return ExcelItemHelper.GetName((uint)(x.Item % 1000000), true).Contains(SearchItem, StringComparison.OrdinalIgnoreCase);
    }

    public override bool ProcessSearchByName(NpcPurchaseSqlDescriptor x)
    {
        var name = S.MainWindow.CIDMap.SafeSelect(x.CidUlong);
        return name.ToString().Contains(SearchName, StringComparison.OrdinalIgnoreCase);
    }
}
