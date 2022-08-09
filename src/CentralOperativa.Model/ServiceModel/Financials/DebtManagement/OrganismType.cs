using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;
using CentralOperativa.Domain.Financials.DebtManagement;
using CentralOperativa.Domain.BusinessPartners;

namespace CentralOperativa.ServiceModel.Financials.DebtManagement
{
    [Route("/financials/debtmanagement/organismtypes/{Id}", "GET")]
    public class GetOrganismType
    {
        public int Id { get; set; }
    }

    [Route("/financials/debtmanagement/organismtypes", "POST")]
    [Route("/financials/debtmanagement/organismtypes/{Id}", "PUT")]
    public class PostOrganismtype : OrganismType
    {
    }

    [Route("/financials/debtmanagement/organismtypes/{Id}", "DELETE")]
    public class DeleteOrganismType : OrganismType
    {
    }
    /*
    // si no hay que traer datos de otra tabla -> se usa asi
    [Route("/financials/debtmanagement/organismtypes", "GET")]
    public class QueryOrganismTypes
    {
        public int? Skip { get; set; }
        public int? Take { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
       // public string BusinessPartnerTypeName { get; set; }
    }
    */
    [Route("/financials/debtmanagement/organismtypes", "GET")]
    public class QueryOrganismTypes : QueryDb<OrganismType, QueryOrganismTypeResult>
            , IJoin<OrganismType, BusinessPartnerType>            
    {
    }

    public class QueryOrganismTypeResult
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string BusinessPartnerTypeName { get; set; }        
    }

    [Route("/financials/debtmanagement/organismtypes/lookup", "GET")]
    public class LookupOrganismType : LookupRequest, IReturn<List<LookupItem>>
    {
    }
}
