using CashFlow.Data.LegacyDescriptors;
using ECommons.ExcelServices;
using NightmareUI.Censoring;

namespace CashFlow.Gui.BaseTabs;
public unsafe class TabRetainerSales : BaseTab<RetainerSaleDescriptor>
{
    public override string SearchNameHint { get; } = "Search Player's/Retainer's Name...";
    public override void DrawTable()
    {
        if(ImGuiEx.BeginDefaultTable(["Your Character", "Your Retainer", "Buyer", "Paid", "~Item", "Qty", "Date"]))
        {
            /*if(ImGui.TableGetSortSpecs().SpecsDirty)
            {
                var index = ImGui.TableGetSortSpecs().Specs.ColumnIndex;
                var isAsc = ImGui.TableGetSortSpecs().Specs.SortDirection == ImGuiSortDirection.Ascending;
                static string index0func(RetainerSaleDescriptor x) => S.MainWindow.CIDMap.SafeSelect(x.Cid).ToString();
                static string index1func(RetainerSaleDescriptor x) => x.RetainerName;
                static string index2func(RetainerSaleDescriptor x) => x.BuyerName;
                static string index3func(RetainerSaleDescriptor x) => ExcelItemHelper.GetName((uint)(x.ItemID % 1000000));
                static int index4func(RetainerSaleDescriptor x) => x.Quantity;
                static long index5func(RetainerSaleDescriptor x) => x.UnixTime;
                static int index6func(RetainerSaleDescriptor x) => x.Price;
                if(index == 0) Sales = [.. isAsc ? Sales.OrderBy(index0func) : Sales.OrderByDescending(index0func)];
                if(index == 1) Sales = [.. isAsc ? Sales.OrderBy(index1func) : Sales.OrderByDescending(index1func)];
                if(index == 2) Sales = [.. isAsc ? Sales.OrderBy(index2func) : Sales.OrderByDescending(index2func)];
                if(index == 3) Sales = [.. isAsc ? Sales.OrderBy(index3func) : Sales.OrderByDescending(index3func)];
                if(index == 4) Sales = [.. isAsc ? Sales.OrderBy(index4func) : Sales.OrderByDescending(index4func)];
                if(index == 5) Sales = [.. isAsc ? Sales.OrderBy(index5func) : Sales.OrderByDescending(index5func)];
                if(index == 6) Sales = [.. isAsc ? Sales.OrderBy(index6func) : Sales.OrderByDescending(index6func)];
            }*/
            for(int i = IndexBegin; i < IndexEnd; i++)
            {
                var t = Data[i];
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                {
                    ImGuiEx.Text(S.MainWindow.CIDMap.TryGetValue(t.CidUlong, out var s) ? Censor.Character(s.ToString()) : Censor.Hide($"{t.CidUlong:X16}"));
                }

                ImGui.TableNextColumn();
                {
                    ImGuiEx.Text(t.RetainerName);
                }

                ImGui.TableNextColumn();
                ImGuiEx.Text(Censor.Character(t.BuyerName));

                ImGui.TableNextColumn();
                Utils.DrawGilIncrease();
                ImGuiEx.Text($" {t.Price:N0}");
                if(t.IsMannequinn)
                {
                    ImGui.SameLine(0, 2);
                    ImGui.PushFont(UiBuilder.IconFont);
                    ImGuiEx.Text("\uf183");
                    ImGui.PopFont();
                    ImGuiEx.Tooltip("This sale was done via Mannequinn");
                }

                ImGui.TableNextColumn();
                ImGuiEx.Text(ExcelItemHelper.GetName((uint)(t.ItemID % 1000000)));

                ImGui.TableNextColumn();
                ImGuiEx.Text($"x{t.Quantity}" + (t.ItemID > 1000000 ? "" : ""));

                ImGui.TableNextColumn();
                ImGuiEx.Text(DateTimeOffset.FromUnixTimeMilliseconds(t.UnixTime).ToLocalTime().ToString());
            }
            ImGui.EndTable();
        }
    }

    public override List<RetainerSaleDescriptor> LoadData()
    {
        return P.DataProvider.GetRetainerHistory(DateMin.ToUnixTimeMilliseconds(), DateMax.ToUnixTimeMilliseconds());
    }

    public override bool ProcessSearchByItem(RetainerSaleDescriptor x)
    {
        return ExcelItemHelper.GetName((uint)(x.ItemID % 1000000), true).Contains(SearchItem, StringComparison.OrdinalIgnoreCase);
    }

    public override bool ProcessSearchByName(RetainerSaleDescriptor x)
    {
        var name = S.MainWindow.CIDMap.SafeSelect(x.CidUlong);
        return name.ToString().Contains(SearchName, StringComparison.OrdinalIgnoreCase)
            || x.RetainerName.ToString().Contains(SearchName, StringComparison.OrdinalIgnoreCase)
            || x.BuyerName.ToString().Contains(SearchName, StringComparison.OrdinalIgnoreCase);
    }
}
