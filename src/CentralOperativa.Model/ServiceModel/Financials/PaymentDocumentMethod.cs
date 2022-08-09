using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;
using System;

namespace CentralOperativa.ServiceModel.Financials
{
    public class PaymentDocumentMethod
    {
        [Route("/financials/paymentdocumentitems/{Id}", "GET")]
        public class Get
        {
            public int Id { get; set; }
        }

        [Route("/financials/paymentdocumentitems/checks/{Id}", "GET")]
        public class GetCheck
        {
            public int Id { get; set; }
        }

        [Route("/financials/paymentdocumentitems", "POST")]
        [Route("/financials/paymentdocumentitems/{Id}", "PUT")]
        public class Post : Domain.Financials.PaymentDocumentMethod
        {
        }

        [Route("/financials/paymentdocumentitems", "GET")]
        public class Find : QueryDb<Domain.Financials.PaymentDocumentMethod, QueryResult>
            , IJoin<Domain.Financials.PaymentDocumentMethod, Domain.Financials.PaymentMethod>
        {
            public string Description { get; set; }
        }

        [Route("/financials/paymentdocumentitems/{userId}/{paymentMethodId}", "GET")]
        public class FindByPaymentMethod : QueryDb<Domain.Financials.PaymentDocumentMethod, QueryResultByType>
            , IJoin<Domain.Financials.PaymentDocumentMethod, Domain.Financials.PaymentMethod>
            , IJoin<Domain.Financials.PaymentDocumentMethod, Domain.Financials.PaymentDocument>
            //, IJoin<Domain.Financials.PaymentDocument, Domain.System.Persons.Person>
            , ILeftJoin<Domain.Financials.PaymentDocumentMethod, Domain.Financials.BankAccount>
            , ILeftJoin<Domain.Financials.PaymentDocumentMethod, Domain.Financials.CheckBook>
            , ILeftJoin<Domain.Financials.PaymentDocumentMethod, Domain.System.Persons.Person>
        {
            public int UserId { get; set; }
            public int PaymentMethodId { get; set; }
        }
        
        //provisorio hasta terminar la tarea. Javier.enero 2017
        [Route("/financials/paymentdocumentitems2/{paymentMethodId}", "GET")]
        public class FindByPaymentMethod2 : QueryDb<Domain.Financials.PaymentDocumentMethod, QueryResultByType>
            , IJoin<Domain.Financials.PaymentDocumentMethod, Domain.Financials.PaymentMethod>
            , IJoin<Domain.Financials.PaymentDocumentMethod, Domain.Financials.PaymentDocument>
            //, IJoin<Domain.Financials.PaymentDocument, Domain.System.Persons.Person>
            , ILeftJoin<Domain.Financials.PaymentDocumentMethod, Domain.Financials.BankAccount>
            , ILeftJoin<Domain.Financials.PaymentDocumentMethod, Domain.Financials.CheckBook>
            , ILeftJoin<Domain.Financials.PaymentDocumentMethod, Domain.System.Persons.Person>
        {
            public int PaymentMethodId { get; set; }
            public BusinessDocumentModule Module { get; set; }
        }

        [Route("/financials/checksincustody", "GET")]
        public class ChecksInCustody : QueryDb<Domain.Financials.PaymentDocumentMethod, QueryResultChecksInCustody>
            , IJoin<Domain.Financials.PaymentDocumentMethod, Domain.Financials.PaymentMethod>
            , ILeftJoin<Domain.Financials.PaymentDocumentMethod, Domain.System.Persons.Person>
            , ILeftJoin<Domain.Financials.PaymentDocumentMethod, Domain.Financials.BankAccount>
        {
            public byte View { get; set; }
        }

        [Route("/financials/paymentdocumentitems/lookup", "GET")]
        public class Lookup : LookupRequest, IReturn<List<LookupItem>>
        {
        }

        [Route("/financials/paymentdocumentitems/lookupcheckstatus", "GET")]
        public class LookupCheckStatus : LookupRequest, IReturn<List<LookupItem>>
        {
        }
        [Route("/financials/paymentdocumentitems/lookupcashtransactionstatus", "GET")]
        public class LookupCashTransaction : LookupRequest, IReturn<List<LookupItem>>
        {
        }
        [Route("/financials/paymentdocumentitems/lookupbanktransfer", "GET")]
        public class LookupBankTransfer : LookupRequest, IReturn<List<LookupItem>>
        {
        }

        [Route("/financials/checksincustody/deposit", "POST")]
        public class PostChecksincustodyDeposit
        {
            public string DepositNumber { get; set; }

            public DateTime DepositDate { get; set; }

            public int BankAccountId { get; set; }

            public List<int> PaymentDocumentMethodIds { get; set; }
        }

        public enum BusinessDocumentModule
        {
            All,
            AccountsPayables,
            AccountsReceivables
        }

        public class QueryResult
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string PaymentMethodTypeName { get; set; }
        }

        // [References(typeof(PaymentDocument))]
        //public int PaymentDocumentId { get; set; }

        //[References(typeof(PaymentMethod))]
        //public int PaymentMethodId { get; set; }

        //[References(typeof(BankAccount))]
        //public int? BankAccountId { get; set; }

        //[References(typeof(CheckBook))]
        //public int? CheckBookId { get; set; }

        //[References(typeof(Person))]
        //public int? IssuerId { get; set; }

        public class QueryResultByType
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public DateTime? IssueDate { get; set; }
            public string Status { get; set; }
            public string FinancingStatus { get; set; }
            public decimal Amount { get; set; }
            public int PaymentDocumentId { get; set; }
            public string PaymentDocumentNumber { get; set; }
            public string PaymentDocumentReciverName { get; set; }
            public string PaymentMethodId { get; set; }
            public string PaymentMethodName { get; set; }
            public string BankAccounCode { get; set; }
            public string BankAccounNumber { get; set; }
            public string CheckBookName { get; set; }
            public DateTime CheckBookVoidDate { get; set; }
            public string CheckBookStatus { get; set; }
            public string PersonName { get; set; }
        }

        public class QueryResultChecksInCustody
        {
            public int Id { get; set; }
            public string IssuerPaymentDocumentNumber { get; set; }
            public string CheckNumber { get; set; }
            public string BankAccountDescription { get; set; }
            public string PersonName { get; set; }
            public decimal Amount { get; set; }
            public DateTime? IssueDate { get; set; }
            public DateTime? VoidDate { get; set; }
            public DateTime? DepositDate { get; set; }
        }
    }
}

