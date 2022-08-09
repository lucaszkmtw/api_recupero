using CentralOperativa.Domain.System;
using CentralOperativa.Domain.System.Persons;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Health
{
    [Alias("HealthCareProviders")]
    public class HealthCareProvider
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Tenant))]
        public int TenantId { get; set; }

        [References(typeof(Person))]
        public int PersonId { get; set; }
    }
}