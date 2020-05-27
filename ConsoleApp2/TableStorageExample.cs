using Microsoft.Azure.Cosmos.Table;
using Microsoft.OData.UriParser;
using PeanutButter.RandomGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    public class TableStorageExample
    {
        public static async Task Execute(string connectionString, string tableName, List<ObjectInfoTableEntity> objInfos)
        {
            // Design for querying
            // https://docs.microsoft.com/en-us/azure/storage/tables/table-storage-design-for-query


            var acct = CloudStorageAccount.Parse(connectionString);
            var tableClient = acct.CreateCloudTableClient();
            var table = tableClient.GetTableReference(tableName);

            //create the table if needed
            Console.WriteLine($"Create Table - {tableName}");
            await table.CreateIfNotExistsAsync();

            Console.WriteLine(Environment.NewLine);

            //insert all the object infos into the table
            Console.WriteLine($"\n\nAdding:");
            foreach (var objinfo in objInfos)
            {
                Console.WriteLine($"     {objinfo}");
                await table.ExecuteAsync(TableOperation.InsertOrReplace(objinfo));
            }

            //pick a random object to play with
            var tofind = objInfos[RandomValueGen.GetRandomInt(0, objInfos.Count - 1)];
            Console.WriteLine($"\n\nSearching for\n     {tofind}");

            var found = await table.ExecuteAsync(TableOperation.Retrieve<ObjectInfoTableEntity>(tofind.PartitionKey, tofind.RowKey));
            Console.WriteLine($"\n\nFound by Partition/Row key\n     {found.Result as ObjectInfoTableEntity}");

            Console.WriteLine($"\n\nFind all revisions of a given object");
            var finds = table.ExecuteQuery(new TableQuery<ObjectInfoTableEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, tofind.PartitionKey)));
            
            foreach (var item in finds)
            {
                Console.WriteLine($"     {item}");
            }

            Console.WriteLine($"\n\nFind all newer revisions of a given object");
            finds = table.ExecuteQuery(new TableQuery<ObjectInfoTableEntity>()
                .Where(TableQuery.CombineFilters(
                   TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, tofind.PartitionKey),
                   TableOperators.And,
                   TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, tofind.RowKey))));

            foreach (var item in finds)
            {
                Console.WriteLine($"     {item}");
            }

            Console.WriteLine($"\n\nFind all future revisions of a given object removed within the last year");
            var newDate = DateTime.UtcNow.AddYears(-1);

            finds = table.ExecuteQuery(new TableQuery<ObjectInfoTableEntity>()
                .Where(TableQuery.CombineFilters(
                    TableQuery.CombineFilters(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, tofind.PartitionKey), TableOperators.And, 
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, tofind.RowKey)),
                   TableOperators.And, TableQuery.GenerateFilterConditionForDate("Removed", QueryComparisons.GreaterThanOrEqual, newDate))));

            foreach (var item in finds)
            {
                Console.WriteLine($"     {item}");
            }


            Console.WriteLine($"\n\nFind all future revisions of a given object removed within the last year with linq");
            finds = from objinfo in table.CreateQuery<ObjectInfoTableEntity>()
                    where objinfo.PartitionKey == tofind.PartitionKey && objinfo.Created >= tofind.Created && objinfo.Removed >= newDate
                    select objinfo;

            foreach (var item in finds)
            {
                Console.WriteLine($"     {item}");
            }


            Console.WriteLine($"\n\nFind all future revisions of a given object removed within the last year in client");
            finds = table.ExecuteQuery(new TableQuery<ObjectInfoTableEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, tofind.PartitionKey)))
                .Where(x => x.Created >= tofind.Created && x.Removed >= newDate);

            foreach (var item in finds)
            {
                Console.WriteLine($"     {item}");
            }

        }
    }
}
