using System;
using System.Threading.Tasks;
using MyNoSqlServer.Abstractions;
using MyNoSqlServer.DataWriter;

namespace TestApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var writer = new MyNoSqlServerDataWriter<TestEntity>(() => "http://192.168.70.80:5123", TestEntity.TableName, true);

            Console.WriteLine("print 0");
            await Print(writer);
            Console.WriteLine("---------------------");
           
            await writer.InsertOrReplaceAsync(TestEntity.Create("111", "111"));
            await writer.InsertOrReplaceAsync(TestEntity.Create("111", "222"));
            await writer.InsertOrReplaceAsync(TestEntity.Create("111", "333"));
            await writer.InsertOrReplaceAsync(TestEntity.Create("111", "444"));
            await writer.InsertOrReplaceAsync(TestEntity.Create("111", "555"));
            await writer.DeleteAsync("111", "444");
            await writer.InsertOrReplaceAsync(TestEntity.Create("222", "111"));
            await writer.InsertOrReplaceAsync(TestEntity.Create("222", "222"));
            await writer.InsertOrReplaceAsync(TestEntity.Create("333", "333"));
            
            Console.WriteLine("print 1");
            await Print(writer);
            Console.WriteLine("---------------------");
            
            await writer.DeleteAsync("111", "333");
            Console.WriteLine("print 2");
            await Print(writer);
            Console.WriteLine("---------------------");

            await writer.CleanAndKeepLastRecordsAsync("111", 1);
            Console.WriteLine("print 3");
            await Print(writer);
            Console.WriteLine("---------------------");

            await writer.CleanAndKeepMaxRecords("111", 1);
            Console.WriteLine("print 4");
            await Print(writer);
            Console.WriteLine("---------------------");

            await writer.DeleteAsync("111", "111");
            Console.WriteLine("print 5");
            await Print(writer);
            Console.WriteLine("---------------------");

            await writer.DeleteAsync("111", "555");
            Console.WriteLine("print 6");
            await Print(writer);
        }

        private static async Task Print(MyNoSqlServerDataWriter<TestEntity> writer)
        {
            var data = await writer.GetAsync();
            foreach (var entity in data)
            {
                Console.WriteLine(entity.Message);
            }
        }
    }

    public class TestEntity : MyNoSqlDbEntity
    {
        public const string TableName = "jw-test-15";

        public static string GeneratePartitionKey(string pk) => pk;
        public static string GenerateRowKey(string rk) => rk;
        
        public string Message { get; set; }


        public static TestEntity Create(string pk, string rk)
        {
            return new TestEntity()
            {
                PartitionKey = GeneratePartitionKey(pk),
                RowKey = rk,
                Message = $"message {pk}/{rk}. Time: {DateTime.Now:O}"
            };
        }

    }
}