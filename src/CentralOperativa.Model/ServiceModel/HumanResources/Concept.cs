using ServiceStack;

namespace CentralOperativa.ServiceModel.HumanResources
{
    public class Concept
    {
        [Route("/payroll/concepts")]
        public class Query : QueryDb<Domain.HumanResources.Concept>
        {
        }

        [Route("/payroll/concepts/{Id}")]
        public class Get
        {
            public int Id { get; set; }
        }

        [Route("/payroll/concepts", "POST")]
        [Route("/payroll/concepts/{Id}", "PUT")]
        public class Post : Domain.HumanResources.Concept
        {
        }
    }
}
