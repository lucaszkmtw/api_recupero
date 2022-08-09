using System;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Cms
{
    [Route("/cms/contents", "GET")]
    public class QueryContents : QueryDb<Domain.Cms.Content, QueryContentsResult>
    {
    }

    public class QueryContentsResult
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public DateTime CreateDate { get; set; }
    }

    [Route("/cms/contents/lookup", "GET")]
    public class LookupContents : LookupRequest
    {
        public int? SectionId { get; set; }
    }

    [Route("/cms/contents/{Id}", "GET")]
    public class GetContent : IReturn<Domain.Cms.Content>
    {
        public int Id { get; set; }
    }

    [Route("/cms/contents", "POST")]
    [Route("/cms/contents/{Id}", "PUT")]
    public class PostContent : Domain.Cms.Content
    {
    }

    [Route("/cms/contents/{Id}", "DELETE")]
    public class DeleteContent : IReturnVoid
    {
        public int Id { get; set; }
    }

    [Route("/cms/contents/check/{Id}", "GET")]
    public class ContentCheckGetRequest
    {
        public int Id { get; set; }
        public bool OutputBodyOnly { get; set; }

        public ContentCheckGetRequest()
        {
            OutputBodyOnly = true;
        }
    }
}