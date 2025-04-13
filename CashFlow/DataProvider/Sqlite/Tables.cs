using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashFlow.DataProvider.Sqlite;
public static unsafe class Tables
{
    public static readonly string CidMap = "CidMap";
    public static readonly string NpcSales = "NpcSales";
    public static readonly string NpcPurchases = "NpcPurchases";
    public static readonly string ShopPurchases = "ShopPurchases";
    public static readonly string RetainerSales = "RetainerSales";
    public static readonly string Trades = "Trades";
    public static readonly string GilRecords = "GilRecords";
}
