using CashFlow.Data.SqlDescriptors;
using ECommons.ExcelServices;
using FFXIVClientStructs.Interop;
using ImPlotNET;
using NightmareUI;
using NightmareUI.Censoring;
using NightmareUI.PrimaryUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashFlow.Gui.BaseTabs;
public unsafe class TabGilHistory : BaseTab<GilRecordSqlDescriptor>
{
    private DateOnly[] DateRange;
    private string[] DateRangeStrings => C.ReverseDayMonth?DateRangeStringsR:DateRangeStringsN;
    private string[] DateRangeStringsN;
    private string[] DateRangeStringsR;
    private float[] DateRangeFloats;
    private double[] DateRangeDoubles;
    private ulong SelectedCID = 0;
    private Dictionary<ulong, long> PrevGil = [];
    private List<ulong> RecordedCIDs = [];
    private Dictionary<DateOnly, Dictionary<ulong, long>> GilByDate = [];
    private Dictionary<DateOnly, long> GilByDateSum = [];
    private Dictionary<ulong, long> GilByCharaSum = [];
    private bool AutoFit = false;
    private long TotalDiff = 0;
    private OrderedDictionary<uint, long> TopItemsSold = [];
    private OrderedDictionary<uint, long> TopItemsPurchased = [];

    public TabGilHistory()
    {
        var firstDate = new DateOnly(2025, 1, 1);
        List<DateOnly> dates = [];
        for(var i = 0; i < 3650; i++)
        {
            dates.Add(firstDate.AddDays(i));
        }
        DateRange = [.. dates];
        DateRangeStringsN = [.. dates.Select(x => Utils.GetBriefDate(x, false))];
        DateRangeStringsR = [.. dates.Select(x => Utils.GetBriefDate(x, true))];
        DateRangeFloats = [.. dates.Select(x => (float)x.GetTotalDays())];
        DateRangeDoubles = [.. dates.Select(x => (double)x.GetTotalDays())];
    }

    void PopulateGilByDateSum()
    {
        var uniques = GilByDate.Values.Select(x => x.Keys).SelectMany(x => x).Distinct().ToArray();
        foreach(var x in GilByDate)
        {
            GilByDateSum[x.Key] = 0;
        }
        //PluginLog.Information($"Keys: {GilByDate.Count}");
        Dictionary<ulong, long> prevValues = [];
        foreach(var x in GilByDateSum.Keys.ToArray().Order())
        {
            List<ulong> addedValues = [];
            foreach(var data in GilByDate[x])
            {
                addedValues.Add(data.Key);
                prevValues[data.Key] = data.Value;
                GilByDateSum[x] += data.Value;
                //PluginLog.Information($"Adding for player {data.Key} by {data.Value}");
            }
            foreach(var data in uniques)
            {
                if(!addedValues.Contains(data))
                {
                    //PluginLog.Information($"Correcting for player {data} by {prevValues.SafeSelect(data)}");
                    GilByDateSum[x] += prevValues.SafeSelect(data);
                }
            }
        }
    }

    public override void DrawTable()
    {
        try
        {
            if(GilByDateSum.Count > 0)
            {

                if(ImPlot.BeginPlot("##GilHistory"))
                {
                    var sorted = GilByDateSum.OrderBy(x => x.Key.GetTotalDays());
                    var x_axisf = GilByDateSum.Keys.Select(x => (float)x.GetTotalDays()).ToArray();
                    var y_axis = GilByDateSum.Values.Select(x => (float)((double)x / 1_000_000.0)).ToArray();
                    var strings = GilByDateSum.Keys.Select(x => x.GetBriefDate()).ToArray();
                    ImPlot.SetupAxisTicks(ImAxis.X1, ref DateRangeDoubles[0], DateRangeDoubles.Length, DateRangeStrings);
                    ImPlot.SetupAxisFormat(ImAxis.Y1, "%gM");
                    if(AutoFit)
                    {
                        ImPlot.SetNextAxesToFit();
                        AutoFit = false;
                    }
                    ImPlot.PushStyleVar(ImPlotStyleVar.LineWeight, 4f);
                    ImPlot.SetNextMarkerStyle(ImPlotMarker.Circle, 5f);
                    ImPlot.PlotLine("Gil", ref x_axisf[0], ref y_axis[0], x_axisf.Length);
                    if(ImPlot.IsPlotHovered())
                    {
                        List<(int Index, Vector2 Pos)> vec = [];
                        for(int i = 0; i < x_axisf.Length; i++)
                        {
                            vec.Add((i, ImPlot.PlotToPixels(x_axisf[i], y_axis[i])));
                        }
                        var nearestPoint = vec.OrderBy(x => Vector2.DistanceSquared(ImGui.GetMousePos(), x.Pos));
                        if(nearestPoint.Any())
                        {
                            var p = nearestPoint.First();
                            //PluginLog.Information($"{ImPlot.PlotToPixels(ImPlot.GetPlotMousePos())} / {nearestPoint.First()} / {ImGui.GetMousePos()}");
                            var plotP = ImPlot.PlotToPixels(ImPlot.GetPlotMousePos());
                            var distance = Vector2.Distance(plotP, p.Pos);
                            if(distance < 10)
                            {
                                ImPlot.GetPlotDrawList().AddCircleFilled(new(p.Pos.X, p.Pos.Y), 7, EColor.RedBright.ToUint());
                                ImGuiEx.Tooltip($"{strings.SafeSelect(p.Index)}\n{y_axis.SafeSelect(p.Index):N0}M gil");
                            }
                        }
                    }
                    ImPlot.PopStyleVar();
                    ImPlot.EndPlot();
                }
            }
        }
        catch(Exception e)
        {
            e.Log();
        }
        NuiTools.ButtonTabs("GilTabs", [[new("Stats By Day", drawStats), new("Averages", drawAverages), new("Totals", drawTotals), new("Tops", drawTops)]], child:false);
        void drawTops()
        {
            if(this.SelectedCID == 0)
            {
                {
                    var data = S.MainWindow.TabTradeLog.CharasGivenGilToTotal;
                    {
                        ImGuiEx.Text($"Gil given to, top-5:");
                        if(ImGuiEx.BeginDefaultTable("Gil2", ["Character", "~Total Gil"]))
                        {
                            foreach(var t in data.OrderByDescending(x => x.Value).Take(5))
                            {
                                ImGui.TableNextRow();
                                ImGui.TableNextColumn();
                                {
                                    ImGuiEx.Text(S.MainWindow.CIDMap.TryGetValue(t.Key, out var s) ? Censor.Character(s.ToString()) : Censor.Hide($"{t.Key:X16}"));
                                }

                                ImGui.TableNextColumn();
                                Utils.DrawColoredGilText(-t.Value);
                            }
                            ImGui.EndTable();
                        }
                    }
                }
                {
                    var data = S.MainWindow.TabTradeLog.CharasRecvGilFromTotal;
                    {
                        ImGuiEx.Text($"Gil received from, top-5:");
                        if(ImGuiEx.BeginDefaultTable("Gil1", ["Character", "~Total Gil"]))
                        {
                            foreach(var t in data.OrderByDescending(x => x.Value).Take(5))
                            {
                                ImGui.TableNextRow();
                                ImGui.TableNextColumn();
                                {
                                    ImGuiEx.Text(S.MainWindow.CIDMap.TryGetValue(t.Key, out var s) ? Censor.Character(s.ToString()) : Censor.Hide($"{t.Key:X16}"));
                                }

                                ImGui.TableNextColumn();
                                Utils.DrawColoredGilText(t.Value);
                            }
                            ImGui.EndTable();
                        }
                    }
                }
                ImGuiEx.Text($"Items sold, gil, top-5:");
                if(ImGuiEx.BeginDefaultTable("ItemsTop2", ["Item##top2", "~Total Gil"]))
                {
                    foreach(var t in this.TopItemsSold)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        {
                            ImGuiEx.Text(ExcelItemHelper.GetName(t.Key));
                        }

                        ImGui.TableNextColumn();
                        Utils.DrawColoredGilText(t.Value);
                    }
                    ImGui.EndTable();
                }
                ImGuiEx.Text($"Items purchased, gil, top-5:");
                if(ImGuiEx.BeginDefaultTable("ItemsTop", ["Item##top", "~Total Gil"]))
                {
                    foreach(var t in this.TopItemsPurchased)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        {
                            ImGuiEx.Text(ExcelItemHelper.GetName(t.Key));
                        }

                        ImGui.TableNextColumn();
                        Utils.DrawColoredGilText(-t.Value);
                    }
                    ImGui.EndTable();
                }
            }
            else
            {
                {
                    if(S.MainWindow.TabTradeLog.CharasGivenGilTo.TryGetValue(SelectedCID, out var data))
                    {
                        ImGuiEx.Text($"Gil given to, top-5:");
                        if(ImGuiEx.BeginDefaultTable(["Character", "~Total Gil"]))
                        {
                            foreach(var t in data.OrderByDescending(x => x.Value))
                            {
                                ImGui.TableNextRow();
                                ImGui.TableNextColumn();
                                {
                                    ImGuiEx.Text(S.MainWindow.CIDMap.TryGetValue(t.Key, out var s) ? Censor.Character(s.ToString()) : Censor.Hide($"{t.Key:X16}"));
                                }

                                ImGui.TableNextColumn();
                                Utils.DrawColoredGilText(-t.Value);
                            }
                            ImGui.EndTable();
                        }
                    }
                }
                {
                    if(S.MainWindow.TabTradeLog.CharasRecvGilFrom.TryGetValue(SelectedCID, out var data))
                    {
                        ImGuiEx.Text($"Gil received from, top-5:");
                        if(ImGuiEx.BeginDefaultTable(["Character", "~Total Gil"]))
                        {
                            foreach(var t in data.OrderByDescending(x => x.Value))
                            {
                                ImGui.TableNextRow();
                                ImGui.TableNextColumn();
                                {
                                    ImGuiEx.Text(S.MainWindow.CIDMap.TryGetValue(t.Key, out var s) ? Censor.Character(s.ToString()) : Censor.Hide($"{t.Key:X16}"));
                                }

                                ImGui.TableNextColumn();
                                Utils.DrawColoredGilText(t.Value);
                            }
                            ImGui.EndTable();
                        }
                    }
                }
            }
        }
        void drawStats()
        {
            if(ImGuiEx.BeginDefaultTable(["Your Character", "~Total Gil", "Diff", "Date"]))
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
                    ImGuiEx.Text($"{t.GilPlayer + t.GilRetainer:N0}");

                    ImGui.TableNextColumn();
                    Utils.DrawColoredGilText(t.Diff);

                    ImGui.TableNextColumn();
                    ImGuiEx.Text(DateTimeOffset.FromUnixTimeMilliseconds(t.UnixTime).ToLocalTime().ToString());
                }
                ImGui.EndTable();
            }
        }
        void drawTotals()
        {
            if(ImGuiEx.BeginDefaultTable(["Your Character", "~Total Gil"]))
            {
                foreach(var t in this.GilByCharaSum)
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    {
                        ImGuiEx.Text(S.MainWindow.CIDMap.TryGetValue(t.Key, out var s) ? Censor.Character(s.ToString()) : Censor.Hide($"{t.Key:X16}"));
                    }

                    ImGui.TableNextColumn();
                    ImGuiEx.Text($"{t.Value:N0}");
                }
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                {
                    ImGuiEx.Text("TOTAL:");
                }

                ImGui.TableNextColumn();
                ImGuiEx.Text($"{GilByCharaSum.Values.Sum():N0}");
                ImGui.EndTable();
            }
        }
        void drawAverages()
        {
            if(ImGuiEx.BeginDefaultTable("Averages", ["1", "2"], false, ImGuiTableFlags.SizingStretchSame | ImGuiTableFlags.BordersInner, true))
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGuiEx.Text($"Total profit/loss per last year:");
                Utils.DrawColoredGilText(this.TotalDiff);
                ImGui.TableNextColumn();
                ImGuiEx.Text($"Average per day:");
                Utils.DrawColoredGilText(this.TotalDiff / 365);
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGuiEx.Text($"Average per week:");
                Utils.DrawColoredGilText(this.TotalDiff / (365/7));
                ImGui.TableNextColumn();
                ImGuiEx.Text($"Average per month:");
                Utils.DrawColoredGilText(this.TotalDiff / 12);
                ImGui.EndTable();
            }
        }
        if(ImGui.CollapsingHeader("Debug"))
        {
            foreach(var x in GilByDate)
            {
                ImGuiEx.Text($"{x.Key} / {x.Key.GetTotalDays()}");
                ImGui.Indent();
                foreach(var z in x.Value)
                {
                    ImGuiEx.Text($"{S.MainWindow.CIDMap.SafeSelect(z.Key)}: {z.Value:N0}");
                }
                ImGui.Unindent();
            }
        }
    }

    public override void DrawSearchBar(out bool updateBlocked)
    {
        updateBlocked = false;
        if(S.WorkerThread.IsBusy) return;
        ImGuiEx.SetNextItemFullWidth();
        if(ImGuiEx.Combo<ulong>("##selectPlayer", ref SelectedCID, [0, .. RecordedCIDs], names: [new KeyValuePair<ulong, string>(0, "All"), .. S.MainWindow.CIDMap.Where(x => RecordedCIDs.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value.ToString())]))
        {
            Data.Clear();
            NeedsUpdate = true;
            updateBlocked = true;
        }
    }

    public override bool ShouldAddData(GilRecordSqlDescriptor data)
    {
        if(!RecordedCIDs.Contains(data.CidUlong)) RecordedCIDs.Add(data.CidUlong);
        if(SelectedCID != 0) return data.CidUlong == SelectedCID;
        return base.ShouldAddData(data);
    }

    public override List<GilRecordSqlDescriptor> LoadData()
    {
        PrevGil.Clear();
        RecordedCIDs.Clear();
        GilByDate.Clear();
        AutoFit = true;
        TotalDiff = 0;
        this.TopItemsPurchased.Clear();
        this.TopItemsSold.Clear();
        return P.DataProvider.GetGilRecords();
    }

    public override void OnPostLoadDataAsync(List<GilRecordSqlDescriptor> newData)
    {
        GilByCharaSum.Clear();
        GilByDateSum.Clear();
        PopulateGilByDateSum();
        foreach(var d in GilByDate.OrderByDescending(x => x.Key))
        {
            foreach(var x in d.Value)
            {
                if(!GilByCharaSum.ContainsKey(x.Key))
                {
                    GilByCharaSum[x.Key] = x.Value;
                }
            }
        }
        Svc.Framework.RunOnFrameworkThread(() =>
        {
            S.MainWindow.TabNpcPurchases.Load(true, false);
            S.MainWindow.TabNpcSales.Load(true, false);
            S.MainWindow.TabRetainerSales.Load(true, false);
            S.MainWindow.TabShopPurchases.Load(true, false);
            S.MainWindow.TabTradeLog.Load(true, false);
            S.WorkerThread.Enqueue(() =>
            {
                this.TopItemsSold = [.. S.MainWindow.TabNpcSales.ItemValues.Union(S.MainWindow.TabRetainerSales.ItemValues).OrderByDescending(x => x.Value).Take(5)];
                this.TopItemsPurchased = [.. S.MainWindow.TabNpcPurchases.ItemValues.Union(S.MainWindow.TabShopPurchases.ItemValues).OrderByDescending(x => x.Value).Take(5)];
            });
        });
    }

    public override bool ProcessSearchByItem(GilRecordSqlDescriptor x)
    {
        return true;
    }

    public override bool ProcessSearchByName(GilRecordSqlDescriptor x)
    {
        var name = S.MainWindow.CIDMap.SafeSelect(x.CidUlong);
        return name.ToString().Contains(SearchName, StringComparison.OrdinalIgnoreCase);
    }

    public override void AddData(GilRecordSqlDescriptor data, List<GilRecordSqlDescriptor> list)
    {
        base.AddData(data, list);
        var cur = data.GilPlayer + data.GilRetainer;
        data.Diff = PrevGil.ContainsKey(data.CidUlong)?cur - PrevGil[data.CidUlong]:0;
        PrevGil[data.CidUlong] = cur;
        var date = Utils.GetLocalDateFromUnixTime(data.UnixTime);
        GilByDate.GetOrCreate(date)[data.CidUlong] = cur;
        if(DateTimeOffset.Now.ToUnixTimeMilliseconds() - data.UnixTime < 365 * 24 * 60 * 60 * 1000L)
        {
            TotalDiff += data.Diff;
        }
    }
}