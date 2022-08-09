using CentralOperativa.Domain.System.Location;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Projects
{
    [Alias("ProjectPlaces"), Schema("projects")]
    public class ProjectPlace
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Project))]
        public int ProjectId { get; set; }

        [References(typeof(Place))]
        public int PlaceId { get; set; }
    }
}
