using System;
using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.System
{
    [Route("/system/tenants/check", "GET")]
    public class CheckTenants
    {
    }

    [Route("/system/tenants/lookup", "GET")]
    public class LookupTenant : LookupRequest, IReturn<List<LookupItem>>
    {
    }

    [Route("/system/tenants/{Id}", "GET")]
    public class GetTenant
    {
        public int Id { get; set; }
    }

    [Route("/system/tenants", "GET")]
    public class QueryTenants : QueryDb<Domain.System.Tenant, GetTenantResponse>, IJoin<Domain.System.Tenant, Domain.System.Persons.Person>
    {
    }

    [Route("/system/tenants", "POST")]
    [Route("/system/tenants/{Id}", "PUT")]
    public class PostTenant : Domain.System.Tenant
    {
    }

    [Route("/system/tenants/{Id}", "DELETE")]
    public class DeleteTenant
    {
        public int Id { get; set; }
    }

    public class GetTenantResponse
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int PersonId { get; set; }

        public Guid FolderGuid { get; set; }

        public string PersonCode { get; set; }

        public string PersonName { get; set; }

        public int TenantUserId { get; set; }

        public bool TenantUserIsDefault { get; set; }
    }
}
