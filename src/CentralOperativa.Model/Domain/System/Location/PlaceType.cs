using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Location
{
    [Alias("PlaceTypes")]
    public class PlaceType
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
