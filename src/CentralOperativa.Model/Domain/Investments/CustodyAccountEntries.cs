using System;

using ServiceStack.DataAnnotations;

using CentralOperativa.Domain.BusinessPartners;

namespace CentralOperativa.Domain.Investments
{
    [Alias("CustodyAccountEntries"), Schema("investments")]
    public class CustodyAccountEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [References(typeof(BusinessPartnerAccount))]
        public int CustodyAccountId { get; set; }

        [References(typeof(Asset))]
        public int AssetId { get; set; }

        public decimal Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal Amount => Price * Quantity;

        public DateTime CreateDate { get; set; }

        public DateTime PostingDate { get; set; }

        public short? LinkedDocumentTypeId { get; set; }

        public int? LinkedDocumentId { get; set; }
    }
}
