using System;
using ServiceStack;

namespace CentralOperativa.ServiceModel.System.DocumentManagement
{
    [Route("/system/documentmanagement/search", "GET")]
    public class SearchDocuments
    {
        public string Q { get; set; }
    }

    public class SearchDocumentsResult
    {
        public int Id { get; set; }
        public byte Type { get; set; }
        public string Name { get; set; }
        public Guid Guid { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
