using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Financials
{
    [Alias("PaymentDocuments")]
    public class PaymentDocument
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Domain.Financials.PaymentDocumentType))]
        public int TypeId { get; set; }

        [References(typeof(Domain.System.Persons.Person))]
        public int IssuerId { get; set; }

        [References(typeof(Domain.System.Persons.Person))]
        public int ReceiverId { get; set; }

        [References(typeof(Domain.System.DocumentManagement.Folder))]
        public int? FolderId { get; set; }

        [References(typeof(Domain.System.Messages.MessageThread))]
        public int? MessageThreadId { get; set; }

        public Guid Guid { get; set; }

        public string Number { get; set; }

        public DateTime DocumentDate { get; set; }

        public decimal Amount { get; set; }

        public string Comments { get; set; }

        [References(typeof(Domain.System.User))]
        public int CreatedBy { get; set; }

        public DateTime CreateDate { get; set; }

        public byte Status { get; set; }

        [References(typeof(Domain.BusinessPartners.BusinessPartnerAccount))]
        public int AccountId { get; set; }
    }
}