using System;
using CentralOperativa.Domain.Catalog;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.BusinessDocuments
{
    [Alias("BusinessDocumentItemLaws")]
    public class BusinessDocumentItemLaw
    {
        [AutoIncrement]
        public int Id { get; set; }

        //estba como BusinessDocuemntItems?
        [References(typeof(BusinessDocumentItem))]
        public int BusinessDocumentItemId { get; set; }

        [References(typeof(Domain.Financials.DebtManagement.Law))]
        public int LawId { get; set; }

        public string Observation { get; set; }

    }
}