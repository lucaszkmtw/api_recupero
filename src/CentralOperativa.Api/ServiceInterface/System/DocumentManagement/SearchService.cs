using ServiceStack;
using ServiceStack.OrmLite;
using System.Collections.Generic;

namespace CentralOperativa.ServiceInterface.System.DocumentManagement
{
    using Api = ServiceModel.System.DocumentManagement;

    [Authenticate]
    public class SearchService : ApplicationService
    {
        public List<Api.SearchDocumentsResult> Get(Api.SearchDocuments request)
        {
            return Db.SqlList<Api.SearchDocumentsResult>("EXEC sy_dm_search @userId, @tenantId, @q", new { userId = Session.UserId, tenantId = Session.TenantId, q = request.Q });
        }
    }
}
