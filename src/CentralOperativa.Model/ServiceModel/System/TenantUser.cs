using ServiceStack;

namespace CentralOperativa.ServiceModel.System
{
    public class TenantUser
    {
        [Route("/system/tenants/{TenantId}/users/{UserId}", "GET")]
        public class GetTenantUser
        {
            public int Id { get; set; }
        }

        [Route("/system/tenants/{TenantId}/users", "GET")]
        public class QueryTenantUsers : QueryDb<Domain.System.TenantUser, GetTenantUserResponse>
            , IJoin<Domain.System.TenantUser, Domain.System.User>
            , IJoin<Domain.System.User, Domain.System.Persons.Person>
        {
            public int TenantId { get; set; }
        }

        [Route("/system/tenants/{TenantId}/users", "POST")]
        [Route("/system/tenants/{TenantId}/users/{Id}", "PUT")]
        public class PostTenantUser : Domain.System.TenantUser
        {
        }

        public class GetTenantUserResponse
        {
            public int Id { get; set; }

            public int UserId { get; set; }

            public string UserName { get; set; }

            public int PersonId { get; set; }

            public string PersonCode { get; set; }

            public string PersonName { get; set; }
        }
    }
}
