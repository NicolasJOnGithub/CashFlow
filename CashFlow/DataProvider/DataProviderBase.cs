using CashFlow.Data.ExplicitStructs;
using CashFlow.Data.LegacyDescriptors;
using CashFlow.Data.SqlDescriptors;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.ChatMethods;
using ECommons.GameFunctions;

namespace CashFlow.DataProvider;
public abstract unsafe class DataProviderBase
{
    public abstract void RecordPlayerCID(string playerName, uint homeWorld, ulong CID);
    public void RecordPlayerCID(IPlayerCharacter pc)
    {
        RecordPlayerCID(pc.Name.ToString(), pc.HomeWorld.RowId, pc.Struct()->ContentId);
    }
    public abstract void RecordIncomingTrade(TradeDescriptor descriptor);
    public abstract List<TradeDescriptor> GetTrades(long unixTimeMsMin = 0, long unixTimeMsMax = 0);
    public abstract Sender? GetPlayerInfo(ulong CID);
    public abstract void PurgeAllRecords(ulong CID);
    public abstract Dictionary<ulong, Sender> GetRegisteredPlayers();
    public abstract void RecordRetainerHistory(List<RetainerHistoryData> trades, ulong CID, string retainerName);
    public abstract List<RetainerSaleDescriptor> GetRetainerHistory(long unixTimeMsMin = 0, long unixTimeMsMax = 0);
    public abstract void RecordShopPurchase(ShopPurchaseSqlDescriptor shopPurchaseDescriptor);
    public abstract List<ShopPurchaseSqlDescriptor> GetShopPurchases(long unixTimeMsMin = 0, long unixTimeMsMax = 0);
    public abstract void RecordNpcPurchase(NpcPurchaseSqlDescriptor npcPurchaseSqlDescriptor);
    public abstract List<NpcPurchaseSqlDescriptor> GetNpcPurchases(long unixTimeMsMin = 0, long unixTimeMsMax = 0);
    public abstract void RecordNpcSale(NpcSaleSqlDescriptor npcSaleSqlDescriptor);
    public abstract List<NpcSaleSqlDescriptor> GetNpcSales(long unixTimeMsMin = 0, long unixTimeMsMax = 0);
    public abstract void RecordPlayerGil(GilRecordSqlDescriptor gilRecordSqlDescriptor);
    public abstract List<GilRecordSqlDescriptor> GetGilRecords(long unixTimeMsMin = 0, long unixTimeMsMax = 0);
}
