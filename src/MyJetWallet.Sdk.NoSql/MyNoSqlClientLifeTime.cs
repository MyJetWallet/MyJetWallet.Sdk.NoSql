using System;
using Autofac;

namespace MyJetWallet.Sdk.NoSql
{
    public class MyNoSqlClientLifeTime: IStartable
    {
        private readonly IMyNoSqlTcpClientManager[] _clients;

        public MyNoSqlClientLifeTime(IMyNoSqlTcpClientManager[] clients)
        {
            _clients = clients;
        }

        public void Start()
        {
            foreach (var client in _clients)
            {
                client.Start();
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