using ECommons.SimpleGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashFlow.Services;
public class CommandManager
{
    private CommandManager()
    {
        EzCmd.Add("/cashflow", EzConfigGui.Open);
        EzCmd.Add("/cflow", EzConfigGui.Open);
    }
}
