using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ServiceStack;
using ServiceStack.Logging;
using ServiceStack.OrmLite;
using Api = CentralOperativa.ServiceModel.Cms.Forms;
using CentralOperativa.ServiceInterface.System.Persons;

namespace CentralOperativa.ServiceInterface.Cms.Forms
{
    public class FormResponseService : ApplicationService
    {
        public static ILog Log = LogManager.GetLogger(typeof(FormResponseService));

        private readonly IAutoQueryDb _autoQuery;
        private readonly PersonRepository _personRepository;
        private readonly FormRepository _formRepository;

        public FormResponseService(
            IAutoQueryDb autoQuery,
            PersonRepository personRepository,
            FormRepository formRepository)
        {
            _autoQuery = autoQuery;
            _personRepository = personRepository;
            _formRepository = formRepository;
        }

        [Authenticate]
        public object Get(Api.GetFormResponses request)
        {
            /*
            var select = new Select();
            select.Fields.Add(new Field("r", "Id", "id"));
            select.Fields.Add(new Field("r", "PersonId", "contactId"));
            select.Fields.Add(new Field("r", "Date", "date"));
            select.Fields.Add(new Field("r", "Guid", "guid"));
            select.Fields.Add(new Field("r", "ClientIp", "clientIp"));
            select.Fields.Add(new Field("r", "StatusId", "statusId"));
            select.Fields.Add(new Field("r", "Fields", "fields"));
            select.From.Add("FormResponse r");
            select.Where.Add("r.FormId =" + request.FormId);
            select.OrderBy.Add("r.Id DESC");
            var query = new JqGridSqlQuery<Model.Forms.FormResponse>(select, Db);
            var data = query.Execute();
            return new FormResponse.GetFormResponsesResponse { Data = data };
            */

            return null;
        }

        [Authenticate]
        public object Get(Api.GetPollResponses request)
        {
            if (request.OrderBy == null)
            {
                request.OrderBy = "Name";
            }
            var p = Request.GetRequestParams();

            if (p.ContainsKey("sidx") && p["sidx"] != "userName")
            {
                if (p.ContainsKey("sord") && p["sord"] == "desc")
                {
                    request.OrderByDesc = p["sidx"];
                    request.OrderBy = null;
                }
                else
                {
                    request.OrderBy = p["sidx"];
                }
            }
            var q = _autoQuery.CreateQuery(request, Request.GetRequestParams());
            q.LeftJoin<Domain.Cms.Forms.FormResponse, Domain.System.User>((fr, u) => fr.CreatedById == u.Id);
            q.UnsafeSelect(@"FormResponses.{0} AS Id,
                            Persons.Name as Name,
                            Users.Name as UserName,
                            FormResponses.StartDate as StartDate,
                            FormResponses.EndDate as EndDate,
                            FormResponses.StatusId as StatusId
                        ".Fmt("Id".SqlColumn()));

            if (p.ContainsKey("userName"))
            {
                q.UnsafeWhere("Users.Name LIKE {0}", Utils.SqlLike(p["userName"]));
            }
            if (p.ContainsKey("sidx") && p["sidx"] == "userName")
            {
                q.UnsafeOrderBy("Users.Name " + p["sord"]);
            }

            return _autoQuery.Execute(request, q);
        }

        [Authenticate]
        public object Get(Api.GetPersonPolls request)
        {
            var session = SessionAs<Infraestructure.Session>();

            if (request.OrderBy == null)
            {
                request.OrderBy = "Name";
            }
            var p = Request.GetRequestParams();

            if (p.ContainsKey("sidx") && p["sidx"] != "userName")
            {
                if (p.ContainsKey("sord") && p["sord"] == "desc")
                {
                    request.OrderByDesc = p["sidx"];
                    request.OrderBy = null;
                }
                else
                {
                    request.OrderBy = p["sidx"];
                }
            }
            var q = _autoQuery.CreateQuery(request, Request.GetRequestParams());
            q.LeftJoin<Domain.Cms.Forms.FormResponse, Domain.System.User>((fr, u) => fr.CreatedById == u.Id);
            q.UnsafeSelect(@"FormResponses.{0} AS Id,
                            Forms.Name as Name,
                            Users.Name as UserName,
                            FormResponses.StartDate as StartDate,
                            FormResponses.EndDate as EndDate,
                            FormResponses.StatusId as StatusId,
                            Forms.Id as FormId,    
                            Forms.AllowUpdates as AllowUpdates
                        ".Fmt("Id".SqlColumn()));

            q.Where<Domain.Cms.Forms.Form>(x => x.TenantId == session.TenantId && x.TypeId == 1);

            return _autoQuery.Execute(request, q);
        }

        [Authenticate]
        public async Task<object> Get(Api.GetFormResponse request)
        {
            var pollFormResponseRequest = new Api.GetFormResponse
            {
                FormResponse = (await Db.SelectAsync(Db.From<Domain.Cms.Forms.FormResponse>().Where(fr => fr.Id == request.FormResponseId))).SingleOrDefault()
            };
            if (pollFormResponseRequest.FormResponse != null)
            {
                pollFormResponseRequest.Form = (await Db.SelectAsync(Db.From<Domain.Cms.Forms.Form>().Where(f => f.Id == pollFormResponseRequest.FormResponse.FormId))).SingleOrDefault();
                if (pollFormResponseRequest.FormResponse.PersonId != null)
                {
                    var personId = (int)pollFormResponseRequest.FormResponse.PersonId;
                    pollFormResponseRequest.Person = await _personRepository.GetPerson(Db, personId);
                    
                }
            }
            return pollFormResponseRequest;
        }

        [Authenticate]
        public object Get(Api.GetFormResponseByForm request)
        {
            if (request.Id != 0)
            {
                var result = Db.SingleById<Domain.Cms.Forms.FormResponse>(request.Id);
                return result; //.ConvertTo<FormResponse.GetFormResponseResponse>();
            }

            if (!request.Guid.Equals(Guid.Empty))
            {
                var result = Db.Select<Domain.Cms.Forms.FormResponse>(x => x.Guid == request.Guid).SingleOrDefault();
                return result; //.ConvertTo<FormResponse.GetFormResponseResponse>();
            }

            if (request.FormId != 0 && request.ContactId != 0)
            {
                var result = Db.Select<Domain.Cms.Forms.FormResponse>(x => x.FormId == request.FormId && x.PersonId == request.ContactId).FirstOrDefault();
                return result; //.ConvertTo<FormResponse.GetFormResponseResponse>();
            }

            return null;
        }

        [Authenticate]
        public async Task<object> Put(Api.PostFormResponse request)
        {
            var formResponse = new Domain.Cms.Forms.FormResponse
            {
                Id = request.Id,
                ClientIp = Request.RemoteIp,
                PersonId = request.PersonId,
                FormId = request.FormId,
                Guid = Guid.NewGuid(),
                StatusId = request.StatusId,
                Fields = request.Fields
            };

            var _formResponse = await Db.SingleByIdAsync<Domain.Cms.Forms.FormResponse>(request.Id);
            formResponse.StartDate = _formResponse.StartDate;
            formResponse.CreatedById = _formResponse.CreatedById;
            formResponse.EndDate = _formResponse.EndDate;

            if (_formResponse.StatusId == 0 && formResponse.StatusId == 1)
            {
                formResponse.EndDate = DateTime.UtcNow;
            }

            await Db.SaveAsync(formResponse);
            
            /*
            if (form.AllowUpdates && request.PersonId.HasValue)
            {
                var contactService = ResolveService<ContactService>();
                var contact = contactService.GetTenant(new GetContact {Id = request.PersonId.Value});
                
                var nameData = new ContactNameData(contact.GetData(ContactMimeTypes.Name))
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName
                };
                Db.Save(nameData.Data);

                var emailData = contact.GetDatas(ContactMimeTypes.Email).FirstOrDefault(x => x.Data2 == "1");
                if (emailData != null)
                {
                    emailData.Data1 = request.Email;
                }
                Db.Save(emailData);

                var phoneData = contact.GetDatas(ContactMimeTypes.Phone).FirstOrDefault(x => x.Data2 == "1");
                if (phoneData != null)
                {
                    phoneData.Data1 = request.Phone;
                }
                Db.Save(phoneData);

                var postalAddressData = contact.GetDatas(ContactMimeTypes.PostalAddress).FirstOrDefault(x => x.Data2 == "1");
                if (postalAddressData != null)
                {
                    postalAddressData.Data1 = request.PostalAddress;
                }
                Db.Save(postalAddressData);


                var key = CacheUtils.KeyFor<Core.Model.Profile.Contact>(request.PersonId.Value);
                this.Cache.Remove(key);
            }
            */

            /*
            var model = new FormResponse.PostFormResponseResponse
            {
                Response = formResponse.ConvertTo<ServiceModel.Cms.Forms.FormResponse>(),
                Success = true
            };
            */

            return formResponse;
        }

        public async Task<Domain.Cms.Forms.FormResponse> Post(Api.PostFormResponse request)
        {
            Domain.Cms.Forms.FormResponse formResponse;
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    int userId;
                    if (!Session.IsAuthenticated)
                    {
                        userId = 2057; //Anonymous user Id
                    }
                    else
                    {
                        userId = Session.UserId;
                    }

                    var form = await _formRepository.GetForm(Db, request.FormId);
                    /*byte formResponseStatus = 0; //0 Confirmado //ListadeEspera
                    if (form.Quota > 0 && form.Responses >= form.Quota)
                    {
                        formResponseStatus = 1;
                    }*/
                    var formResponseStatus = request.StatusId;
                    form.Responses = form.Responses + 1;
                    await _formRepository.UpdateForm(Db, form.ConvertTo<Api.PostForm>());

                    if (request.Person != null && (!string.IsNullOrEmpty(request.Person.LastName) || !string.IsNullOrEmpty(request.Person.Name)))
                    {
                        request.Person = (await _personRepository.CreatePerson(Db, request.Person)).ConvertTo<ServiceModel.System.Persons.PostPerson>();
                        request.PersonId = request.Person.Id;

                        //Employer
                        if (request.Employer != null && !string.IsNullOrEmpty(request.Employer.LastName))
                        {
                            request.Employer = (await _personRepository.CreatePerson(Db, request.Employer)).ConvertTo<ServiceModel.System.Persons.PostPerson>();
                            if (request.Employee != null)
                            {
                                var employee = new Domain.CRM.Contacts.Employee
                                {
                                    PersonId = request.Person.Id,
                                    Salary = request.Employee.Salary,
                                    CreatedById = userId,
                                    CreatedDate = DateTime.Now,
                                    FromDate = DateTime.Now,
                                    ToDate = DateTime.Now,
                                    EmployerId = request.Employer.Id
                                };

                                request.Employee.Id = (int) await Db.InsertAsync(employee, true);
                            }
                        }
                    }

                    var currentRequest = HostContext.TryGetCurrentRequest();
                    formResponse = new Domain.Cms.Forms.FormResponse
                    {
                        ClientIp = currentRequest?.UserHostAddress,
                        PersonId = request.PersonId,
                        FormId = request.FormId,
                        Guid = Guid.NewGuid(),
                        StatusId = formResponseStatus,
                        Fields = request.Fields,
                        StartDate = DateTime.UtcNow,
                        CreatedById = userId
                    };

                    if (formResponse.StatusId == 1)
                    {
                        formResponse.EndDate = DateTime.UtcNow;
                    }

                    await Db.SaveAsync(formResponse);
                    if (request.LeadId.HasValue)
                    {
                        var leadFormResponse = new Domain.CRM.LeadFormResponse
                        {
                            LeadId = (int)request.LeadId,
                            FormResponseId = formResponse.Id
                        };
                        await Db.InsertAsync(leadFormResponse);
                    }

                    trx.Commit();

                    //if (form.ValidateUser && request.PersonId.HasValue)
                    //{
                    //    var contactService = this.ResolveService<ContactService>();
                    //    var contact = contactService.GetTenant(new GetContact { Id = request.PersonId.Value });

                    //    var tokens = new List<Token>
                    //    {
                    //        new Token("Form.Name", form.Name),
                    //        new Token("Form.Contact", form.Contact),
                    //        new Token("Form.Description", form.Description),
                    //        new Token("Form.FinalMessage", form.FinalMessage),
                    //        new Token("Form.Place", form.Place),
                    //        new Token("Form.ReceiptFooter", form.ReceiptFooter),
                    //        new Token("Form.Date", form.Date.HasValue ? form.Date.ToString() : string.Empty)
                    //    };

                    //    var toEmail = contact.GetData(ContactMimeTypes.Email).Data1;
                    //    var toName = contact.GetData(ContactMimeTypes.Name).Data1;

                    //    var workflowMessageService = this.ResolveService<WorkflowMessageService>();
                    //    workflowMessageService.SendNotification("Forms.NewResponseNotification", 2, tokens, toEmail, toName);
                    //}
                }
                catch (Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }

            //Notifications TODO: Va a estar parametrizado
            if (request.FormId == 1035) //Energias Sustentables Region Sudoeste
            {
                var fromAddress = "convocatoriaenergias.mcti@gba.gob.ar";
                var toAddresses = new List<string>();
                var bccAddresses = new List<string> { "pcejas@gmail.com", "sebastian.vigliola@gmail.com" };
                var to = request.Person.Emails.FirstOrDefault()?.Address;
                if (!string.IsNullOrEmpty(to))
                {
                    var emailTemplate = await Db.LoadSingleByIdAsync<Domain.System.Notifications.EmailTemplate>(2);
                    if (emailTemplate == null)
                    {
                        throw new ApplicationException("Notification template missing");
                    }

                    toAddresses.Add(to);
                    var task = new ServiceModel.System.Notifications.MailingTask
                    {
                        From = new ServiceModel.System.Notifications.EmailAddress
                        {
                            Name = fromAddress,
                            Address = fromAddress
                        },
                        Subject = emailTemplate.Subject,
                        Template = emailTemplate.Body,
                        To = toAddresses.Select(x =>
                            new ServiceModel.System.Notifications.EmailAddress {Name = x, Address = x}).ToList(),
                        Bcc = bccAddresses.Select(x =>
                            new ServiceModel.System.Notifications.EmailAddress {Name = x, Address = x}).ToList(),
                        UseSES = true
                    };
                    await System.Notifications.NotificationService.SendMail(task);
                }
            }

            if (request.FormId == 1041) //Convocatoria Región Centro
            {
                var fromAddress = "plataformainnovacion.mcti@gba.gob.ar";
                var toAddresses = new List<string>();
                var bccAddresses = new List<string> { "pcejas@gmail.com", "sebastian.vigliola@gmail.com", "clara.carbonetti@gmail.com", "j.canievsky@gmail.com" };
                //var bccAddresses = new List<string> { "pcejas@gmail.com" };
                var to = request.Person.Emails.FirstOrDefault()?.Address;
                if (!string.IsNullOrEmpty(to))
                {
                    var emailTemplate = Db.LoadSingleById<Domain.System.Notifications.EmailTemplate>(3);
                    if (emailTemplate == null)
                    {
                        throw new ApplicationException("Notification template missing");
                    }

                    toAddresses.Add(to);

                    var task = new ServiceModel.System.Notifications.MailingTask
                    {
                        From = new ServiceModel.System.Notifications.EmailAddress
                        {
                            Name = fromAddress,
                            Address = fromAddress
                        },
                        Subject = emailTemplate.Subject,
                        Template = emailTemplate.Body,
                        To = toAddresses.Select(x =>
                            new ServiceModel.System.Notifications.EmailAddress {Name = x, Address = x}).ToList(),
                        Bcc = bccAddresses.Select(x =>
                            new ServiceModel.System.Notifications.EmailAddress {Name = x, Address = x}).ToList(),
                        UseSES = true
                    };
                    await System.Notifications.NotificationService.SendMail(task);
                }
            }

            if (request.FormId == 1045) // Presolicitud Widex
            {
                var fromName = "Tiempo de Descuento";
                var fromAddress = "info@tiempodedescuento.com.ar";
                var toAddresses = new List<string>();
                var bccAddresses = new List<string> { "pcejas@gmail.com", "sebastian.vigliola@gmail.com" };
                var to = request.Person.Emails.FirstOrDefault()?.Address;
                if (!string.IsNullOrEmpty(to))
                {
                    var emailTemplate = await Db.LoadSingleByIdAsync<Domain.System.Notifications.EmailTemplate>(4);
                    if (emailTemplate == null)
                    {
                        throw new ApplicationException("Notification template missing");
                    }

                    toAddresses.Add(to);

                    var task = new ServiceModel.System.Notifications.MailingTask
                    {
                        From = new ServiceModel.System.Notifications.EmailAddress
                        {
                            Name = fromName,
                            Address = fromAddress
                        },
                        Subject = emailTemplate.Subject,
                        Template = emailTemplate.Body,
                        To = toAddresses.Select(x => new ServiceModel.System.Notifications.EmailAddress {Name = x, Address = x}).ToList(),
                        Bcc = bccAddresses.Select(x => new ServiceModel.System.Notifications.EmailAddress {Name = x, Address = x}).ToList()
                    };
                    await System.Notifications.NotificationService.SendMail(task);
                }

                fromName = "Tiempo de Descuento";
                fromAddress = "info@tiempodedescuento.com.ar";
                toAddresses = new List<string>();
                to = "online@tiempodedescuento.com.ar";
                if (!string.IsNullOrEmpty(to))
                {
                    var emailTemplate = Db.LoadSingleById<Domain.System.Notifications.EmailTemplate>(4);
                    if (emailTemplate == null)
                    {
                        throw new ApplicationException("Notification template missing");
                    }

                    toAddresses.Add(to);

                    var task = new ServiceModel.System.Notifications.MailingTask
                    {
                        From = new ServiceModel.System.Notifications.EmailAddress
                        {
                            Name = fromName,
                            Address = fromAddress
                        },
                        Subject = emailTemplate.Subject,
                        Template = emailTemplate.Body,
                        To = toAddresses.Select(x => new ServiceModel.System.Notifications.EmailAddress {Name = x, Address = x}).ToList(),
                        Bcc = bccAddresses.Select(x => new ServiceModel.System.Notifications.EmailAddress {Name = x, Address = x}).ToList()
                    };
                    await System.Notifications.NotificationService.SendMail(task);
                }
            }

            return formResponse;
        }

        [Authenticate]
        public async Task Delete(Api.DeleteFormResponse request)
        {
            await Db.DeleteByIdAsync<Domain.Cms.Forms.FormResponse>(request.Id);
        }
    }
}
