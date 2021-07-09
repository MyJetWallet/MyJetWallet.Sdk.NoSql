using MyJetWallet.Sdk.Service.Tools;
using MyNoSqlServer.DataReader;

namespace MyJetWallet.Sdk.NoSql
{
    public interface IMyNoSqlTcpClientManager
    {
        void Start();
        void Stop();
    }

    public class MyNoSqlTcpClientManager : IMyNoSqlTcpClientManager
    {
        private readonly MyNoSqlTcpClient _client;

        public MyNoSqlTcpClientManager(MyNoSqlTcpClient client)
        {
            _client = client;
        }

        public void Start()
        {
            _client.Start();
        }

        public void Stop()
        {
            _client.Stop();
        }
    }
}