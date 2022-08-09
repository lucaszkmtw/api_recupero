using System;
using ServiceStack;
using System.Collections.Generic;
using CentralOperativa.Domain.Financials;

namespace CentralOperativa.ServiceModel.Financials
{
    [Route("/financials/paymentdocuments/{PaymentDocumentGuid}/confirm", "POST")]
    public class PostConfirmPaymentDocumentRequest
    {
        public Guid PaymentDocumentGuid { get; set; }
    }

    [Route("/financials/paymentdocuments/{PaymentDocumentId}/files/{Guid}", "GET")]
    public class GetPaymentDocumentFilesRequest : IReturn<GetPaymentDocumentFileResponse>
    {
        public int PaymentDocumentId { get; set; }
        public Guid Guid { get; set; }
    }

    [Route("/financials/paymentdocuments", "POST")]
    [Route("/financials/paymentdocuments/{Id}", "POST, PUT")]
    public class PostPaymentDocumentRequest : PaymentDocument, IReturn<GetPaymentDocumentResponse>
    {
        public PostPaymentDocumentRequest()
        {
            this.Methods = new List<Domain.Financials.PaymentDocumentMethod>();
            this.Items = new List<PaymentDocumentItemCollect>();
        }

        public List<PaymentDocumentItemCollect> Items { get; set; }

        public List<Domain.Financials.PaymentDocumentMethod> Methods { get; set; }
        //public int AccountId { get; set; }
    }
    public class PaymentDocumentItemCollect : PaymentDocumentItem
    {
        public int? RelatedDocumentId { get; set; }
        public List<ServiceModel.BusinessDocuments.ParentItem> ParentItems { get; set; }
    }
    [Route("/financials/paymentdocuments/{Id}", "GET")]
    public class GetPaymentDocumentRequest : IReturn<GetPaymentDocumentResponse>
    {
        public int Id { get; set; }
        public bool Edit { get; set; }
    }

    public class GetPaymentDocumentResponse : PaymentDocument
    {
        public GetPaymentDocumentResponse()
        {
            this.Items = new List<PaymentDocumentItem>();
            this.Methods = new List<PaymentDocumentMethodResponse>();
        }

        public List<PaymentDocumentItem> Items { get; set; }

        public List<PaymentDocumentMethodResponse> Methods { get; set; }

        public System.Persons.Person Issuer { get; set; }

        public System.Persons.Person Receiver { get; set; }
        //public int AccountId { get; set; }
    }

    [Route("/financials/paymentdocuments", "GET")]
    public class QueryPaymentDocumentsRequest : QueryDb<PaymentDocument, QueryPaymentDocumentResult>
    {
        public string Q { get; set; }
        public int TypeId { get; set; }
        public string TypeName { get; set; }
        public int Module { get; set; }
    }

    public class QueryPaymentDocumentResult
    {
        public int Id { get; set; }

        public string TypeName { get; set; }

        public DateTime DocumentDate { get; set; }

        public string Number { get; set; }

        public byte Status { get; set; }

        public string IssuerName { get; set; }

        public string ReceiverName { get; set; }

        public decimal Amount { get; set; }

        public int? ApprovalWorkflowInstanceId { get; set; }

        public string Roles { get; set; }
    }

    public class QueryResult
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public DateTime CreateDate { get; set; }
    }

    public class GetPaymentDocumentFileResponse : Domain.System.DocumentManagement.File
    {
    }

    public class PaymentDocumentMethodResponse : Domain.Financials.PaymentDocumentMethod
    {
        public string PaymentMethodName { get; set; }
        public string BankAccountName { get; set; }
    }

    public class GetPaymentDocumentItemResponse : PaymentDocumentItem
    {
        public PaymentDocument PaymentDocument { get; set; }
    }
}
