using ServiceStack;
using System.Collections.Generic;
using CentralOperativa.Infraestructure;


namespace CentralOperativa.ServiceModel.Catalog
{
    public class Category
    {
        public int Id;

        [Route("/catalog/categories/{Id}", "GET")]
        public class GetCategory
        {
            public int Id { get; set; }
        }


    [Route("/catalog/categories", "GET")]
        public class QueryCategories
        {
            public int? ParentId { get; set; }
        }

        public class GetCategoryResponse
        {
            public GetCategoryResponse()
            {
                this.Items = new List<GetCategoryResponse>();
            }

            public List<GetCategoryResponse> Items { get; set; }

            public int Id { get; set; }

            public int? ParentId { get; set; }

            public string Name { get; set; }
        }

        [Route("/catalog/categories/lookup", "GET")]
        public class LookupCategory : LookupRequest, IReturn<List<LookupItem>>
        {
            public int OrganismId { get; set; }
        }

        [Route("/catalog/categories", "POST")]
        [Route("/catalog/categories/{Id}", "PUT")]
        public class PostCategory : Domain.Catalog.Category
        {
        }

        [Route("/catalog/categories/{Id}", "DELETE")]
        public class DeleteCategory
        {
            public int Id { get; set; }
        }
    }
}
