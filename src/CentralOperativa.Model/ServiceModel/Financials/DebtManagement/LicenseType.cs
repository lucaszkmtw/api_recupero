using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Financials.DebtManagement
{
    [Route("/financials/debtmanagement/licensetypes/{Id}", "GET")]
    public class GetLicenseType
    {
        public int Id { get; set; }
    }

    [Route("/financials/debtmanagement/licensetypes", "POST")]
    [Route("/financials/debtmanagement/licensetypes/{Id}", "PUT")]
    public class PostLicenseType : Domain.Financials.DebtManagement.LicenseType
    {
    }

    [Route("/financials/debtmanagement/licensetypes/{Id}", "DELETE")]
    public class DeleteLicenseType : Domain.Financials.DebtManagement.LicenseType
    {
    }
    /*
    [Route("/financials/debtmanagement/licensetypes", "GET")]
    public class QueryLicenseTypes : QueryDb<Domain.Financials.DebtManagement.LicenseType>
    {
        // public int? Skip { get; set; }
        //public int? Take { get; set; }
        //public string Name { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }
    }
    */
    
    [Route("/financials/debtmanagement/licensetypes/lookup", "GET")]
    public class LookupLicenseType : LookupRequest, IReturn<List<LookupItem>>
    {
    }

    [Route("/financials/debtmanagement/licensetypes", "GET")]
    public class QueryLicenseType : QueryDb<Domain.Financials.DebtManagement.LicenseType, QueryLicenseTypeResult>
    {
    }

    public class QueryLicenseTypeResult
    {
        public int Id { get; set; }        
        public string Name { get; set; }
        public int Status { get; set; }
    }
    
    public class GetLicenseTypeResponse : Domain.Financials.DebtManagement.LicenseType
    {
    }





}
