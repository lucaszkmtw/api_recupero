using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;
using CentralOperativa.Domain.System.Persons;
using CentralOperativa.Domain.Financials.DebtManagement;

namespace CentralOperativa.ServiceModel.Financials.DebtManagement
{
    [Route("/financials/debtmanagement/proxielicensetypes/{Id}", "GET")]
    public class GetProxieLicenseType
    {
        public int Id { get; set; }
    }

    [Route("/financials/debtmanagement/proxielicensetypes", "POST")]
    [Route("/financials/debtmanagement/proxielicensetypes/{Id}", "PUT")]
    public class PostProxieLicenseType : ProxieLicenseType
    {
        public List<LicenseType> LicenseTypes
        {
            get; set;
        }
    }

    [Route("/financials/debtmanagement/proxielicensetypes/{Id}", "DELETE")]
    public class DeleteProxieLicenseType : ProxieLicenseType
    {
    }

    [Route("/financials/debtmanagement/proxielicensetypes", "GET")]
    public class QueryProxieLicenseTypes : QueryDb<ProxieLicenseType, QueryProxieLicenseTypeResult>
        , IJoin<ProxieLicenseType, LicenseType>        
        , IJoin<ProxieLicenseType, Proxie>
        , IJoin<Proxie, Person>
    {

    }

    public class QueryProxieLicenseTypeResult
    {
        public int Id { get; set; }
        public string LicenseTypeName { get; set; }
        public string PersonName { get; set; }
        public int ProxieId { get; set; }
        public int ProxieStatus { get; set; }
        public int ProxieLicenseTypeStatus { get; set; }
       
    }

    [Route("/financials/debtmanagement/proxielicensetypes/lookup", "GET")]
    public class LookupProxieLicenseType : LookupRequest, IReturn<List<LookupItem>>
    {

    }

    public class GetProxieLicenseTypeResult : ProxieLicenseType
    {
        public string PersonName { get; set; }
    }

    public class ProxieLicenseType : Domain.Financials.DebtManagement.ProxieLicenseType
    {
        public ProxieLicenseType()
        {
        }
    }


}
