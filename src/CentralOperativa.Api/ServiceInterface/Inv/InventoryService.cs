using ServiceStack;
using ServiceStack.Logging;
using ServiceStack.OrmLite;
using Inventory = CentralOperativa.ServiceModel.Inv.Inventory;
namespace CentralOperativa.ServiceInterface.Inv
{
    [Authenticate]
    public class InventoryService : ApplicationService
    {
        public static ILog Log = LogManager.GetLogger(typeof(InventorySiteService));

        public IAutoQueryDb AutoQuery { get; set; }
        public object Get(Inventory.QueryInventory request)
        {
            string q = null;

            var nameContains = Request.QueryString["nameContains"];
            if (!string.IsNullOrEmpty(nameContains))
            {
                q = nameContains;
            }

            var data = Db.SqlList<Inventory.InventoryResult>("EXEC RPT_Inventory @tenantId, @filter", new { tenantId = Session.TenantId, filter = q });
            return data;
        }

        public object Get(Inventory.QueryInventoryProduct request)
        {
            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            q.Join<Domain.Inv.InventorySite, Domain.System.Tenant>((iss, t) => iss.PersonId == t.PersonId);
            q.And<Domain.System.Tenant>(w => w.Id == Session.TenantId);
            q.And<Domain.Inv.InventoryEntry>(w => w.ProductId == request.ProductId);
            q.GroupBy<Domain.Inv.InventorySite>(x => new { x.Id, x.Name });
            q.Select<Domain.Inv.InventorySite, Domain.Inv.InventoryEntry>((iss, ie) => new
            {
                Id = iss.Id,
                Name = iss.Name,
                Quantity = Sql.Sum(ie.Quantity)
            });
            q.OrderByDescending(2);
            return AutoQuery.Execute(request, q);
        }

        public object Get(Inventory.QueryInventoryProductSite request)
        {
            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            q.Join<Domain.Inv.InventorySite, Domain.System.Tenant>((iss, t) => iss.PersonId == t.PersonId);
            q.Join<Domain.BusinessDocuments.BusinessDocument, Domain.System.Persons.Person>((bd, ip) => bd.IssuerId == ip.Id, Db.JoinAlias("Issuer"));
            q.Join<Domain.BusinessDocuments.BusinessDocument, Domain.System.Persons.Person>((bd, rp) => bd.ReceiverId == rp.Id, Db.JoinAlias("Receiver"));
            q.And<Domain.System.Tenant>(w => w.Id == Session.TenantId);
            q.Select<
                Domain.Inv.InventoryEntry,
                Domain.BusinessDocuments.BusinessDocumentItem,
                Domain.Inv.InventorySite,
                Domain.BusinessDocuments.BusinessDocument,
                Domain.BusinessDocuments.BusinessDocumentType,
                Domain.System.Persons.Person,
                Domain.System.Persons.Person>(
                (ie, bdi, iss, bd, bdt, ip, rp) => new
            {
                ShortName = bdt.ShortName,
                Number = bd.Number,
                Quantity = ie.Quantity,
                DocumentDate = bd.DocumentDate,
                ReceiverName = Sql.JoinAlias(rp.Name, "Receiver"),
                IssuerName = Sql.JoinAlias(ip.Name, "Issuer")
                });
            q.OrderByDescending<Domain.BusinessDocuments.BusinessDocument>(o => o.DocumentDate);
            return AutoQuery.Execute(request, q);
            
        }
    }
}