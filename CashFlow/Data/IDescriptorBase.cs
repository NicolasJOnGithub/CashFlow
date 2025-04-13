using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashFlow.Data;
public interface IDescriptorBase
{
    ulong CidUlong { get; }
    long UnixTime { get; set; }
}
