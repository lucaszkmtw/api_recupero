using ServiceStack.DataAnnotations;

using CentralOperativa.Domain.System.Persons;

namespace CentralOperativa.Domain.Projects
{
    [Alias("ProjectMembers"), Schema("projects")]
    public class ProjectMember
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Project))]
        public int ProjectId { get; set; }

        [References(typeof(Person))]
        public int PersonId { get; set; }

        [References(typeof(ProjectMemberRole))]
        public int RoleId { get; set; }

        public string Description { get; set; }
    }
}
