using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;
using CentralOperativa.Domain.Financials.DebtManagement;

namespace CentralOperativa.ServiceModel.Financials.DebtManagement
{
    [Route("/financials/debtmanagement/normativelaws/{Id}", "GET")]
    public class GetNormativeLaw
    {
        public int Id { get; set; }
        //public int NormativeId { get; set; }
        
    }

    [Route("/financials/debtmanagement/normativelaws", "POST")]
    [Route("/financials/debtmanagement/normativelaws/{Id}", "PUT")]
    public class PostNormativeLaw : NormativeLaw
    {
      
        public List<Law> Laws { get; set; }
    }

    [Route("/financials/debtmanagement/normativelaws/{Id}", "DELETE")]
    public class DeleteNormativeLaw : NormativeLaw
    {
    }

    [Route("/financials/debtmanagement/normativelaws", "GET")]
    public class QueryNormativeLaws : QueryDb<NormativeLaw, QueryNormativeLawResult>
        , IJoin<Domain.Financials.DebtManagement.NormativeLaw, Normative>
        , IJoin<Domain.Financials.DebtManagement.NormativeLaw, Law>
    {
    }

    public class QueryNormativeLawResult
    {
        public int Id { get; set; }
        public int NormativeId { get; set; }
        public string NormativeName { get; set; }
        public string LawName { get; set; }
    }

    [Route("/financials/debtmanagement/normativelaws/lookup", "GET")]
    public class LookupNormativeLaw : LookupRequest, IReturn<List<LookupItem>>
    {
    }

    public class GetNormativeLawResult : NormativeLaw
    {
        public string NormativeName { get; set; }
        public string LawName { get; set; }
    }

    public class NormativeLaw : Domain.Financials.DebtManagement.NormativeLaw
    {
        public NormativeLaw()
        {
        }
    }


}
