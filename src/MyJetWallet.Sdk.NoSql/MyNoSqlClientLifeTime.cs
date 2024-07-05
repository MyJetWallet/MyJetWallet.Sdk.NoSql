using System;
using System.Linq;
using System.Threading;
using Autofac;
using Microsoft.Extensions.Logging;
using Serilog;

namespace MyJetWallet.Sdk.NoSql
{
    public class MyNoSqlClientLifeTime: IStartable
    {
        private readonly IMyNoSqlTcpClientManager[] _clients;
        private readonly INoSqlReaderCountDataGetter[] _countDataGetter;
        private readonly ILogger<MyNoSqlClientLifeTime> _logger;
        private readonly MyNoSqlTcpClientWatcher[] _watcher;

        public MyNoSqlClientLifeTime(
            IMyNoSqlTcpClientManager[] clients, 
            INoSqlReaderCountDataGetter[] countDataGetter,
            ILogger<MyNoSqlClientLifeTime> logger,
            MyNoSqlTcpClientWatcher[] watcher)
        {
            _clients = clients;
            _countDataGetter = countDataGetter;
            _logger = logger;
            _watcher = watcher;
        }

        public void Start()
        {
            foreach (var client in _clients)
            {
                client.Start();
            }
            
            Console.WriteLine("Check and wait NoSql subscribers:");

            var subscribers = _countDataGetter.ToList();
            if (subscribers.Any())
            {
                Thread.Sleep(2000);
            }

            var iteration = 0;
            while (subscribers.Any() && iteration < 10)
            {
                foreach (var subs in subscribers.ToArray())
                {
                    var (count, type, mode) = subs.CountEntities();
                    if (count > 0 || mode == NoSqlDataWaitMode.None)
                    {
                        subscribers.Remove(subs);
                        continue;
                    }
                    Console.WriteLine($"[{iteration}] subscriber to {type} is empty.");
                }

                if (subscribers.Any())
                {
                    iteration++;
                    Console.WriteLine($"Wait data in nosql for 1 second");
                    Thread.Sleep(1000);
                }
            }

            var text = "";
            foreach (var subs in subscribers)
            {
                var (count, type, mode) = subs.CountEntities();
                if (count == 0 && mode == NoSqlDataWaitMode.WaitAndThrow)
                {
                    _logger.LogError("Cannot start application. NoSqlReader for {text} is empty", type);
                    throw new Exception($"Cannot start application. NoSqlReader for {type} is empty");
                }

                if (count == 0 && mode == NoSqlDataWaitMode.WaitAndContinue)
                    text += $", {type}";
            }

            if (!string.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("!!! Start with not filled nosql data. Empty readers for: {text}", text);
            }
            
            if (_watcher != null)
            {
                foreach (var watcher in _watcher)
                {
                    watcher.Start();
                }
            }
            
        }

        public void Stop()
        {
            foreach (var client in _clients)
            {
                try
                {
                    client.Stop();
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"cannot stop NoSqlClient:\n{ex}");
                }
            }
        }
    }
}