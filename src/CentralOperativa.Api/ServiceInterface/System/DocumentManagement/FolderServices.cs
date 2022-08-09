using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CentralOperativa.Domain.System.DocumentManagement;
using ServiceStack;
using ServiceStack.OrmLite;

namespace CentralOperativa.ServiceInterface.System.DocumentManagement
{
    using Api = ServiceModel.System.DocumentManagement;

    [Authenticate]
    public class FolderServices : ApplicationService
    {
        private readonly IAutoQueryDb _autoQuery;
        private readonly FolderRepository _folderRepository;
        private readonly FileRepository _fileRepository;

        public FolderServices(IAutoQueryDb autoQuery, FolderRepository folderRepository, FileRepository fileRepository)
        {
            _autoQuery = autoQuery;
            _folderRepository = folderRepository;
            _fileRepository = fileRepository;
        }

        public async Task<Api.GetFolderResult> Get(Api.GetFolder request)
        {
            return await _folderRepository.GetFolder(Db, request.Guid, Session, true, request.Limit);
        }

        public QueryResponse<Api.QueryFoldersResult> Get(Api.QueryFolders request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var q = _autoQuery.CreateQuery(request, Request.GetRequestParams());
            return _autoQuery.Execute(request, q);
        }

        public async Task<Api.GetFolderResult> Post(Api.PostFolder request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var folder = await _folderRepository.CreateFolder(Db, Session, request.ParentFolderGuid, request.Name);
                    trx.Commit();
                    return folder;
                }
                catch (Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }
        }

        public Folder Put(Api.PostFiles request)
        {
            throw new NotImplementedException();
        }

        public async Task<List<File>> Post(Api.PostFiles request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var files = new List<File>();
                    if (!string.IsNullOrEmpty(request.Uri))
                    {
                        var folder = await _folderRepository.GetFolder(Db, request.FolderGuid, Session, false);
                        var file = await _fileRepository.CreateFile(Db, request.Uri, request.Name);
                        var folderFile = new FolderFile
                        {
                            CreateDate = DateTime.UtcNow,
                            FileId = file.Id,
                            FolderId = folder.Id
                        };
                        await Db.InsertAsync(folderFile);

                        files.Add(file);
                    }
                    else
                    {
                        await _folderRepository.CreateFiles(Db, request.FolderGuid, Request.Files, Session);
                    }
                    trx.Commit();

                    return files;
                }
                catch (Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }
        }
    }
}
