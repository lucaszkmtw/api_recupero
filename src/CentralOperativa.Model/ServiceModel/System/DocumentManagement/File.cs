using System;
using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.System.DocumentManagement
{
    [Route("/system/documentmanagement/files/{Guid}", "GET")]
    public class GetFile
    {
        public Guid Guid { get; set; }
    }

    [Route("/system/documentmanagement/files", "GET")]
    public class QueryFiles : QueryDb<Domain.System.DocumentManagement.File, QueryFilesResult>
    {
    }

    [Route("/system/documentmanagement/files/lookup", "GET")]
    public class LookupFile : LookupRequest, IReturn<List<LookupItem>>
    {
    }

    public class QueryFilesResult
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public int FolderId { get; set; }
        public string FolderName { get; set; }
        public DateTime Date { get; set; }
    }

    [Route("/system/documentmanagement/files", "POST")]
    public class PostFile : Domain.System.DocumentManagement.File
    {
    }
}
