using System;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.Abstractions;
using MyNoSqlServer.DataReader;
using MyNoSqlServer.DataWriter;

namespace MyJetWallet.Sdk.NoSql
{
    public static class AutofacNoSqlHelper
    {
        public static MyNoSqlTcpClient CreateNoSqlClient(this ContainerBuilder builder, Func<string> readerHostPort)
        {
            var myNoSqlClient = new MyNoSqlTcpClient(
                readerHostPort,
                ApplicationEnvironment.HostName ??
                $"{ApplicationEnvironment.AppName}:{ApplicationEnvironment.AppVersion}");

            builder.RegisterInstance(myNoSqlClient).SingleInstance();
            builder
                .Register(context =>
                {
                    var logger = context.Resolve<ILogger<MyNoSqlTcpClientWatcher>>();
                    var watcher = new MyNoSqlTcpClientWatcher(myNoSqlClient, logger);
                    return watcher;
                })
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();

            builder
                .RegisterInstance(new MyNoSqlTcpClientManager(myNoSqlClient))
                .As<IMyNoSqlTcpClientManager>()
                .SingleInstance();

            builder.RegisterType<MyNoSqlClientLifeTime>().AsSelf().SingleInstance();


            return myNoSqlClient;
        }

        public static IMyNoSqlServerDataWriter<T> RegisterMyNoSqlWriter<T>(this ContainerBuilder builder, Func<string> writerUrl, string tableName, bool persist = true, 
            DataSynchronizationPeriod dataSynchronizationPeriod = DataSynchronizationPeriod.Sec5) where T: IMyNoSqlDbEntity, new()
        {
            var writer = new MyNoSqlServerDataWriter<T>(writerUrl, tableName, persist, dataSynchronizationPeriod);

            builder
                .RegisterInstance(writer)
                .As<IMyNoSqlServerDataWriter<T>>()
                .SingleInstance();

            return writer;
        }

        public static IMyNoSqlServerDataReader<T> RegisterMyNoSqlReader<T>(this ContainerBuilder builder, MyNoSqlTcpClient client, string tableName) where T : IMyNoSqlDbEntity, new()
        {
            var reader = new MyNoSqlReadRepository<T>(client, tableName);

            builder
                .RegisterInstance(reader)
                .As<IMyNoSqlServerDataReader<T>>()
                .SingleInstance();

            return reader;
        }


    }
}