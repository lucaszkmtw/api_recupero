using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.System.Persons
{
    public class Group
    {
        [Route("/system/persons/groups/lookup", "GET")]
        public class LookupGroup : LookupRequest, IReturn<List<LookupItem>>
        {
        }

        [Route("/system/persons/groups/{Id}", "GET")]
        public class GetGroup : IReturn<GetGroupResult>
        {
            public int Id { get; set; }
        }

        [Route("/system/persons/groups", "GET")]
        public class QueryGroups : QueryDb<Domain.System.Persons.Group, QueryGroupResult>
        {
        }

        public class QueryGroupResult : Domain.System.Persons.Group
        {
        }


       
        public class GetGroupResult : Domain.System.Persons.Group
        {
        }


        [Route("/system/persons/groups", "POST")]
        [Route("/system/persons/groups/{Id}", "PUT")]
        public class PostGroup : Domain.System.Persons.Group
        {
        }


        [Route("/system/persons/groups/{Id}", "DELETE")]
        public class DeleteGroup
        {
            public int Id { get; set; }
        }
       
    }
}
