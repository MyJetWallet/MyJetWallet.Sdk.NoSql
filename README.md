# using

**Module:**

```csharp
public class ServiceBusModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var noSqlClient = builder.CreateNoSqlClient(Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort));
        
        // register writer (IMyNoSqlServerDataWriter<PortfolioTradeNoSql>)
        builder.RegisterMyNoSqlWriter<PortfolioTradeNoSql>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl), PortfolioTradeNoSql.TableName);
        
        // register reader (IMyNoSqlServerDataReader<PortfolioTradeNoSql>)
        builder.RegisterMyNoSqlReader<PortfolioTradeNoSql>(noSqlClient, PortfolioTradeNoSql.TableName);
    }
}
```

**LifeTime:**

```csharp
public class ApplicationLifetimeManager : ApplicationLifetimeManagerBase
{
    private readonly MyNoSqlTcpClient[] _myNoSqlTcpClientManagers;

    public ApplicationLifetimeManager(IHostApplicationLifetime appLifetime, MyNoSqlTcpClient[] myNoSqlTcpClientManagers)
        : base(appLifetime)
    {
        _myNoSqlTcpClientManagers = myNoSqlTcpClientManagers;
    }

    protected override void OnStarted()
    {
        foreach(var client in _myNoSqlTcpClientManagers)
        {
            client.Start();
        }
    }

    protected override void OnStopping()
    {
        foreach(var client in _myNoSqlTcpClientManagers)
        {
            try
            {
                client.Start();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex)
            }
        }
    }
}
```

**Model:**

```csharp

public class PortfolioTradeNoSql : MyNoSqlDbEntity
{
    public const string TableName = "myjetwallet-liquitity-portfoliotrade";

    public static string GeneratePartitionKey(string instrumentSymbol) => instrumentSymbol;
    public static string GenerateRowKey(string tradeId) => tradeId;

    public PortfolioTrade Trade { get; set; }

    public static PortfolioTradeNoSql Create(PortfolioTrade trade)
    {
        return new SettingsLiquidityConverterNoSql()
        {
            PartitionKey = GeneratePartitionKey(trade.InstrumentSymbol),
            RowKey = GenerateRowKey(trade.Id),
            Trade = trade
        };
    }
}
```
