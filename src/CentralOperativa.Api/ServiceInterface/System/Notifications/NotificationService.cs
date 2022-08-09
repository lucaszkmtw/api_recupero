using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ServiceStack;
using Microsoft.WindowsAzure.Storage.Table;

using Notification = CentralOperativa.ServiceModel.System.Notifications.Notification;

namespace CentralOperativa.ServiceInterface.System.Notifications
{
    [Authenticate]
    public class NotificationService : ApplicationService
    {
        public async Task<object> Get(Notification.GetTest request)
        {
            const string subject = "Notification test";
            const string body = "This email was sent through Centraloperativa.";

            var toAddresses = new List<string>
            {
                "pcejas@gmail.com"
            };

            var task = new ServiceModel.System.Notifications.MailingTask();
            task.From = new ServiceModel.System.Notifications.EmailAddress { Name = "ArventGroup", Address = "arvent-noreply@arventgroup.com" };
            task.Subject = subject;
            task.Template = body;
            task.To = toAddresses.Select(x => new ServiceModel.System.Notifications.EmailAddress { Name = x, Address = x }).ToList();
            task.Campaign = "Test";
            return await SendMail(task);
        }

        public async Task<object> Post(Notification.PostSendNotification request)
        {
            var task = new ServiceModel.System.Notifications.MailingTask();
            task.From = new ServiceModel.System.Notifications.EmailAddress { Name = request.FromName, Address = request.FromAddress };
            task.Subject = request.Subject;
            task.Template = request.Body;
            task.To = request.ToAddresses.Select(x => new ServiceModel.System.Notifications.EmailAddress { Name = x, Address = x }).ToList();
            return await SendMail(task);
        }

        public static async Task<object> SendMail(ServiceModel.System.Notifications.MailingTask task)
        {
            if (task.UseSES)
            {
                throw new NotImplementedException("Port to SparkPost");
                /*
                var destination = new Amazon.SimpleEmail.Model.Destination();
                destination.ToAddresses = task.To.Select(x => x.Address).ToList();
                destination.BccAddresses = task.Bcc.Select(x => x.Address).ToList();

                var subjectContent = new Amazon.SimpleEmail.Model.Content(task.Subject);
                var bodyContent = new Amazon.SimpleEmail.Model.Content(task.Template);
                var messageBody = new Amazon.SimpleEmail.Model.Body { Html = bodyContent };
                var message = new Amazon.SimpleEmail.Model.Message(subjectContent, messageBody);
                var emailRequest = new Amazon.SimpleEmail.Model.SendEmailRequest(task.From.Address, destination, message);
                var region = Amazon.RegionEndpoint.USEast1;
                var client = new Amazon.SimpleEmail.AmazonSimpleEmailServiceClient(region);
                return await client.SendEmailAsync(emailRequest);
                */
            }
            else
            {
                string key = null;
                if (task.From.Address.EndsWith("tiempodedescuento.com.ar"))
                {
                    key = ""; // tiempodedescuento.com.ar
                }

                if (task.From.Address.EndsWith("rapicobros.com"))
                {
                    key = ""; // rapicobros.com.ar
                }

                if (key == null)
                {
                    key = ""; // rapicobros.com.ar
                    task.From.Address = "info@rapicobros.com";
                }


                //var cloudTable = Infraestructure.AzureHelper.FindOrCreateTable("mailingtasks");
                var sparkPostClient = new SparkPost.Client(key);
                sparkPostClient.CustomSettings.SendingMode = SparkPost.SendingModes.Async;

                var trans = new SparkPost.Transmission();
                trans.CampaignId = task.Campaign;
                trans.Content.From.Name = task.From.Name;
                trans.Content.From.Email = task.From.Address;
                if (task.ReplyTo.Any())
                {
                    trans.ReturnPath = task.ReplyTo.First()?.Address;
                }

                foreach (var address in task.To)
                {
                    trans.Recipients.Add(new SparkPost.Recipient { Address = new SparkPost.Address(address.Address, address.Name), Type = SparkPost.RecipientType.To });
                }

                trans.Content.Subject = task.Subject;
                trans.Content.Html = task.Template;

                try
                {
                    Console.Write("Sending mail...");
                    var response = await sparkPostClient.Transmissions.Send(trans);
                    //var sentMessage = new SentMessageEntity(client.Id, formattedAddresses, DateTime.UtcNow, response.Id, true);
                    //var insertOperation = TableOperation.Insert(sentMessage);
                    //cloudTable.Execute(insertOperation);
                    return response;
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.Message);
                    //Console.Error.WriteLine("{0}: {1}", client.Id, formattedAddresses);
                    return null;
                }
            }
        }
    }

    public class SentMessageEntity : TableEntity
    {
        public SentMessageEntity(int clientId, string addresses, DateTime sentOn, string messageId, bool success)
        {
            this.ClientId = clientId;
            this.Addresses = addresses;
            this.SentOn = sentOn;
            this.MessageId = messageId;
            this.Success = success;

            this.PartitionKey = clientId.ToString();
            this.RowKey = messageId;
        }

        public int ClientId { get; set; }
        public string Addresses { get; set; }
        public DateTime SentOn { get; set; }
        public string MessageId { get; set; }
        public bool Success { get; set; }
    }
}
