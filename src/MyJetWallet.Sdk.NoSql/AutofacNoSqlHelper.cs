using System;
using Autofac;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.Abstractions;
using MyNoSqlServer.DataReader;
using MyNoSqlServer.DataWriter;

namespace MyJetWallet.Sdk.NoSql
{
    public static class AutofacNoSqlHelper
    {
        public static MyNoSqlTcpClient CreateNoSqlClient(this ContainerBuilder builder, Func<string> readerUrl)
        {
            var myNoSqlClient = new MyNoSqlTcpClient(
                readerUrl,
                ApplicationEnvironment.HostName ??
                $"{ApplicationEnvironment.AppName}:{ApplicationEnvironment.AppVersion}");

            builder.RegisterInstance(myNoSqlClient).AsSelf().SingleInstance();
            builder.RegisterType<MyNoSqlTcpClientWatcher>().AutoActivate().SingleInstance();



            return myNoSqlClient;
        }

        public static ContainerBuilder RegisterMyNoSqlWriter<T>(this ContainerBuilder builder, Func<string> writerUrl, string tableName, bool persist = true, 
            DataSynchronizationPeriod dataSynchronizationPeriod = DataSynchronizationPeriod.Sec5) where T: IMyNoSqlDbEntity, new()
        {
            builder
                .RegisterInstance(new MyNoSqlServerDataWriter<T>(
                    writerUrl, tableName, persist, dataSynchronizationPeriod))
                .As<IMyNoSqlServerDataWriter<T>>()
                .SingleInstance();

            return builder;
        }


    }
}