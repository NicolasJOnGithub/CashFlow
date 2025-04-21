using ECommons.SimpleGui;

namespace CashFlow.Services;
public class CommandManager
{
    private CommandManager()
    {
        EzCmd.Add("/cashflow", EzConfigGui.Open);
        EzCmd.Add("/cflow", EzConfigGui.Open);
    }
}
