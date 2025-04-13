using CashFlow.Gui;
using ECommons.Schedulers;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CashFlow.Services;
public static class ServiceManager
{
    public static MainWindow MainWindow;
    public static MemoryManager MemoryManager;
    public static CommandManager CommandManager;
    public static TradeDetectionManager TradeDetectionManager;
    public static EventWatcher EventWatcher;
    public static WorkerThread WorkerThread;
    public static TradeOverlay TradeOverlay;
}
