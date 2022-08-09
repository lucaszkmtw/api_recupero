using System;

namespace CentralOperativa.Domain.BusinessPartners
{
    [Flags]
    public enum BusinessPartnerStatus : byte
    {
        Active,
        Deleted
    }
}
