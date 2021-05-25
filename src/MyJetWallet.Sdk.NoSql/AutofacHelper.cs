using System;
using Autofac;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.DataReader;

namespace MyJetWallet.Sdk.NoSql
{
    public static class AutofacHelper
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


    }
}