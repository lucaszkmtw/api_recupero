using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.System.Workflows
{
    public class Process
    {
        [Route("/system/workflows/processes/{Id}")]
        public class GetProcess
        {
            public int Id { get; set; }
        }

        [Route("/system/workflows/processes")]
        public class QueryProcesses : QueryDb<Domain.System.Workflows.Process>
        {
        }

        [Route("/system/workflows/processes/lookup", "GET")]
        public class LookupProcess : LookupRequest, IReturn<List<LookupItem>>
        {
        }
    }
}
