using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.System.Persons
{
    public class Skill
    {
        [Route("/system/persons/skills/{Id}", "GET")]
        public class Get
        {
            public int Id { get; set; }
        }

        [Route("/system/persons/skills", "POST")]
        [Route("/system/persons/skills/{Id}", "PUT")]
        public class Post : Domain.System.Persons.Skill
        {
        }

        [Route("/system/persons/skills", "GET")]
        public class Query : QueryDb<Domain.System.Persons.Skill, QueryResult>
        {
        }

        [Route("/system/persons/skills/lookup", "GET")]
        public class Lookup : LookupRequest, IReturn<List<LookupItem>>
        {
        }

        public class QueryResult
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }
    }
}
