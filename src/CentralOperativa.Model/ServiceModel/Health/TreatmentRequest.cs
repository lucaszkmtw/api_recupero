using System;
using System.Collections.Generic;
using CentralOperativa.Domain.Catalog;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Health
{
    public class TreatmentRequest
    {
        [Route("/health/treatmentrequests/{Id}", "GET")]
        public class Get : IReturn<GetResponse>
        {
            public int Id { get; set; }
        }

        [Route("/health/treatmentrequests", "POST")]
        [Route("/health/treatmentrequests/{Id}", "PUT")]
        public class Post : Domain.Health.TreatmentRequest
        {
            public Post()
            {
                this.Diagnostics = new List<PatientDiagnostic>();
                this.Products = new List<Product>();
                this.Drugs = new List<Drug>();
                this.Practices = new List<Practice>();
            }

            public ServiceModel.System.Workflows.WorkflowInstance.PostWorkflowInstance WorkflowInstance { get; set; }

            public int DoctorId { get; set; }

            public string Comments { get; set; }

            public List<PatientDiagnostic> Diagnostics { get; set; }

            public List<Product> Products { get; set; }

            public List<Drug> Drugs { get; set; }

            public List<Practice> Practices { get; set; }

            public class PatientDiagnostic
            {
                public int? Id { get; set; }

                public int DiseaseId { get; set; }

                public string Comments { get; set; }
            }

            public class Product
            {
                public int? Id { get; set; }

                public int ProductId { get; set; }

                public decimal Quantity { get; set; }

                public string Comments { get; set; }

                public int? VendorId { get; set; }

                public decimal? Price { get; set; }
            }

            public class Drug
            {
                public int? Id { get; set; }

                public int DrugId { get; set; }

                public int? CommercialDrugId { get; set; }

                public decimal Quantity { get; set; }

                public string Frequency { get; set; }

                public string Comments { get; set; }

                public int? VendorId { get; set; }

                public decimal? Price { get; set; }
            }

            public class Practice
            {
                public int? Id { get; set; }

                public int MedicalPracticeId { get; set; }

                public decimal Quantity { get; set; }

                public string Frequency { get; set; }

                public string Comments { get; set; }

                public int? VendorId { get; set; }

                public DateTime? FromDate { get; set; }

                public DateTime? ToDate { get; set; }
            }
        }

        [Route("/health/treatmentrequests", "GET")]
        public class Query : QueryDb<Domain.Health.TreatmentRequest, QueryResult>
        {
            public byte View { get; set; }
            public int? PersonId { get; set; }
            public string Q { get; set; }
        }

        [Route("/health/treatmentrequests/lookup", "GET")]
        public class Lookup : LookupRequest, IReturn<List<LookupItem>>
        {
        }

        public class QueryResult
        {
            public int Id { get; set; }
            public DateTime Date { get; set; }
            public int PatientId { get; set; }
            public int PersonId { get; set; }
            public string PersonName { get; set; }
            public string WorkflowCode { get; set; }
            public int WorkflowActivityId { get; set; }
            public bool WorkflowActivityIsFinal { get; set; }
            public string WorkflowActivityName { get; set; }
            public string Roles { get; set; }
            public bool WorkflowInstanceIsTerminated { get; set; }
            public int WorkflowInstanceId { get; set; }
            public decimal WorkflowInstanceProgress { get; set; }
            public DateTime WorkflowInstanceCreateDate { get; set; }
        }

        public class GetResponse : Domain.Health.TreatmentRequest
        {
            public GetResponse()
            {
                this.Diagnostics = new List<PatientDiagnostic>();
                this.Drugs = new List<TreatmentRequestDrug>();
                this.Products = new List<TreatmentRequestProduct>();
                this.Practices = new List<TreatmentRequestPractice>();
            }

            public ServiceModel.System.Workflows.WorkflowInstance.GetWorkflowInstanceResponse WorkflowInstance { get; set; }

            public int MessageCount { get; set; }

            public List<PatientDiagnostic> Diagnostics { get; set; }

            public List<TreatmentRequestDrug>  Drugs { get; set; }

            public List<TreatmentRequestProduct> Products { get; set; }

            public List<TreatmentRequestPractice> Practices { get; set; }

            public class PatientDiagnostic
            {
                public int Id { get; set; }
                public int DoctorId { get; set; }
                public string DoctorName { get; set; }
                public DateTime Date { get; set; }
                public string Comments { get; set; }
                public int DiseaseId { get; set; }
                public ServiceModel.Health.Disease.GetResponse Disease { get; set; }
            }

            public class TreatmentRequestPractice : Domain.Health.TreatmentRequestPractice
            {
                public Domain.Health.MedicalPractice MedicalPractice { get; set; }
                public ServiceModel.Procurement.Vendor.GetVendorResult Vendor { get; set; }
            }

            public class TreatmentRequestDrug : Domain.Health.TreatmentRequestDrug
            {
                public Domain.Health.Drug Drug { get; set; }

                public CommercialDrug.QueryResult CommercialDrug { get; set; }
            }

            public class TreatmentRequestProduct : Domain.Health.TreatmentRequestProduct
            {
                public Product Product { get; set; }
                public ServiceModel.Procurement.Vendor.GetVendorResult Vendor { get; set; }
            }
        }
    }
}
