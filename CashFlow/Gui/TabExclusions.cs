using Dalamud.Interface.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashFlow.Gui;
public unsafe static class TabExclusions
{
    static string Search = "";
    public static void Draw()
    {
        ImGuiEx.TextWrapped($"Here you can select certain data to be hidden for certain characters. Please note that even if you will hide it, entries will still be recorded unless the character is fully blacklisted from data creation.");
        if(S.WorkerThread.IsBusy)
        {
            ImGuiEx.Text($"Loading, please wait...");
            return;
        }
        ImGuiEx.SetNextItemFullWidth();
        ImGui.InputTextWithHint("##search", "Search...", ref Search, 50);
        if(ImGuiEx.BeginDefaultTable(["~Character", "Data Types", "##blacklist"]))
        {
            foreach(var x in S.MainWindow.CIDMap)
            {
                var charaName = x.Value.ToString();
                if(Search.Length > 0 && !charaName.Contains(Search, StringComparison.OrdinalIgnoreCase)) continue;
                ImGui.PushID(x.Key.ToString());
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGuiEx.TextV($"{charaName}");
                ImGui.TableNextColumn();

                if(C.Blacklist.Contains(x.Key))
                {
                    if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.UserCheck, "Unblacklist character"))
                    {
                        C.Blacklist.Remove(x.Key);
                    }
                }
                else
                {
                    DrawToggleSetting(FontAwesomeIcon.ChartLine, x.Key, C.DisplayExclusionsGilHistory, "Display gil history");
                    DrawToggleSetting(FontAwesomeIcon.PersonCirclePlus, x.Key, C.DisplayExclusionsNpcPurchases, "Display NPC purchases");
                    DrawToggleSetting(FontAwesomeIcon.PersonCircleMinus, x.Key, C.DisplayExclusionsNpcSales, "Display NPC sales");
                    DrawToggleSetting(FontAwesomeIcon.CartPlus, x.Key, C.DisplayExclusionsShopPurchasesNormal, "Display normal Market Board purchases");
                    DrawToggleSetting(FontAwesomeIcon.ShoppingCart, x.Key, C.DisplayExclusionsRetainerSalesNormal, "Display normal Retainer Sales");
                    DrawToggleSetting(FontAwesomeIcon.UserPlus, x.Key, C.DisplayExclusionsShopPurchasesMannequinn, "Display purchases from Mannequinn");
                    DrawToggleSetting(FontAwesomeIcon.UserMinus, x.Key, C.DisplayExclusionsRetainerSalesMannequinn, "Display mannequinn sales");
                    DrawToggleSetting(FontAwesomeIcon.List, x.Key, C.DisplayExclusionsTradeLog, "Display trade log");

                    ImGui.TableNextColumn();
                    if(ImGuiEx.IconButton(FontAwesomeIcon.UserSlash, enabled: ImGuiEx.Ctrl && ImGuiEx.Shift))
                    {
                        C.Blacklist.Add(x.Key);
                        P.DataProvider.PurgeAllRecords(x.Key);
                    }
                    ImGuiEx.Tooltip($"Blacklist {charaName} from the plugin and IRRECOVERABLY DELETE ALL PREVIOUSLY RECORDED DATA FOR IT. \n\nThis will completely remove all the data associated with {charaName} from the database, and will prevent any entries for {charaName} from being created in future.\n\nHold SHIFT and CTRL and click this button.");
                }

                ImGui.PopID();
            }
            ImGui.EndTable();
        }
    }

    static void DrawToggleSetting(FontAwesomeIcon icon, ulong cid, ICollection<ulong> collection, string tt)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        ImGuiEx.CollectionButtonCheckbox(icon.ToIconString(), cid, collection, color:ImGuiColors.HealerGreen, inverted: true);
        ImGui.PopFont();
        ImGuiEx.Tooltip(tt);
        ImGui.SameLine();
    }
}
