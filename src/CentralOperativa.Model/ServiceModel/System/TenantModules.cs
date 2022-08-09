using ServiceStack;

namespace CentralOperativa.ServiceModel.System
{
    public class TenantModule
    {
        [Route("/system/tenants/{TenantId}/modules/{ModuleId}", "GET")]
        public class GetTenantModule
        {
            public int Id { get; set; }
        }

        [Route("/system/tenants/{TenantId}/modules", "GET")]
        public class QueryTenantModules : QueryDb<Domain.System.TenantModule, GetTenantModuleResponse>
            , IJoin<Domain.System.TenantModule, Domain.System.Module>
        {
            public int TenantId { get; set; }
        }

        [Route("/system/tenants/{TenantId}/modules", "POST")]
        [Route("/system/tenants/{TenantId}/modules/{Id}", "PUT")]
        public class PostTenantModule : Domain.System.TenantModule
        {
        }

        public class GetTenantModuleResponse
        {
            public int Id { get; set; }

            public int ModuleId { get; set; }

            public string ModuleName { get; set; }
        }
    }
}
