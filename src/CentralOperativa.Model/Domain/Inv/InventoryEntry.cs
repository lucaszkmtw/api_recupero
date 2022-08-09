using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Inv
{
    [Alias("InventoryEntries")]
    public class InventoryEntry
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Domain.BusinessDocuments.BusinessDocumentItem))]
        public int BusinessDocumentItemId { get; set; }

        [References(typeof(Domain.Catalog.Product))]
        public int ProductId { get; set; }

        [References(typeof(Domain.Inv.InventorySite))]
        public int InventorySiteId { get; set; }

        public decimal Quantity { get; set; }

        public DateTime CreateDate { get; set; }

    }
}
