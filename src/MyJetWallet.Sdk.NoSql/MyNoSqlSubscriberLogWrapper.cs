using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MyNoSqlServer.DataReader;

namespace MyJetWallet.Sdk.NoSql;

public class MyNoSqlSubscriberLogWrapper: IMyNoSqlSubscriber
{
    private readonly IMyNoSqlSubscriber _client;
    private readonly string _name;

    public MyNoSqlSubscriberLogWrapper(IMyNoSqlSubscriber client, string name)
    {
        _client = client;
        _name = name;
    }

    public void Subscribe<T>(string tableName, 
        Action<IReadOnlyList<T>> initAction, 
        Action<string, IReadOnlyList<T>> initPartitionAction, 
        Action<IReadOnlyList<T>> updateAction,
        Action<IEnumerable<(string partitionKey, string rowKey)>> deleteActions)
    {
        var init = new InitActionWrapper<T>(_name, tableName, initAction);
        var initPk = new InitPartitionActionWrapper<T>(_name, tableName, initPartitionAction);
        var update = new UpdateActionWrapper<T>(_name, tableName, updateAction);
        var delete = new DeleteActionsWrapper<T>(_name, tableName, deleteActions);
        
        _client.Subscribe<T>(tableName, init.Action, initPk.Action, update.Action, delete.Action);
    }

    public class InitActionWrapper<T>
    {
        private readonly string _name;
        private readonly string _tableName;
        private readonly Action<IReadOnlyList<T>> _initAction;

        public InitActionWrapper(string name, string tableName, Action<IReadOnlyList<T>> initAction)
        {
            _name = name;
            _tableName = tableName;
            _initAction = initAction;
        }
        
        public void Action(IReadOnlyList<T> items)
        {
            Console.WriteLine($"[{_name}][{_tableName}]: init action, receive {items.Count}");
            
            var sw = new Stopwatch();
            sw.Start();
            _initAction.Invoke(items);
            sw.Stop();
            
            Console.WriteLine($"[{_name}][{_tableName}]: init action DONE ({sw.Elapsed})");
        }
    }
    
    public class InitPartitionActionWrapper<T>
    {
        private readonly string _name;
        private readonly string _tableName;
        private readonly Action<string, IReadOnlyList<T>> _action;

        public InitPartitionActionWrapper(string name, string tableName, Action<string, IReadOnlyList<T>> action)
        {
            _name = name;
            _tableName = tableName;
            _action = action;
        }
        
        public void Action(string pk, IReadOnlyList<T> items)
        {
            Console.WriteLine($"[{_name}][{_tableName}]: init partition ({pk}), receive {items.Count}");

            var sw = new Stopwatch();
            sw.Start();
            _action.Invoke(pk, items);
            sw.Stop();
            
            Console.WriteLine($"[{_name}][{_tableName}]: init partition ({pk}) DONE ({sw.Elapsed})");
        }
    }
    
    public class UpdateActionWrapper<T>
    {
        public static bool LogStartEnabled { get; set; } = true;
        public static bool LogStopEnabled { get; set; } = true;
        
        private readonly string _name;
        private readonly string _tableName;
        private readonly Action<IReadOnlyList<T>> _action;

        public UpdateActionWrapper(string name, string tableName, Action<IReadOnlyList<T>> action)
        {
            _name = name;
            _tableName = tableName;
            _action = action;
        }
        
        public void Action(IReadOnlyList<T> items)
        {
            if (LogStartEnabled) Console.WriteLine($"[{_name}][{_tableName}]: update, receive {items.Count}");

            var sw = new Stopwatch();
            sw.Start();
            _action.Invoke(items);
            sw.Stop();
            
            if (LogStopEnabled) Console.WriteLine($"[{_name}][{_tableName}]: update DONE ({sw.Elapsed})");
        }
    }
    
    public class DeleteActionsWrapper<T>
    {
        public static bool LogEnabled { get; set; } = true;
        
        private readonly string _name;
        private readonly string _tableName;
        private readonly Action<IEnumerable<(string partitionKey, string rowKey)>> _action;

        public DeleteActionsWrapper(string name, string tableName, Action<IEnumerable<(string partitionKey, string rowKey)>> action)
        {
            _name = name;
            _tableName = tableName;
            _action = action;
        }
        
        public void Action(IEnumerable<(string partitionKey, string rowKey)> items)
        {
            var data = items.ToList();
            if (LogEnabled) Console.WriteLine($"[{_name}][{_tableName}]: delete, receive {data.Count()}");

            var sw = new Stopwatch();
            sw.Start();
            _action.Invoke(data);
            sw.Stop();
            
            if (LogEnabled) Console.WriteLine($"[{_name}][{_tableName}]: delete DONE ({sw.Elapsed})");
        }
    }
}