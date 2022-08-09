using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceInterface
{
    [Authenticate]
    public class ApplicationService : Service
    {
        public Session Session
        {
            get { return SessionAs<Session>(); }
        }
    }
}
