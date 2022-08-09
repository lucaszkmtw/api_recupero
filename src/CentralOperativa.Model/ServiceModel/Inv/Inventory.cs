using System;
using ServiceStack;
using System.Collections.Generic;

namespace CentralOperativa.ServiceModel.Inv
{
    public class Inventory
    {
        [Route("/inv/inventory", "GET")]
        public class QueryInventory
        {
            public List<InventoryResult> Results { get; set;  }
        }
        
        public class InventoryResult
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Q { get; set; }
            public int LastMonthReceipts { get; set; }
            public int LastMonthDeliveries { get; set; }
            public int ThisMonthReceipts { get; set; }
            public int ThisMonthDeliveries { get; set; }
        }

        [Route("/inv/inventoryproduct/{ProductId}", "GET")]
        public class QueryInventoryProduct : QueryDb<Domain.Inv.InventorySite, InventoryProductResult>
            , IJoin<Domain.Inv.InventorySite, Domain.Inv.InventoryEntry>
        {
            public int ProductId { get; set; }
        }

        public class InventoryProductResult
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Quantity { get; set; }
        }

        [Route("/inv/inventoryproductsite/{ProductId}/{SiteId]", "GET")]
        public class QueryInventoryProductSite : QueryDb<Domain.Inv.InventoryEntry, InventoryProductSiteResult>
            , IJoin<Domain.Inv.InventoryEntry, Domain.BusinessDocuments.BusinessDocumentItem>
            , IJoin<Domain.Inv.InventoryEntry, Domain.Inv.InventorySite>
            , IJoin<Domain.BusinessDocuments.BusinessDocumentItem, Domain.BusinessDocuments.BusinessDocument>
            , IJoin<Domain.BusinessDocuments.BusinessDocument, Domain.BusinessDocuments.BusinessDocumentType>
        {
            public int ProductId { get; set; }
            public int SiteId { get; set; }
        }

        public class InventoryProductSiteResult
        {
            public string ShortName { get; set; }
            public string Number { get; set; }
            public int Quantity { get; set; }
            public DateTime DocumentDate { get; set; }
            public int Balance { get; set; }
            public string ReceiverName { get; set; }
            public string IssuerName { get; set; }
        }

    }
}
