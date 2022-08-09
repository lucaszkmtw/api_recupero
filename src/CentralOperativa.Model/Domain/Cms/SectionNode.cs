using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Cms
{
    [Alias("SectionNodes")]
    public class SectionNode : Domain.Cms.Section
    {
        public string Path { get; set; }

        public int ChildCount { get; set; }
    }
}