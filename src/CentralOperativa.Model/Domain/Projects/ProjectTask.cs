using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Projects
{
    [Alias("ProjectTasks"), Schema("projects")]
    public class ProjectTask
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(ProjectTask))]
        public int ParentId { get; set; }

        [References(typeof(Project))]
        public int ProjectId { get; set; }

        public string Name { get; set; }
    }
}
