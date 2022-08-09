using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Persons
{
    [Alias("Nosis")]
    public class Nosis
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public string Data { get; set; }
    }
}