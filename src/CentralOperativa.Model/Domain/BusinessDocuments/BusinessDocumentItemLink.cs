using System;
using CentralOperativa.Domain.Catalog;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.BusinessDocuments
{
    [Alias("BusinessDocumentItemLinks")]
    public class BusinessDocumentItemLink
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(BusinessDocumentItem))]
        public int DocumentItemId { get; set; }

        [References(typeof(BusinessDocumentItem))]
        public int DocumentItemRelatedId { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public double? Amount { get; set; }
        public double? AppliedAmount { get; set; }


    }
}