using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashFlow.Data.LegacyDescriptors;
public class RetainerSaleDescriptor : IEquatable<RetainerSaleDescriptor>, IDescriptorBase
{
    public ulong CidUlong { get; set; }
    public string RetainerName;
    public int ItemID;
    public int Quantity;
    public int Price;
    public bool IsMannequinn;
    public string BuyerName;
    public long UnixTime { get; set; }

    public override bool Equals(object obj)
    {
        return Equals(obj as RetainerSaleDescriptor);
    }

    public bool Equals(RetainerSaleDescriptor other)
    {
        return other is not null &&
               CidUlong == other.CidUlong &&
               RetainerName == other.RetainerName &&
               ItemID == other.ItemID &&
               Quantity == other.Quantity &&
               Price == other.Price &&
               IsMannequinn == other.IsMannequinn &&
               BuyerName == other.BuyerName &&
               UnixTime == other.UnixTime;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(CidUlong, RetainerName, ItemID, Quantity, Price, IsMannequinn, BuyerName, UnixTime);
    }

    public static bool operator ==(RetainerSaleDescriptor left, RetainerSaleDescriptor right)
    {
        return EqualityComparer<RetainerSaleDescriptor>.Default.Equals(left, right);
    }

    public static bool operator !=(RetainerSaleDescriptor left, RetainerSaleDescriptor right)
    {
        return !(left == right);
    }
}
