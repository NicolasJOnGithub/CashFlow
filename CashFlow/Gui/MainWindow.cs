using CashFlow.Data;
using CashFlow.Gui.BaseTabs;
using ECommons.ChatMethods;
using ECommons.SimpleGui;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashFlow.Gui;
public unsafe class MainWindow : ConfigWindow
{
    public volatile Dictionary<ulong, Sender> CIDMap = [];
    public TabTradeLog TabTradeLog = new();
    public TabRetainerSales TabRetainerSales = new();
    public TabShopPurchases TabShopPurchases = new();
    public TabNpcPurchases TabNpcPurchases = new();
    public TabNpcSales TabNpcSales = new();
    public TabGilHistory TabGilHistory = new();

    public void UpdateData()
    {
        TabTradeLog.NeedsUpdate = true;
        TabRetainerSales.NeedsUpdate = true;
        TabShopPurchases.NeedsUpdate = true;
        TabNpcPurchases.NeedsUpdate = true;
        TabNpcSales.NeedsUpdate = true;
        TabGilHistory.NeedsUpdate = true;
    }

    private MainWindow()
    {
        EzConfigGui.Init(this);
    }

    public override void Draw()
    {
        ImGuiEx.EzTabBar("tabs", [
            ("Trade Log", TabTradeLog.Draw, null, true),
            ("Purchase Log", TabShopPurchases.Draw, null, true),
            ("Sale Log", TabRetainerSales.Draw, null, true),
            ("NPC Sale Log", TabNpcSales.Draw, null, true),
            ("NPC Purchase Log", TabNpcPurchases.Draw, null, true),
            ("Gil History", TabGilHistory.Draw, null, true),
            ("Settings", DrawSettings, null, true),
            ("Debug", TabDebug.Draw, ImGuiColors.DalamudGrey3, true),
            ]);
    }

    private void DrawSettings()
    {
        ImGui.SetNextItemWidth(150);
        ImGuiEx.SliderInt("Records per page", ref C.PerPage, 1000, 10000);
        ImGui.Checkbox("Merge sequential gil-only trades with the same player into one", ref C.MergeTrades);
        ImGuiEx.HelpMarker("Does not affects how trades are stored in database, only affects view");
        ImGui.Indent();
        ImGui.SetNextItemWidth(100);
        ImGui.SliderInt($"Time limit for merging trades, minutes", ref C.MergeTradeTreshold, 1, 10);
        ImGui.Unindent();
        ImGui.Checkbox("Change arrows directions", ref C.ReverseArrows);
        ImGuiEx.TextV("Date format:");
        ImGui.SameLine();
        ImGuiEx.RadioButtonBool("Month/Day", "Day.Month", ref C.ReverseDayMonth, sameLine: true, inverted:true);

        ImGui.Separator();
        ImGui.Checkbox("Show recent gil transaction summary while trade window is open", ref C.ShowTradeOverlay);
        ImGui.Indent();
        ImGui.SetNextItemWidth(150f);
        ImGui.InputInt("Time, minutes", ref C.LastGilTradesMin);
        ImGui.Unindent();

        ImGui.Separator();
        ImGui.Checkbox("Censor Names", ref C.CensorConfig.Enabled);
        ImGui.Indent();
        ImGui.Checkbox("Lesser Censor Mode", ref C.CensorConfig.LesserCensor);
        if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Undo, "Reset Censor Seed")) C.CensorConfig.Seed = Guid.NewGuid().ToString();
        ImGui.Unindent();
    }
}
