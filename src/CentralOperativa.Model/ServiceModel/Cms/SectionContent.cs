using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Cms
{
    [Route("/cms/sectioncontents/highlight", "POST")]
    public class HighlightSectionContent
    {
        public int Id { get; set; }

        public bool Value { get; set; }
    }

    [Route("/cms/sectioncontents/move", "POST")]
    public class MoveSectionContent
    {
        public int SourceId { get; set; }

        public int TargetId { get; set; }

        public byte Position { get; set; }
    }

    [Route("/cms/sectioncontents", "GET")]
    [Route("/cms/sections/{SectionId}/contents", "GET")]
    public class SearchSectionContents : LookupRequest
    {
        public int? SectionId { get; set; }
        public bool OnlyPublished { get; set; }
        public bool OnlyTitle { get; set; }
        public bool ChildSections { get; set; }
    }

    public class GetSectionContentResponse
    {
        public int Id { get; set; }

        public short DisplayOrder { get; set; }

        public bool Highlighted { get; set; }

        public Domain.Cms.Section Section { get; set; }

        public Domain.Cms.Content Content { get; set; }
    }

    public class GetSectionContentTitleResponse : Domain.Cms.SectionContent
    {
        public string Title { get; set; }
    }

    [Route("/cms/sectioncontents/lookup", "GET")]
    [Route("/cms/sectioncontents/lookup/{Id}", "GET")]
    public class LookupSectionContents : LookupRequest
    {
        public int? SectionId { get; set; }
    }

    [Route("/cms/sectioncontents/{Id}", "GET")]
    public class GetSectionContent : IReturn<Domain.Cms.SectionContent>
    {
        public int Id { get; set; }
    }

    [Route("/cms/sectioncontents", "POST")]
    [Route("/cms/sectioncontents/{Id}", "PUT")]
    public class PostSectionContent : Domain.Cms.SectionContent
    {
    }

    [Route("/cms/sectioncontents/{Id}", "DELETE")]
    public class DeleteSectionContent
    {
        public int Id { get; set; }
    }
}