using System;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.Service.LivnesProbs;
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
                .AsSelf()
                .As<ILivenessReporter>()
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
                .AutoActivate()
                .SingleInstance();

            return writer;
        }

        public static IMyNoSqlServerDataReader<T> RegisterMyNoSqlReader<T>(this ContainerBuilder builder,
            MyNoSqlTcpClient client, string tableName) where T : IMyNoSqlDbEntity, new()
        {
            var reader = builder.RegisterMyNoSqlReader<T>(client, tableName, NoSqlDataWaitMode.WaitAndContinue);
            return reader;
        }
        public static IMyNoSqlServerDataReader<T> RegisterMyNoSqlReader<T>(this ContainerBuilder builder, MyNoSqlTcpClient client, string tableName, 
            NoSqlDataWaitMode waitDataOnStart) where T : IMyNoSqlDbEntity, new()
        {
            var reader = new MyNoSqlReadRepository<T>(client, tableName);

            builder
                .RegisterInstance(reader)
                .As<IMyNoSqlServerDataReader<T>>()
                .AutoActivate()
                .SingleInstance();

            if (waitDataOnStart != NoSqlDataWaitMode.None)
            {
                Func<int> calc = () => reader.Get().Count;
                var waiter = new NoSqlReaderCountDataGetter(calc, typeof(T).Name, waitDataOnStart);

                builder.RegisterInstance(waiter).SingleInstance().As<INoSqlReaderCountDataGetter>();
            }

            return reader;
        }


    }
}