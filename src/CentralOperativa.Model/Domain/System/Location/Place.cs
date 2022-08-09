using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Location
{
    [Alias("Places")]
    public class Place
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Place))]
        public int? ParentId { get; set; }

        [References(typeof(PlaceType))]
        public int TypeId { get; set; }

        public string Name { get; set; }

        public string Geo { get; set; }
    }
}
