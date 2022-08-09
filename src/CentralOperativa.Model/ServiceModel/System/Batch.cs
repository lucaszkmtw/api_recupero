using System;
using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.System
{
    public class Batch
    {
        [Route("/system/batches/{Id}", "GET")]
        public class GetBatch
        {
            public int Id { get; set; }
        }

        [Route("/system/batches", "GET")]
        public class QueryBatches : QueryDb<Domain.System.Batch, QueryBatchesResult>
            , IJoin<Domain.System.Batch, Domain.System.Persons.Person>
            , IJoin<Domain.System.Batch, Domain.System.User>
        {
            public string[] Types { get; set; }
        }

        [Route("/system/batches/lookup", "GET")]
        public class Lookup : LookupRequest, IReturn<List<LookupItem>>
        {
        }

        public class QueryBatchesResult
        {
            public int Id { get; set; }
            public int PersonId { get; set; }
            public string PersonName { get; set; }
            public int UserId { get; set; }
            public string UserName { get; set; }
            public DateTime Date { get; set; }
            public string State { get; set; }
            public string Period { get; set; }

            public string Type { get; set; }
            public int Items { get; set; }
        }

        [Route("/system/batches/{Id}/distribute", "POST")]
        public class DistributeBatch
        {
            public int Id { get; set; }
        }

        [Route("/system/batches/{Id}/import", "POST")]
        public class ImportBatch
        {
            public int Id { get; set; }
        }

        [Route("/system/batches", "POST")]
        public class PostBatch
        {
            public byte ModuleId { get; set; }

            public string Period { get; set; }

            public int PersonId { get; set; }
        }
    }
}
