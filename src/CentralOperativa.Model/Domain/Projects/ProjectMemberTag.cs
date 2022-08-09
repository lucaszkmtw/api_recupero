using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Projects
{
    [Alias("ProjectMemberTags"), Schema("projects")]
    public class ProjectMemberTag
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(ProjectMember))]
        public int ProjectMemberId { get; set; }

        public string Name { get; set; }
    }
}
