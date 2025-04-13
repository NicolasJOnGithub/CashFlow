using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashFlow.Data;
public readonly record struct ItemPrice(int Price, int Amount, bool HQ, string Retainer, int Index)
{
}
