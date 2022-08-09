using System;
using CentralOperativa.Domain.System.Persons;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Health
{
    [Alias("CommercialDrugs")]
    public class CommercialDrug
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Drug))]
        public int DrugId { get; set; }

        [References(typeof(DrugPresentation))]
        public int? PresentationId { get; set; }

        [References(typeof(Person))]
        public int? ManufacturerId { get; set; }

        [References(typeof(DrugTherapeuticEffect))]
        public int? TherapeuticEffectId { get; set; }

        [References(typeof(DrugAdministrationRoute))]
        public short? AdministrationRouteId { get; set; }

        [References(typeof(DrugUnitType))]
        public short? UnitTypeId { get; set; }

        public int? DrugDosageId { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string PresentationName { get; set; }

        public string Size { get; set; }

        public short Units { get; set; }

        public decimal Price { get; set; }
        
        public DateTime? PriceValidity { get; set; }

        public bool Vat { get; set; }

        public bool Taxed { get; set; }

        public string BarCode { get; set; }

        public bool Imported { get; set; }

        public bool Refrigerated { get; set; }

        public bool Enabled { get; set; }

        public byte SaleType { get; set; }

        public decimal IOMAPrice { get; set; }

        public string IOMACategory { get; set; }

        public string PAMIDisccount { get; set; }

        public bool SIFAR { get; set; }
    }
}