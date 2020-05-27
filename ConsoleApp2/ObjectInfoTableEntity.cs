using Microsoft.Azure.Cosmos.Table;
using PeanutButter.RandomGenerators;
using PeanutButter.Utils;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ConsoleApp2
{
    public class ObjectInfoTableEntity : TableEntity
    {
        public ObjectInfoTableEntity()
        {

        }

        public string CustomerId { get; set; }
        public string O365Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Removed { get; set; }

        public override string ToString()
        {
            return $"Customer ID: {CustomerId}, O365 ID: {O365Id}, Created: {Created:s}, Removed: {Removed:s}, PartitionKey: {PartitionKey}, RowKey: {RowKey}";
        }

        internal static IEnumerable<ObjectInfoTableEntity> GenerateSample(int count, int revisions)
        {
            for (int i = 0; i < count; i++)
            {
                ObjectInfoTableEntity last = null;

                for (int revision = 0; revision < RandomValueGen.GetRandomInt(1, revisions); revision++)
                {
                    if (revision == 0)
                    {
                        var obj = new ObjectInfoTableEntity();

                        obj.CustomerId = Guid.NewGuid().ToString();
                        obj.O365Id = Guid.NewGuid().ToString();
                        obj.Created = RandomValueGen.GetRandomUtcDate(DateTime.UtcNow.AddYears(-5), DateTime.UtcNow);
                        obj.Removed = RandomValueGen.GetRandomUtcDate(obj.Created, DateTime.UtcNow);
                        obj.PartitionKey = $"{obj.CustomerId}|{obj.O365Id}".MD5Hash();
                        obj.RowKey = obj.Created.ToString("s");

                        last = obj;

                        yield return obj;
                    }
                    else
                    {
                        var obj = new ObjectInfoTableEntity();

                        obj.CustomerId = last.CustomerId;
                        obj.O365Id = last.O365Id;
                        obj.Created = RandomValueGen.GetRandomUtcDate(last.Created, DateTime.UtcNow);
                        obj.Removed = RandomValueGen.GetRandomUtcDate(obj.Created, DateTime.UtcNow);
                        obj.PartitionKey = $"{obj.CustomerId}|{obj.O365Id}".MD5Hash();
                        obj.RowKey = obj.Created.ToString("s");

                        last = obj;

                        yield return obj;

                    }
                }
            }
        }

    }
}
