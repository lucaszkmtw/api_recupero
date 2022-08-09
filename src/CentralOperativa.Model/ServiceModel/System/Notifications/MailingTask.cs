using System;
using System.Collections.Generic;

namespace CentralOperativa.ServiceModel.System.Notifications
{
    public class MailingTask
    {
        public string Campaign { get; set; }
        public EmailAddress From { get; set; }
        public List<EmailAddress> ReplyTo { get; set; }
        public List<EmailAddress> To { get; set; }
        public List<EmailAddress> Bcc { get; set; }
        public List<EmailAddress> OverrideTo { get; set; }
        public string Subject { get; set; }
        public string Template { get; set; }
        public List<Client> Clients { get; set; }
        public Guid Guid { get; set; }
        public bool UseSES { get; set; }
        public List<AttachmentMail> Attachments { get; set; }
        public MailingTask()
        {
            To = new List<EmailAddress>();
            Bcc = new List<EmailAddress>();
            OverrideTo = new List<EmailAddress>();
            ReplyTo = new List<EmailAddress>();
            Attachments = new List<AttachmentMail>();
            this.Clients = new List<Client>();
        }
        public int TemplateId { get; set; }
    }
    public class AttachmentMail
    {
        public byte[] ContentBytes { get; set; }
        public string FileName { get; set; }
        public string Type { get; set; }
    }
    public class Client
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string CUIT { get; set; }
        public string Street { get; set; }
        public string StreetNumber { get; set; }
        public string Floor { get; set; }
        public string Appartment { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Telephone { get; set; }
        public string State { get; set; }
        public List<Invoice> Invoices { get; set; }

        public Client()
        {
            this.Invoices = new List<Invoice>();
        }

        public string GetAddress()
        {
            return string.Join(" ", this.Street, this.StreetNumber, this.Floor, this.Appartment);
        }
    }
    public class Invoice
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public int CreatedById { get; set; }
        public string DocumentNumber { get; set; }
        public int IssuerId { get; set; }
        public int ReceiverId { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime? VoidDate { get; set; }
        public decimal Amount { get; set; }
        public int Status { get; set; }
        public int FinancingStatus { get; set; }
        public string Comment { get; set; }
        public string Data1 { get; set; }
        public string Data2 { get; set; }
        public string Data3 { get; set; }
        public string Data4 { get; set; }
        public int Data5 { get; set; }
    }
}