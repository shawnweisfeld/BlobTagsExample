using Microsoft.Extensions.Configuration;
using PeanutButter.RandomGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<Program>()
                .Build()
                .Get<AppSettings>();

            var sampleInfo = ObjectInfoTableEntity.GenerateSample(config.Count, config.Revisions).ToList();
            
            await TableStorageExample.Execute(config.StorageAccount, config.Name + RandomValueGen.GetRandomAlphaString(8), sampleInfo);

            await BlobStorageExample.Execute(config.StorageAccount, config.Name + RandomValueGen.GetRandomAlphaString(8), sampleInfo);

            Console.WriteLine("Done!");
        }
    }

}
