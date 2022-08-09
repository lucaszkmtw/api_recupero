using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Extensions.Configuration;
using ServiceStack;
using ServiceStack.OrmLite;
using CentralOperativa.Domain.System.DocumentManagement;
using CentralOperativa.Infraestructure;

namespace CentralOperativa.ServiceInterface.System.DocumentManagement
{
    using Api = ServiceModel.System.DocumentManagement;

    public class FolderRepository
    {
        private readonly TenantRepository _tenantRepository;

        public FolderRepository(TenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }

        public async Task<Api.GetFolderResult> GetFolder(IDbConnection db, int id, Session session, bool loadReferences = true)
        {
            var model = (await db.SingleByIdAsync<Folder>(id)).ConvertTo<Api.GetFolderResult>();
            if (loadReferences)
            {
                await LoadModel(db, model, session);
            }
            return model;
        }

        public async Task<Api.GetFolderResult> GetFolder(IDbConnection db, Guid guid, Session session, bool loadReferences = true, int? limit = null)
        {
            var model = (await db.SingleAsync<Folder>(w => w.Guid == guid)).ConvertTo<Api.GetFolderResult>();
            if (loadReferences)
            {
                await LoadModel(db, model, session, limit);
            }
            return model;
        }

        private async Task LoadModel(IDbConnection db, Api.GetFolderResult model, Session session, int? limit = null)
        {
            //Ancestors
            model.Ancestors = await db.SqlListAsync<Api.FolderHierarchyItem>("SELECT f.*, h.* FROM Folders f CROSS APPLY dbo.GetFolderHierarchyUp(f.Id) h WHERE f.Id = @folderId", new { folderId = model.Id });

            //Children
            var childrenQuery = db.From<Folder>()
                        .Join<Folder, FolderFolder>((c, ff) => c.Id == ff.ChildId)
                        .Where<FolderFolder>(w => w.ParentId == model.Id);
            if (limit.HasValue)
            {
                childrenQuery.Limit(limit.Value);
            }
            model.Children = await db.SelectAsync(childrenQuery);
            var tenant = await _tenantRepository.GetTenant(db, session.TenantId);
                    
            //Virtual Folders
            if (model.Guid == session.ProfileFolderGuid)
            {
                var tenantFolder = await GetFolder(db, tenant.FolderGuid, session, false);
                model.Children.Add(tenantFolder);
            }

            //Files
            model.Files = await db.SelectAsync(db.From<File>().Join<File, FolderFile>((c, ff) => c.Id == ff.FileId).Where<FolderFile>(w => w.FolderId == model.Id));
        }

        public async Task<Api.GetFolderResult> CreateFolder(IDbConnection db, Session session, Guid? parentFolderGuid, string name)
        {
            var newFolder = new Folder
            {
                CreateDate = DateTime.UtcNow,
                Guid = Guid.NewGuid(),
                Name = name
            };
            newFolder.Id = (int) await db.InsertAsync(newFolder, true);

            if (parentFolderGuid.HasValue)
            {
                var folder = await GetFolder(db, parentFolderGuid.Value, session, false);
                db.Insert(new FolderFolder
                {
                    ParentId = folder.Id,
                    ChildId = newFolder.Id,
                    CreateDate = DateTime.UtcNow
                });
            }
            return newFolder.ConvertTo<Api.GetFolderResult>();
        }

        public async Task<List<File>> CreateFiles(IDbConnection db, Guid folderGuid, ServiceStack.Web.IHttpFile[] files, Session session)
        {
            var folder = await GetFolder(db, folderGuid, session, false);
            return await CreateFiles(db, folder.Id, files);
        }

        public async Task<List<File>> CreateFiles(IDbConnection db, int folderId, ServiceStack.Web.IHttpFile[] httpFiles)
        {
            var files = new List<File>();
            var configuration = HostContext.Resolve<IConfiguration>();
            //Save the file to ABS
            // Retrieve storage account from connection string.
            var storageAccount = CloudStorageAccount.Parse(configuration.GetConnectionString("Storage"));

            // Create the blob client.
            var blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            var container = blobClient.GetContainerReference("centraloperativa-files");
            foreach (var httpFile in httpFiles)
            {
                var extension = global::System.IO.Path.GetExtension(httpFile.FileName);
                var guid = Guid.NewGuid();

                var blockBlob = container.GetBlockBlobReference(guid + extension);
                using (var fileStream = httpFile.InputStream)
                {
                    await blockBlob.UploadFromStreamAsync(fileStream);
                }

                var file = new File
                {
                    Guid = guid,
                    Name = httpFile.FileName,
                    Url = blockBlob.Uri.LocalPath,
                    CreateDate = DateTime.UtcNow,
                };
                file.Id = (int) await db.InsertAsync(file, true);

                var folderFile = new FolderFile
                {
                    FolderId = folderId,
                    FileId = file.Id,
                    CreateDate = DateTime.UtcNow
                };
                await db.InsertAsync(folderFile);

                files.Add(file);
            }

            return files;
        }
    }
}