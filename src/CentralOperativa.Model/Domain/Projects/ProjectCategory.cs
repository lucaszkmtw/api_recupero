using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Projects
{
    [Alias("ProjectCategories"), Schema("projects")]
    public class ProjectCategory
    {
        [AutoIncrement]
        public int Id { get; set; }
        
        [References(typeof(Project))]
        public int ProjectId { get; set; }

        [References(typeof(Category))]
        public int CategoryId { get; set; }

        [Ignore]
        public string Name { get { return ProjectId + " - " + CategoryId; } }
    }
}
