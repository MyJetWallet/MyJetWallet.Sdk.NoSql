using System;

namespace MyJetWallet.Sdk.NoSql
{
    public interface INoSqlReaderCountDataGetter
    {
        (int, string, NoSqlDataWaitMode) CountEntities();
    }

    public enum NoSqlDataWaitMode
    {
        None,
        WaitAndContinue,
        WaitAndThrow
    }
    
    public class NoSqlReaderCountDataGetter: INoSqlReaderCountDataGetter
    {
        private readonly Func<int> _countCallback;
        private readonly string _typeName;
        private readonly NoSqlDataWaitMode _mode;

        public NoSqlReaderCountDataGetter(Func<int> countCallback, string typeName, NoSqlDataWaitMode mode)
        {
            _countCallback = countCallback ?? throw new ArgumentException("countCallback cannot be null", nameof(countCallback));
            _typeName = typeName;
            _mode = mode;
        }

        public (int, string, NoSqlDataWaitMode) CountEntities()
        {
            return (_countCallback.Invoke(), _typeName, _mode);
        }
    }
}