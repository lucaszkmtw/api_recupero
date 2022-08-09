using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;
using CentralOperativa.Domain.System.Persons;
using CentralOperativa.Domain.Financials.DebtManagement;

namespace CentralOperativa.ServiceModel.Financials.DebtManagement
{
        [Route("/financials/debtmanagement/organisms/{Id}", "GET")]
        public class GetOrganism
        {
		    public int Id { get; set; }
		}

        public class OrganismInfo : Domain.Financials.DebtManagement.Organism
        {
            public int AccountId { get; set; }
        }

        [Route("/financials/debtmanagement/organisms", "POST")]
        [Route("/financials/debtmanagement/organisms/{Id}", "PUT")]
		public class PostOrganism : Organism
        {
        }

        [Route("/financials/debtmanagement/organisms/{Id}", "DELETE")]
        public class DeleteOrganism : Organism
        {
        }
    
        [Route("/financials/debtmanagement/organisms", "GET")]
        public class QueryOrganisms : QueryDb<Organism, QueryOrganismResult>
            , IJoin <Domain.Financials.DebtManagement.Organism, OrganismType>
            , IJoin<Domain.Financials.DebtManagement.Organism, Person>
        {
        }
            
        public class QueryOrganismResult
        {
        public int Id { get; set; }
        public string Code { get; set; }
        public string OrganismTypeName { get; set; }
        public string PersonName { get; set; }
        public int PersonId { get; set; }
        public int AccountId { get; set; }
        }
            
        [Route("/financials/debtmanagement/organisms/lookup", "GET")]
        public class LookupOrganism : LookupRequest, IReturn<List<LookupItem>>
        {
            public string BusinessPartnerTypeName { get; set; }
    }

        public class GetOrganismResult : Organism
        {
            public string PersonName { get; set; }
        }
    
        public class Organism : Domain.Financials.DebtManagement.Organism
            {
                public Organism()
                {
                }
            }
}
