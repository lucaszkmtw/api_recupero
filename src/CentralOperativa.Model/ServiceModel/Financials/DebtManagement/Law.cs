using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Financials.DebtManagement
{
    [Route("/financials/debtmanagement/laws/{Id}", "GET")]
    public class GetLaw
    {
        public int Id { get; set; }
    }

    [Route("/financials/debtmanagement/laws", "POST")]
    [Route("/financials/debtmanagement/laws/{Id}", "PUT")]
    public class PostLaw : Domain.Financials.DebtManagement.Law
    {
    }

    [Route("/financials/debtmanagement/laws/{Id}", "DELETE")]
    public class DeleteLaw : Domain.Financials.DebtManagement.Law
    {
    }
   
    [Route("/financials/debtmanagement/laws", "GET")]
    public class QueryLaws
    {
        public int? Skip { get; set; }
        public int? Take { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int Prescription { get; set; }
    }
    
    [Route("/financials/debtmanagement/laws/lookup", "GET")]
    public class LookupLaw : LookupRequest, IReturn<List<LookupItem>>
    {
        public int OrganismId { get; set; }      
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
    }

    [Route("/financials/debtmanagement/credittypes", "GET")]
    public class QueryLaw : QueryDb<Domain.Financials.DebtManagement.Law, QueryResultLaw>
    {
    }

    public class QueryResultLaw
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }


    public class GetResponseLaw : Domain.Financials.DebtManagement.Law
    {
       
    }



}
