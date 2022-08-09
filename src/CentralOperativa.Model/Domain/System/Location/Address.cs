using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Location
{
    [Alias("Addresses")]
    public class Address
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Place))]
        public int PlaceId { get; set; }

        public string Street { get; set; }
        
        public string StreetNumber { get; set; }
        
        public string Floor { get; set; }

        public string Appartment { get; set; }

        public string ZipCode { get; set; }

        [Compute, Ignore]
        public string Name
        {
            get { return string.Join(" ", new[] { this.Street, this.StreetNumber, this.Floor, this.Appartment }).Replace("  ", " ").Replace("  ", " ").Replace("  ", " "); }
        }
    }
}
