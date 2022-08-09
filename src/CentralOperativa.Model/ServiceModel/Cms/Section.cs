using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Cms
{
    [Route("/cms/sections/nodes", "GET")]
    public class GetNodes
    {
        public int? Id { get; set; }
    }

    public class SectionNodeResult
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int ChildCount { get; set; }
    }

    [Route("/cms/sections/lookup", "GET")]
    public class LookupSections : LookupRequest, IReturn<List<LookupItem>>
    {
        public bool OnlyPublished { get; set; }

        public bool ShowPath { get; set; }

        public LookupSections()
        {
            OnlyPublished = true;
        }
    }

    [Route("/cms/sections/{Id}", "GET")]
    public class GetSection
    {
        public int Id { get; set; }
        public bool IncludeSubSections { get; set; }
    }

    public class GetSectionResponse : Domain.Cms.Section
    {
        public GetSectionResponse()
        {
            this.Sections = new List<GetSectionResponse>();
            this.Contents = new List<GetSectionContentResponse>();
        }

        public List<GetSectionResponse> Sections { get; set; }

        public List<GetSectionContentResponse> Contents { get; set; }
    }

    [Route("/cms/sections/{Id}/rename", "POST")]
    public class SectionRenameRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [Route("/cms/sections/{Id}/move", "POST")]
    public class SectionMoveRequest
    {
        public int Id { get; set; }
        public int TargetId { get; set; }
        public byte Position { get; set; }
    }

    [Route("/cms/sections", "POST")]
    [Route("/cms/sections/{Id}", "PUT")]
    public class PostSection : Domain.Cms.Section
    {
    }

    [Route("/cms/sections/{Id}", "DELETE")]
    public class DeleteSection : IReturnVoid
    {
        public int Id { get; set; }
    }
}