# using

**Module:**

```csharp
public class ServiceBusModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var noSqlClient = builder.CreateNoSqlClient(Program.Settings.MyNoSqlReaderHostPort, Program.LoggerFactory);
        
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
    private readonly MyNoSqlClientLifeTime _myNoSqlClientLifeTime;

    public ApplicationLifetimeManager(IHostApplicationLifetime appLifetime, MyNoSqlClientLifeTime myNoSqlClientLifeTime)
        : base(appLifetime)
    {
        _myNoSqlClientLifeTime = myNoSqlClientLifeTime;
    }

    protected override void OnStarted()
    {
        _myNoSqlClientLifeTime.Start();
    }

    protected override void OnStopping()
    {
        _myNoSqlClientLifeTime.Stop();
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
