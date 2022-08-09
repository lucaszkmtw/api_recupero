using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

namespace CentralOperativa.Infraestructure
{
    public class AzureService
    {
        private readonly IConfiguration _configuration;

        public AzureService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CloudStorageConnectionString => _configuration.GetConnectionString("CentralOperativaStorage");

        public async Task<CloudBlobContainer> FindOrCreateBlobContainer(string containerName)
        {
            Trace.TraceInformation("FindOrCreatePrivateBlobContainer '" + containerName + "' with connectionstring '" + CloudStorageConnectionString + "'");
            var account = CloudStorageAccount.Parse(CloudStorageConnectionString);
            var blobClient = account.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            await container.CreateIfNotExistsAsync();
            return container;
        }

        public async Task<CloudQueue> FindOrCreateQueue(string queueName)
        {
            Trace.TraceInformation("FindOrCreateQueue '" + queueName + "' with connectionstring '" + CloudStorageConnectionString + "'");
            var account = CloudStorageAccount.Parse(CloudStorageConnectionString);
            var queueClient = account.CreateCloudQueueClient();
            var container = queueClient.GetQueueReference(queueName);
            await container.CreateIfNotExistsAsync();
            return container;
        }

        public async Task<CloudTable> FindOrCreateTable(string tableName)
        {
            Trace.TraceInformation("FindOrCreateTable '" + tableName + "' with connectionstring '" + CloudStorageConnectionString + "'");
            var account = CloudStorageAccount.Parse(CloudStorageConnectionString);
            var tableClient = account.CreateCloudTableClient();
            var container = tableClient.GetTableReference(tableName);
            await container.CreateIfNotExistsAsync();
            return container;
        }
    }
}