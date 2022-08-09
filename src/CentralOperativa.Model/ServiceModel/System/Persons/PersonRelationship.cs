using CentralOperativa.Domain.System.Persons;
using ServiceStack;

namespace CentralOperativa.ServiceModel.System.Persons
{
    public class PersonRelationship
    {
        [Route("/persons/{Id}/relationships", "GET")]
        public class Get : IReturn<RelationShip>
        {
            public int Id { get; set; }
        }


        [Route("/persons/{Id}/relationships", "GET")]
        public class Query : QueryDb<Domain.System.Persons.Person, RelationShip>,
            IJoin<Domain.System.Persons.PersonRelationship, Domain.System.Persons.Person,
                                                    PersonRelationshipType>
        {
        }

        public class RelationShip {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public string RelationShipTypeName { get; set; }

        }

        [Route("/persons/{Id}/relationships", "PUT")]
        [Route("/persons/{Id}/relationships", "POST")]
        public class Post : Domain.System.Persons.PersonRelationship
        {
            public string TypeName { get; set; }
        }

        [Route("/persons/{Id}/relationships", "DELETE")]
        public class Delete 
        {
            public int Id { get; set; }
        }

    }
}
