namespace CentralOperativa.Infraestructure
{
    public class UserAuth : ServiceStack.Auth.UserAuth
    {
        public int TentantUserId { get; set; }
        public int TenantId { get; set; }
    }
}