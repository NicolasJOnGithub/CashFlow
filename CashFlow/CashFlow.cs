using CashFlow.Data;
using CashFlow.Data.SqlDescriptors;
using CashFlow.DataProvider;
using CashFlow.DataProvider.Sqlite;
using ECommons.Configuration;
using ECommons.Events;
using ECommons.GameHelpers;
using ECommons.Singletons;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using NightmareUI.Censoring;

namespace CashFlow;

public sealed unsafe class CashFlow : IDalamudPlugin
{
    public static CashFlow P;
    private Configuration Configuration;
    public DataProviderBase DataProvider;
    public static Configuration C => P.Configuration;
    public CashFlow(IDalamudPluginInterface dalamudPluginInterface)
    {
        P = this;
        ECommonsMain.Init(dalamudPluginInterface, this);
        Configuration = EzConfig.Init<Configuration>();
        Censor.Config = Configuration.CensorConfig;
        DataProvider = new SqliteDataProvider();
        SingletonServiceManager.Initialize(typeof(ServiceManager));
        new TickScheduler(() =>
        {
            ProperOnLogin.RegisterAvailable(OnLogin, true);
        });
        Svc.Framework.Update += Framework_Update;
        Svc.Condition.ConditionChange += Condition_ConditionChange;
    }

    private void Condition_ConditionChange(ConditionFlag flag, bool value)
    {
        if(flag == ConditionFlag.OccupiedSummoningBell && !value)
        {
            EzConfig.Save();
        }
        if(Player.CID != 0)
        {
            if(flag.EqualsAny(ConditionFlag.LoggingOut, ConditionFlag.BetweenAreas) && value)
            {
                var entry = new GilRecordSqlDescriptor()
                {
                    CidUlong = Player.CID,
                    GilPlayer = InventoryManager.Instance()->GetInventoryItemCount(1),
                    GilRetainer = Utils.GetCurrentOrCachedPlayerRetainerGil(),
                    UnixTime = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                };
                DataProvider.RecordPlayerGil(entry);
            }
        }
    }

    private void Framework_Update(Dalamud.Plugin.Services.IFramework framework)
    {
        if(Svc.Condition[ConditionFlag.OccupiedSummoningBell])
        {
            if(RetainerManager.Instance()->IsReady && EzThrottler.Throttle("SaveRetainerGil"))
            {
                C.CachedRetainerGil[Player.CID] = Utils.GetCurrentPlayerRetainerGil();
            }
        }
    }

    private void OnLogin()
    {
        DataProvider.RecordPlayerCID(Player.Object);
    }

    public void Dispose()
    {
        Svc.Condition.ConditionChange -= Condition_ConditionChange;
        Svc.Framework.Update -= Framework_Update;
        ECommonsMain.Dispose();
        P = null;
    }
}
