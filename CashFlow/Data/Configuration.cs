using ECommons.Configuration;
using NightmareUI.Censoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashFlow.Data;
public class Configuration : IEzConfig
{
    public CensorConfig CensorConfig = new();
    public int PerPage = 1000;
    public bool MergeTrades = true;
    public int MergeTradeTreshold = 5;
    public int LastGilTradesMin = 5;
    public bool ShowTradeOverlay = false;
    public Dictionary<ulong, long> CachedRetainerGil = [];
    public bool ReverseArrows = false;
}
