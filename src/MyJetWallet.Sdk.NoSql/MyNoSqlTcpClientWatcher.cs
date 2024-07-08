using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.LivnesProbs;
using MyJetWallet.Sdk.Service.Tools;
using MyNoSqlServer.Abstractions;
using MyNoSqlServer.DataReader;

namespace MyJetWallet.Sdk.NoSql
{
    public interface IMyNoSqlTcpClientWatcher
    {
        void Start();
    }
    
    public class MyNoSqlTcpClientWatcher : IDisposable, ILivenessReporter, IMyNoSqlTcpClientWatcher
    {
        private readonly MyNoSqlTcpClient _myNoSqlTcpClient;
        private readonly ILogger<MyNoSqlTcpClientWatcher> _logger;
        private readonly MyTaskTimer _timer;
        private readonly IMyNoSqlServerDataReader<LivenessNoSqlEntity> _livenessReader;
        private DateTime _startTime;

        private DateTime? _lastLossConnect = null;
        // private DateTime? _lastConnectedAt = null;

        public MyNoSqlTcpClientWatcher(MyNoSqlTcpClient myNoSqlTcpClient, ILogger<MyNoSqlTcpClientWatcher> logger, IMyNoSqlServerDataReader<LivenessNoSqlEntity> livenessReader)
        {
            _myNoSqlTcpClient = myNoSqlTcpClient;
            _logger = logger;
            _livenessReader = livenessReader;

            _timer = new MyTaskTimer(nameof(MyNoSqlTcpClientWatcher), TimeSpan.FromSeconds(10), logger, Watch);
        }

        public void Start()
        {
            _startTime = DateTime.UtcNow;
            _timer.Start();
        }

        private Task Watch()
        {
            if ((DateTime.UtcNow - _startTime).TotalSeconds < 30)
                return Task.CompletedTask;
            
            if (!_myNoSqlTcpClient.Connected)
            {
                _logger.LogError("MyNoSqlTcpClient DO NOT CONNECTED, please start the client and validate url and connection");
                
                _lastLossConnect ??= DateTime.UtcNow;
            }
            else
            {
                _lastLossConnect = null;
            }
            
            var probe = _livenessReader.Get(LivenessNoSqlEntity.GeneratePartitionKey(), LivenessNoSqlEntity.GenerateRowKey());

            if (probe.LastUpde < DateTime.UtcNow.AddSeconds(-180))
            {
                _logger.LogError("Liveness probe is too old. Last update: {time}", probe.LastUpde);
                _myNoSqlTcpClient.ReCreateAndStart();
                _startTime = DateTime.UtcNow;
                return Task.CompletedTask;
            }       

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer.Stop();
            _timer.Dispose();
        }

        public (string, List<string>) GetIssues()
        {
            if ((DateTime.UtcNow - _startTime).TotalSeconds < 30)
                return ("MyNoSqlTcpClientWatcher", new List<string>());

            var ts = _lastLossConnect;
            
            if (ts != null && (DateTime.UtcNow - ts.Value).TotalSeconds > 30)
            {
                return ("MyNoSqlTcpClientWatcher", new List<string>()
                {
                    "MyNoSqlTcpClient DO NOT CONNECTED!"
                });
            }
            
            return ("MyNoSqlTcpClientWatcher", new List<string>());
        }
    }
}
