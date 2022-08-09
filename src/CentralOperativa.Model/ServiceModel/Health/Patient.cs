using System.Collections.Generic;
using CentralOperativa.Domain.System.Persons;
using ServiceStack;
using CentralOperativa.Infraestructure;

namespace CentralOperativa.ServiceModel.Health
{
    using Api = ServiceModel.Health;
    [Route("/health/patients/{Id}", "GET")]
    public class GetPatient : IReturn<GetPatientResponse>
    {
        public int Id { get; set; }
    }

    public class GetPatientResponse : Domain.Health.Patient
    {
        public ServiceModel.System.Persons.Person Person { get; set; }
        public Api.GetHealthServicePatientResult GetHealthServicePatient { get; set; }
    }

    [Route("/health/patients/{PatientId}/clinicalhistory", "GET")]
    public class GetPatientClinicalHistory : IReturn<List<ServiceModel.Health.TreatmentRequest.GetResponse>>
    {
        public int PatientId { get; set; }
    }

    [Route("/health/patients/{PatientId}/documents", "GET")]
    public class GetPatientDocuments
    {
        public int PatientId { get; set; }
    }

    [Route("/health/patients/batch", "POST")]
    public class PostPatientBatch
    {
        public List<PostPatientBatchItem> Items { get; set; }

        public PostPatientBatch()
        {
            this.Items = new List<PostPatientBatchItem>();
        }

        public class PostPatientBatchItem : Domain.Health.Patient
        {
            public System.Persons.PostPerson Person { get; set; }
            public GetHealthServicePatientResult GetHealthServicePatient { get; set; }
        }
    }

    [Route("/health/patients", "POST")]
    [Route("/health/patients/{Id}", "PUT")]
    public class PostPatient : Domain.Health.Patient
    {
        public Api.GetHealthServicePatientResult GetHealthServicePatient { get; set; }
    }

    [Route("/health/patients", "GET")]
    public class QueryPatients : QueryDb<Domain.Health.Patient, QueryPatientsResult>
        , IJoin<Domain.Health.Patient, Person>
    {
    }

    [Route("/health/patients/lookup", "GET")]
    public class LookupPatient : LookupRequest, IReturn<List<LookupItem>>
    {
    }

    public class QueryPatientsResult
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public string PersonCode { get; set; }
        public string PersonName { get; set; }
        public string Phone { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string HealthService { get; set; }
        public string ServicePlan { get; set; }
        public string CardNumber { get; set; }
    }
}