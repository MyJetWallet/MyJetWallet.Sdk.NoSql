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
    private readonly MyNoSqlTcpClient _noSqlClient;

    public ApplicationLifetimeManager(IHostApplicationLifetime appLifetime, MyNoSqlTcpClient noSqlClient)
        : base(appLifetime)
    {
        _noSqlClient = noSqlClient;
    }

    protected override void OnStarted()
    {
        _noSqlClient.Start();
    }

    protected override void OnStopping()
    {
        _noSqlClient.Stop();
    }
}
```

**Model:**

```csharp
[DataContract]
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
            RowKey = GenerateRowKey(trader.Id),
            Trade = trade
        };
    }
}
```
