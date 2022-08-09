using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Loans
{
    [Flags, EnumAsInt]
    public enum LoanPersonRole : byte
    {
        Collector = 0, //Agente de cobro - EsTenant
        Administrator = 1, //Administrador - EsTenant
        Investor = 2, //Inversor - EsTenant
        Liquidator = 3, //Liquidador - EsTenant
        PortfolioHolder = 4, //Titular de cartera - EsTenant
        Applicant = 5, //Client
        Seller = 6 //Store - EsTenant
    }
}
