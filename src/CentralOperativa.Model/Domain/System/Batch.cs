using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System
{
    [Alias("Batches")]
    public class Batch
    {
        [AutoIncrement]
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public string Period { get; set; }

        [References(typeof(Domain.System.User))]
        public int UserId { get; set; }

        public int Items { get; set; }

        public string Type { get; set; }

        public string State { get; set; }

        [References(typeof(Domain.System.Persons.Person))]
        public int PersonId { get; set; }

        [References(typeof(Domain.System.Module))]
        public byte ModuleId { get; set; }
    }
}