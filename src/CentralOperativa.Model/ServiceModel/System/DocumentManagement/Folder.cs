using System;
using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.System.DocumentManagement
{
    [Route("/system/documentmanagement/folders/{Guid}", "GET")]
    public class GetFolder
    {
        public Guid Guid { get; set; }
        public int? Limit { get; set; }
    }

    public class GetFolderResult : Domain.System.DocumentManagement.Folder
    {
        public GetFolderResult()
        {
            Children = new List<Domain.System.DocumentManagement.Folder>();
            Files = new List<Domain.System.DocumentManagement.File>();
            Ancestors = new List<FolderHierarchyItem>();
        }

        public List<FolderHierarchyItem> Ancestors { get; set; }
        public List<Domain.System.DocumentManagement.Folder> Children { get; set; }
        public List<Domain.System.DocumentManagement.File> Files { get; set; }
    }

    [Route("/system/documentmanagement/folders", "GET")]
    public class QueryFolders : QueryDb<Domain.System.DocumentManagement.Folder, QueryFoldersResult>
    {
    }

    [Route("/system/documentmanagement/folders/lookup", "GET")]
    public class LookupFolder : LookupRequest, IReturn<List<LookupItem>>
    {
    }

    public class QueryFoldersResult
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
    }

    [Route("/system/documentmanagement/folders/{ParentFolderGuid}", "POST")]
    public class PostFolder
    {
        public Guid ParentFolderGuid { get; set; }

        public string Name { get; set; }
    }

    [Route("/system/documentmanagement/folders/{FolderGuid}/files", "POST")]
    public class PostFiles
    {
        public Guid FolderGuid { get; set; }

        public string Uri { get; set; }

        public string Name { get; set; }
    }
}
