using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;
using CentralOperativa.Domain.System.Persons;
using CentralOperativa.Domain.Financials.DebtManagement;
using CentralOperativa.Domain.Catalog;

namespace CentralOperativa.ServiceModel.Financials.DebtManagement
{
    [Route("/financials/debtmanagement/organismlaws/{Id}", "GET")]
    public class GetOrganismLaw
    {
        public int Id { get; set; }
    }

    [Route("/financials/debtmanagement/organismlaws", "POST")]
    [Route("/financials/debtmanagement/organismlaws/{Id}", "PUT")]
    public class PostOrganismLaw : OrganismLaw
    {
        public List<Law> Laws
        {
            get; set;
        }
    }

    [Route("/financials/debtmanagement/organismlaws/{Id}", "DELETE")]
    public class DeleteOrganismLaw : OrganismLaw
    {
    }

    [Route("/financials/debtmanagement/organismlaws", "GET")]
    public class QueryOrganismLaws : QueryDb<OrganismLaw, QueryOrganismLawResult>
        , IJoin<OrganismLaw, Law>
        , IJoin<OrganismLaw, Organism>
        , IJoin<Organism, Person>
    {

    }

    public class QueryOrganismLawResult
    {
        public int Id { get; set; }
        public string LawName { get; set; }
        public string PersonName { get; set; }
        public int OrganismId { get; set; }
        public int OrganismStatus { get; set; }
        public int OrganismProductStatus { get; set; }
    }

    [Route("/financials/debtmanagement/organismlaws/lookup", "GET")]
    public class LookupOrganismLaw : LookupRequest, IReturn<List<LookupItem>>
    {

    }

    public class GetOrganismLawResult : OrganismLaw
    {
        public string PersonName { get; set; }
    }

    public class OrganismLaw : Domain.Financials.DebtManagement.OrganismLaw
    {
        public OrganismLaw()
        {
        }
    }


}