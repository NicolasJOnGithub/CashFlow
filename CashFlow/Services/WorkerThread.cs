using CashFlow.Data;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Action = System.Action;

namespace CashFlow.Services;
public unsafe class WorkerThread : IDisposable
{
    private BlockingCollection<Action> Queue = [];
    private bool ThreadRunning = false;
    public volatile bool IsBusy = false;
    private WorkerThread()
    {
    }

    public void Dispose()
    {
        Queue.CompleteAdding();
        Queue.Dispose();
    }

    public void ClearAndEnqueue(Action a)
    {
        Clear();
        Enqueue(a);
    }

    public void Enqueue(Action a)
    {
        try
        {
            Queue.Add(a);
            if(!ThreadRunning)
            {
                ThreadRunning = true;
                new Thread(Run).Start();
            }
        }
        catch(ObjectDisposedException e)
        {
            e.LogDebug();
        }
        catch(Exception e)
        {
            e.Log();
        }
    }

    public void Clear()
    {
        try
        {
            while(Queue.TryTake(out _, 0)) ;
        }
        catch(ObjectDisposedException e)
        {
            e.LogDebug();
        }
        catch(Exception e)
        {
            e.Log();
        }
    }

    private void Run()
    {
        try
        {
            PluginLog.Debug($"Thread started");
            while(Queue.TryTake(out var result, -1))
            {
                IsBusy = true;
                try
                {
                    //Thread.Sleep(1000);
                    result();
                }
                catch(Exception e)
                {
                    e.Log();
                }
                IsBusy = false;
            }
            PluginLog.Debug($"Thread stopped");
        }
        catch(ObjectDisposedException e)
        {
            e.LogDebug();
        }
        catch(Exception e)
        {
            e.Log();
        }
    }
}
