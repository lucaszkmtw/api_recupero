using System;
using ServiceStack.DataAnnotations;
using CentralOperativa.Domain.System.Persons;
using CentralOperativa.Domain.Financials;

namespace CentralOperativa.Domain.CRM.Contacts
{
    [Alias("EmployeeBankAccount")]
    public class EmployeeBankAccount
    {
        [AutoIncrement]
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int BankAccountId { get; set; }
    }
}
