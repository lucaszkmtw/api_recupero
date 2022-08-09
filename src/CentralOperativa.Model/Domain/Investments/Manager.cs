﻿using ServiceStack.DataAnnotations;
using CentralOperativa.Domain.BusinessPartners;
using CentralOperativa.Domain.System.Persons;


namespace CentralOperativa.Domain.Investments
{
    [Alias("Managers"), Schema("investments")]
    public class Manager
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Person))]
        public int PersonId { get; set; }

        [References(typeof(BusinessPartner))]
        public int BusinessPartnerId { get; set; }

        public decimal Commission { get; set; }
    }
}
