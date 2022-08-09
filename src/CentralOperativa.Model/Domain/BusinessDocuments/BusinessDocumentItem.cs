using System;
using CentralOperativa.Domain.Catalog;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.BusinessDocuments
{
    [Alias("BusinessDocumentItems")]
    public class BusinessDocumentItem
    {
        [AutoIncrement]
        public int Id { get; set; }

        //estba como BusinessDocuemntItems?
        [References(typeof(BusinessDocument))]
        public int BusinessDocumentId { get; set; }

        [References(typeof(Product))]
        public int? ProductId { get; set; }

        public string Code { get; set; }

        public string Concept { get; set; }

        public int UnitTypeId { get; set; }

        public decimal Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Bonus { get; set; }

        public decimal VatRate { get; set; }

        public int? InventorySiteId { get; set; }

        public string FieldsJSON { get; set; }

        public DateTime? ItemDate { get; set; }

        public DateTime? VoidDate { get; set; }

        public DateTime? NotificationDate { get; set; }

        public DateTime? PrescriptionDate { get; set; }

        public decimal? AppliedAmount { get; set; }
        public decimal? AppliedInterest { get; set; }
        public decimal? PendingInterest { get; set; }
        public decimal? OriginalAmount { get; set; }
        public DateTime? OriginalVoidDate { get; set; }
    }
}