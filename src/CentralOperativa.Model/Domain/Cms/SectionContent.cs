using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Cms
{
    [Alias("SectionContents")]
    public class SectionContent
    {
        [AutoIncrement]
        public int Id { get; set; }

        public int SectionId { get; set; }

        public int ContentId { get; set; }

        public short DisplayOrder { get; set; }

        public bool Highlighted { get; set; }
    }
}