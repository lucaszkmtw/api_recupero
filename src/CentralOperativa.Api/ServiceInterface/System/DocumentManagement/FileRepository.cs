using System;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using System.Linq;
using CentralOperativa.Infraestructure;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using ServiceStack;
using ServiceStack.OrmLite;
using File = CentralOperativa.Domain.System.DocumentManagement.File;

namespace CentralOperativa.ServiceInterface.System.DocumentManagement
{
    public class FileRepository
    {
        public async Task<object> GetFile(IDbConnection db, Guid guid)
        {
            var configuration = HostContext.Container.Resolve<IConfiguration>();

            var file = db.Select(db.From<Domain.System.DocumentManagement.File>().Join<Domain.System.DocumentManagement.FileSystemProvider>().Where(w => w.Guid == guid)).SingleOrDefault();
            if (file != null)
            {
                var memStream = new MemoryStream();
                string contentType = null;

                if (file.Url.StartsWith("http"))
                {
                    var storageAccount = CloudStorageAccount.Parse(configuration.GetConnectionString("OsmmedtStorage"));
                    var blobClient = storageAccount.CreateCloudBlobClient();
                    var container = blobClient.GetContainerReference("afiliados");
                    var blobName = file.Url.Substring(container.Uri.ToString().Length+1);
                    var blob = await container.GetBlobReferenceFromServerAsync(blobName);
                    contentType = blob.Properties.ContentType;
                    await blob.DownloadToStreamAsync(memStream);
                }
                else
                {
                    var storageAccount = CloudStorageAccount.Parse(configuration.GetConnectionString("Storage"));
                    var blobClient = storageAccount.CreateCloudBlobClient();
                    var container = blobClient.GetContainerReference("centraloperativa-files");
                    var blob = await container.GetBlobReferenceFromServerAsync(file.Url.Replace("/centraloperativa-files/", string.Empty));
                    contentType = blob.Properties.ContentType;
                    await blob.DownloadToStreamAsync(memStream);
                }
                var fileName = file.Name.ToLowerInvariant();
                if (contentType.Contains("octet"))
                {
                    if (fileName.EndsWith(".txt"))
                    {
                        contentType = "text/plain";
                    }

                    if (fileName.EndsWith(".htm"))
                    {
                        contentType = "text/html";
                    }

                    if (fileName.EndsWith(".html"))
                    {
                        contentType = "text/html";
                    }

                    if (fileName.EndsWith(".msg"))
                    {
                        contentType = "application/vnd.ms-outlook";
                    }

                    if (fileName.EndsWith(".eml"))
                    {
                        contentType = "message/rfc822";
                    }

                    if (fileName.EndsWith(".doc"))
                    {
                        contentType = "application/msword";
                    }

                    if (fileName.EndsWith(".docx"))
                    {
                        contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    }

                    if (fileName.EndsWith(".xls"))
                    {
                        contentType = "application/vnd.ms-excel";
                    }

                    if (fileName.EndsWith(".xslx"))
                    {
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    }

                    if (fileName.EndsWith(".pdf"))
                    {
                        contentType = "application/pdf";
                    }

                    if (fileName.EndsWith(".jpg"))
                    {
                        contentType = "image/jpg";
                    }

                    if (fileName.EndsWith(".gif"))
                    {
                        contentType = "image/gif";
                    }

                    if (fileName.EndsWith(".png"))
                    {
                        contentType = "image/png";
                    }
                }

                memStream.Position = 0;
                return new FileResult(memStream, contentType, fileName);
            }

            throw HttpError.NotFound("The requested file was not found");
        }

        public async Task<Domain.System.DocumentManagement.File> CreateFile(IDbConnection db, string imageData, string containerName, string fileName, string contentType)
        {
            var configuration = HostContext.Container.Resolve<IConfiguration>();

            var storageAccount = CloudStorageAccount.Parse(configuration.GetConnectionString("Storage"));
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            var guid = Guid.NewGuid();
            var filename = guid.ToString();
            var imageBytes = Convert.FromBase64String(imageData);
            var blob = container.GetBlockBlobReference(filename);
            blob.Properties.ContentType = "image/png";
            await blob.UploadFromByteArrayAsync(imageBytes, 0, imageBytes.Length);
            var localPath = blob.Uri.LocalPath;
            var file = new Domain.System.DocumentManagement.File
            {
                CreateDate = DateTime.UtcNow,
                Guid = guid,
                Name = fileName,
                Url = localPath
            };

            file.Id = (int) db.Insert(file, true);
            return file;
        }

        public async Task<Domain.System.DocumentManagement.File> CreateFile(IDbConnection db, string uri, string fileName)
        {
            var guid = Guid.NewGuid();
            var file = new Domain.System.DocumentManagement.File
            {
                CreateDate = DateTime.UtcNow,
                Guid = guid,
                Name = fileName,
                Url = uri
            };

            file.Id = (int) await db.InsertAsync(file, true);
            return file;
        }

        public async Task<File> CreateFile(IDbConnection db, byte[] textBytes, string containerName, string fileName, string contentType)
        {
            var configuration = HostContext.Container.Resolve<IConfiguration>();

            var storageAccount = CloudStorageAccount.Parse(configuration.GetConnectionString("Storage"));
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            var guid = Guid.NewGuid();
            var filename = guid.ToString();

            var blob = container.GetBlockBlobReference(filename);
            blob.Properties.ContentType = contentType;
            try
            {
                await blob.UploadFromByteArrayAsync(textBytes, 0, textBytes.Length); var localPath = blob.Uri.LocalPath;
                var file = new File
                {
                    CreateDate = DateTime.UtcNow,
                    Guid = guid,
                    Name = fileName,
                    Url = localPath
                };

                file.Id = (int)db.Insert(file, true);
                return file;
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}