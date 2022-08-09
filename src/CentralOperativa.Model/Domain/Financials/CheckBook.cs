using ServiceStack.DataAnnotations;
using System;

namespace CentralOperativa.Domain.Financials
{
    [Alias("CheckBooks")]
    public class CheckBook
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(BankAccount))]
        public int BankAccountId{ get; set; }
        public string Name { get; set; }
        public string FromNumber { get; set; }
        public string ToNumber { get; set; }
        public string NextNumber { get; set; }
        public bool Autoincrement { get; set; }
        public byte Type{ get; set; }
        public byte Status{ get; set; }

        [Ignore]
        public DateTime VoidDate { get; set; }

        [Ignore]
        public string ShowValue { get { return this.FromNumber + " - " + this.ToNumber; } }
    }
}