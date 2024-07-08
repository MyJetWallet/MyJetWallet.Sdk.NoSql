using System;
using MyNoSqlServer.Abstractions;

namespace MyJetWallet.Sdk.NoSql;

public class LivenessNoSqlEntity: MyNoSqlDbEntity
{
    public const string TableName = "myjetwallet-liveness";

    public static string GeneratePartitionKey() => "liveness";
    public static string GenerateRowKey() => "liveness";
    public DateTime LastUpde { get; set; }


    public static LivenessNoSqlEntity Create()
    {
        return new LivenessNoSqlEntity()
        {
            PartitionKey = GeneratePartitionKey(),
            RowKey = GenerateRowKey(),
            LastUpde = DateTime.UtcNow
        };
    }
}