using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Location
{
    [Alias("PlaceNodes")]
    public class PlaceNode : Domain.System.Location.Place
    {
        public string Path { get; set; }

        public int ChildCount { get; set; }
    }
}
