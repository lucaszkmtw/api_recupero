using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Loans
{
    [Alias("LoanConcepts")]
    public class LoanConcept
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public LoanConceptOperation Operation { get; set; }

        public LoanConceptType Type { get; set; }

        public LoanConceptSource? Source { get; set; }

        public LoanConceptApplyTo ApplyTo { get; set; }

        //[References(typeof(LoanConcept))]
        public int? BasedOnId { get; set; }

        public bool PostDirectPaymentOrder { get; set; }

        /// <summary>
        /// 1 - Client Account, 2 - Vendor Account
        /// </summary>
        public byte OperatingAccountPostingType { get; set; }

        [Alias("DefaultLoanPersonRoleId")]
        public LoanPersonRole? DefaultLoanPersonRole { get; set; }

        public int? DefaultBusinessPartnerId { get; set; }

        /// <summary>
        /// False = Addition, True = Substraction
        /// </summary>
        public bool OperationSign { get; set; }

        //public byte InvoiceReceiverOperatingAccountPostingType { get; set; }
        //public string InvoiceItemConcept { get; set; }
    }

    [Flags, EnumAsInt]
    public enum LoanConceptOperation : byte
    {
        None,
        Addition,
        Subtraction,
        Multiplication,
        Division
    }

    [Flags, EnumAsInt]
    public enum LoanConceptType : byte
    {
        Fixed,
        Percentage
    }

    [Flags, EnumAsInt]
    public enum LoanConceptApplyTo : byte
    {
        Capital,
        Installment,
        Other
    }

    [Flags, EnumAsInt]
    public enum LoanConceptSource : byte
    {
        Capital,
        Installment,
        Concept
    }
}

