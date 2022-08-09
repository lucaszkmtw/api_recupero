using System.Collections.Generic;
using ServiceStack;
using CentralOperativa.Infraestructure;

namespace CentralOperativa.ServiceModel.Projects
{
    [Route("/projects/categories/{Id}", "GET")]
    public class GetCategory : IReturn<Category>
    {
        public int Id { get; set; }
    }

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [Route("/projects/categories", "POST")]
    [Route("/projects/categories/{Id}", "PUT")]
    public class PostCategory : Domain.Projects.Category, IReturn<Category>
    {
    }

    [Route("/projects/categories/{Id}", "DELETE")]
    public class DeleteCategory : IReturnVoid
    {
        public int Id { get; set; }
    }

    [Route("/projects/categories", "GET")]
    public class QueryCategories : QueryDb<Domain.Projects.Category, Category>
    {
        public string Description { get; set; }
    }

    [Route("/projects/categories/lookup", "GET")]
    public class LookupCategories : LookupRequest, IReturn<List<LookupItem>>
    {
    }
}