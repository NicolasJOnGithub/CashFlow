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
    public int MergeTradeTreshold = 10;
    public int LastGilTradesMin = 30;
    public bool ShowTradeOverlay = false;
    public bool ReverseArrows = false;
    public bool ReverseDayMonth = true;
    public bool UseUTCTime = false;
    public bool UseCustomTimeFormat = false;
    public string CustomTimeFormat = "MM.dd.yyyy HH:mm";
    public Dictionary<ulong, long> CachedRetainerGil = [];
}
