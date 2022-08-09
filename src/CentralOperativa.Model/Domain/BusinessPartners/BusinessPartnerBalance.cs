using CentralOperativa.Domain.System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.BusinessPartners
{
    [Alias("BusinessPartnerBalances")]
    public class BusinessPartnerBalance
    {
        public int AccountId { get; set; }

        public decimal Balance { get; set; }
    }
}