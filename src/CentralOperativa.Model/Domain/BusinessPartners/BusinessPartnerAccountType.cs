using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.BusinessPartners
{
    [Flags, EnumAsInt]
    public enum BusinessPartnerAccountType
    {
        InvestmentsAccount,
        CustodyAccount
    }
}
