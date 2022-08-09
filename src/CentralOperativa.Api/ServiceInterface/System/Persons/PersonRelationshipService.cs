using CentralOperativa.Domain.System.Persons;
using ServiceStack;
using ServiceStack.OrmLite;
using PersonRelationship = CentralOperativa.ServiceModel.System.Persons.PersonRelationship;

namespace CentralOperativa.ServiceInterface.System.Persons
{
    [Authenticate]
    public class PersonRelationshipService : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }


        public object Get(PersonRelationship.Get request)
        {

            return Db.From<Person, Domain.System.Persons.PersonRelationship>
                ((p, r) => p.Id == r.FromPersonId || p.Id == r.ToPersonId)
                    .FullJoin<Domain.System.Persons.PersonRelationship,
                    PersonRelationshipType>((r, t) => r.TypeId == t.Id)
                    .Select<Person, PersonRelationshipType>
                    ((p, t) => new { p.FirstName, p.LastName, t.Name } )
                    .Where(p => p.Id == request.Id);
        }

        public object Any (PersonRelationship.Query request)
        {
            var query = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            return AutoQuery.Execute(request, query);
        }
    }
}