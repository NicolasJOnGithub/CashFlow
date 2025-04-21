using CashFlow.Data;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.SimpleGui;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace CashFlow.Gui;
public unsafe class TradeOverlay : Window
{
    private long GilSent = 0;
    private long GilReceived = 0;
    private List<ItemWithQuantity> Items = [];
    private uint LastFrame = 0;
    private ulong TradePartner = 0;
    private volatile bool NeedUpdate = true;
    public TradeOverlay() : base("GilSight_TradeOverlay", ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse)
    {
        EzConfigGui.WindowSystem.AddWindow(this);
        IsOpen = true;
        RespectCloseHotkey = false;
    }

    public override void Draw()
    {
        DrawInternal();
        if(TryGetAddonByName<AtkUnitBase>("Trade", out var addon) && addon->IsReady())
        {
            short x;
            short y;
            addon->GetPosition(&x, &y);
            Position = new Vector2(x + addon->GetScaledWidth(true), y + ImGui.GetStyle().WindowPadding.Y);
            SizeConstraints = new()
            {
                MinimumSize = new(0, 0),
                MaximumSize = new(float.MaxValue, addon->GetScaledHeight(true) - ImGui.GetStyle().WindowPadding.Y * 2)
            };
        }
    }

    public void DrawInternal()
    {
        if(CSFramework.Instance()->FrameCounter > LastFrame + 1)
        {
            NeedUpdate = true;
        }
        LastFrame = CSFramework.Instance()->FrameCounter;
        if(S.WorkerThread.IsBusy)
        {
            ImGuiEx.Text("Loading, please wait...");
            return;
        }
        if(TryGetAddonByName<AtkUnitBase>("Trade", out var addon) && addon->IsReady())
        {
            var partner = Utils.GetTradePartner().Struct()->ContentId;
            if(NeedUpdate)
            {
                NeedUpdate = false;
                S.WorkerThread.Enqueue(() =>
                {
                    var data = P.DataProvider.GetTrades(DateTimeOffset.Now.ToUnixTimeMilliseconds() - C.LastGilTradesMin * 60 * 1000);
                    var sent = 0;
                    var received = 0;
                    List<ItemWithQuantity> items = [];
                    foreach(var x in data)
                    {
                        if(x.TradePartnerCID == partner)
                        {
                            if(x.ReceivedGil > 0)
                            {
                                received += x.ReceivedGil;
                            }
                            else
                            {
                                sent -= x.ReceivedGil;
                            }
                            if(x.ReceivedItems != null)
                            {
                                foreach(var item in x.ReceivedItems)
                                {
                                    if(items.TryGetFirst(s => s.ItemID == item.ItemID, out var result))
                                    {
                                        result.Quantity += item.Quantity;
                                    }
                                    else
                                    {
                                        items.Add(item);
                                    }
                                }
                            }
                            items.RemoveAll(x => x.Quantity == 0);
                        }
                    }
                    Svc.Framework.RunOnFrameworkThread(() =>
                    {
                        GilReceived = received;
                        GilSent = sent;
                        Items = items;
                        var p = Utils.GetTradePartner();
                        if(p != null)
                        {
                            TradePartner = p.Struct()->ContentId;
                        }
                    });
                });
            }
        }
        if(!NeedUpdate)
        {
            ImGuiEx.Text($"Within last {C.LastGilTradesMin} minutes");
            ImGuiEx.Text(EColor.Red, $"↗ {GilSent:N0}");
            ImGuiEx.Tooltip("Gave");
            ImGuiEx.Text(EColor.Green, $"↘ {GilReceived:N0}");
            ImGuiEx.Tooltip("Received");
            if(Items.Count > 0)
            {
                if(ImGuiEx.BeginDefaultTable($"TradeGiven", ["##type", "Item", "Qty"], false, ImGuiEx.DefaultTableFlags & ~ImGuiTableFlags.Borders | ImGuiTableFlags.BordersInnerV, true))
                {
                    var itemsSent = Items.Where(x => x.Quantity < 0);
                    var itemsReceived = Items.Where(x => x.Quantity > 0);
                    if(itemsSent.Any())
                    {
                        foreach(var x in itemsSent)
                        {
                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            Utils.DrawGilDecrease(false);
                            ImGuiEx.Tooltip("Gave item");
                            ImGui.TableNextColumn();
                            ImGuiEx.Text(ExcelItemHelper.GetName(x.ItemID % 1000000));
                            ImGui.TableNextColumn();
                            ImGuiEx.Text($"x{Math.Abs(x.Quantity)}" + (x.ItemID > 1000000 ? "" : ""));
                        }
                    }
                    if(itemsReceived.Any())
                    {
                        foreach(var x in itemsReceived)
                        {
                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            Utils.DrawGilIncrease(false);
                            ImGuiEx.Tooltip("Received item");
                            ImGui.TableNextColumn();
                            ImGuiEx.Text(ExcelItemHelper.GetName(x.ItemID % 1000000));
                            ImGui.TableNextColumn();
                            ImGuiEx.Text($"x{Math.Abs(x.Quantity)}" + (x.ItemID > 1000000 ? "" : ""));
                        }
                    }
                    ImGui.EndTable();
                }
            }
        }
    }

    public override bool DrawConditions()
    {
        return C.ShowTradeOverlay && Svc.Condition[ConditionFlag.TradeOpen] && Utils.GetTradePartner() != null;
    }
}
