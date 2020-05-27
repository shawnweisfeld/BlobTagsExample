using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Azure;
using PeanutButter.RandomGenerators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    public static class BlobStorageExample
    {
        public async static Task Execute(string connectionString, string containerName, List<ObjectInfoTableEntity> objInfos)
        {
            //https://azure.microsoft.com/en-us/blog/manage-and-find-data-with-blob-index-for-azure-storage-now-in-preview/
            //https://docs.microsoft.com/en-us/azure/storage/blobs/storage-manage-find-blobs
            //https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blob-index-how-to

            containerName = containerName.ToLower();

            var serviceClient = new BlobServiceClient(connectionString);
            var containerClient = serviceClient.GetBlobContainerClient(containerName);


            //create the container if needed
            Console.WriteLine($"Create Container - {containerName}");
            await containerClient.CreateIfNotExistsAsync();

            //upload all the blobs
            Console.WriteLine($"\n\nAdding:");
            foreach (var objinfo in objInfos)
            {
                Console.WriteLine($"     {objinfo}");
                string file = $"{objinfo.PartitionKey}/{objinfo.RowKey}.txt";
                var blobClient = containerClient.GetBlobClient(file);

                await blobClient.DeleteIfExistsAsync();

                await blobClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(RandomValueGen.GetRandomWords(50))));

                await blobClient.SetTagsAsync(new Dictionary<string, string>
                {
                    {"CustomerId", objinfo.CustomerId},
                    {"O365Id", objinfo.O365Id},
                    {"Created", objinfo.Created.ToString("s")},
                    {"Removed", objinfo.Removed.ToString("s")}
                });
            }


            //pick a random object to play with
            var tofind = objInfos[RandomValueGen.GetRandomInt(0, objInfos.Count - 1)];
            Console.WriteLine($"\n\nSearching for\n     {tofind}");

            var found = (await containerClient.GetBlobClient($"{tofind.PartitionKey}/{tofind.RowKey}.txt").GetTagsAsync()).Value;
            Console.WriteLine($"\n\nFound by Partition/Row key\n     {found.ToJoinedString()}");

            Console.WriteLine($"\n\nFind all revisions of a given object by hierarchy");
            await foreach (var item in containerClient.GetBlobsByHierarchyAsync(delimiter: "/", prefix: tofind.PartitionKey + "/", traits: BlobTraits.Tags))
            {
                if (item.IsBlob)
                {
                    Console.WriteLine($"     {item.Blob.Tags.ToJoinedString()} - {item.Blob.Name}");
                }
            }


            Console.WriteLine($"\n\nFind all revisions of a given object by tags");
            string query = $"@container = '{containerName}' "
                + $"AND \"CustomerId\" = '{tofind.CustomerId}' "
                + $"AND \"O365Id\" = '{tofind.O365Id}' ";

            await foreach (var item in serviceClient.FindBlobsByTagsAsync(query))
            {
                var blobClient = containerClient.GetBlobClient(item.Name);
                var tags = (await blobClient.GetTagsAsync()).Value;
                Console.WriteLine($"     {tags.ToJoinedString()} - {item.Name}");
            }


        }
    }
}
