using CashFlow.Data.SqlDescriptors;
using ECommons.ExcelServices;
using NightmareUI.Censoring;

namespace CashFlow.Gui.BaseTabs;
public unsafe class TabNpcSales : BaseTab<NpcSaleSqlDescriptor>
{
    public Dictionary<uint, long> ItemValues = [];
    public override void DrawTable()
    {
        if(ImGuiEx.BeginDefaultTable(["Your Character", "Paid", "~Item Name", "Qty", "Date"]))
        {
            for(int i = IndexBegin; i < IndexEnd; i++)
            {
                var t = Data[i];
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                {
                    ImGuiEx.Text(S.MainWindow.CIDMap.TryGetValue(t.CidUlong, out var s) ? Censor.Character(s.ToString()) : Censor.Hide($"{t.CidUlong:X16}"));
                }

                ImGui.TableNextColumn();
                Utils.DrawGilIncrease();
                ImGuiEx.Text($" {t.Price:N0}");

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

    public override void AddData(NpcSaleSqlDescriptor data, List<NpcSaleSqlDescriptor> list)
    {
        var id = (uint)data.Item % 1_000_000u;
        if(!ItemValues.ContainsKey(id)) ItemValues[id] = 0;
        ItemValues[id] += data.Price * data.Quantity;
        base.AddData(data, list);
    }

    public override List<NpcSaleSqlDescriptor> LoadData()
    {
        ItemValues.Clear();
        return P.DataProvider.GetNpcSales();
    }

    public override bool ProcessSearchByItem(NpcSaleSqlDescriptor x)
    {
        return ExcelItemHelper.GetName((uint)(x.Item % 1000000), true).Contains(SearchItem, StringComparison.OrdinalIgnoreCase);
    }

    public override bool ProcessSearchByName(NpcSaleSqlDescriptor x)
    {
        var name = S.MainWindow.CIDMap.SafeSelect(x.CidUlong);
        return name.ToString().Contains(SearchName, StringComparison.OrdinalIgnoreCase);
    }
}
