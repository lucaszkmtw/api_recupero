using System;
using CentralOperativa.Domain.System.Persons;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Financials
{
    [Alias("PaymentDocumentMethods")]
    public class PaymentDocumentMethod
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(PaymentDocument))]
        public int IssuerPaymentDocumentId { get; set; }

        [References(typeof(PaymentDocument))]
        public int? ReceiverPaymentDocumentId { get; set; }

        [References(typeof(PaymentMethod))]
        public int PaymentMethodId { get; set; }

        [References(typeof(BankAccount))]
        public int? BankAccountId { get; set; }

        [References(typeof(CheckBook))]
        public int? CheckBookId { get; set; }

        [References(typeof(Person))]
        public int? IssuerId { get; set; }

        public DateTime? IssueDate { get; set; }

        public DateTime? VoidDate { get; set; }

        public DateTime? DepositDate { get; set; }

        public DateTime? CollectionDate { get; set; }

        public string Source { get; set; }

        public string CheckNumber { get; set; }

        public decimal Amount { get; set; }

        public string Comments { get; set; }

        public byte Status { get; set; }

        public byte FinancingStatus { get; set; }

        public string DepositNumber { get; set; }
    }
}