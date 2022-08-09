using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ServiceStack;
using ServiceStack.Logging;
using ServiceStack.OrmLite;

using CentralOperativa.Domain.BusinessDocuments;

using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceInterface.BusinessPartners;
using CentralOperativa.ServiceInterface.System.Workflows;
using CentralOperativa.Domain.BusinessPartners;

using CentralOperativa.ServiceInterface.System;
using CentralOperativa.ServiceInterface.System.Persons;
using Api = CentralOperativa.ServiceModel.BusinessDocuments;
using CentralOperativa.ServiceInterface.System.DocumentManagement;
using ApiBusinessDocumentService = CentralOperativa.ServiceInterface.BusinessDocuments.BusinessDocumentService;
using CentralOperativa.Domain.System.Workflows;

namespace CentralOperativa.ServiceInterface.BusinessDocuments
{
    [Authenticate]
    public class BusinessDocumentMigrationService : ApplicationService
    {
        public static ILog Log = LogManager.GetLogger(typeof(BusinessDocumentService));
        private readonly IAutoQueryDb _autoQuery;
        private readonly TenantRepository _tenantRepository;
        private readonly PersonRepository _personRepository;
        private readonly WorkflowActivityRepository _workflowActivityRepository;
        private readonly WorkflowInstanceRepository _workflowInstanceRepository;
        private readonly BusinessPartnerRepository _businessPartnerRepository;
        private readonly FileRepository _fileRepository;

        public BusinessDocumentMigrationService(
            IAutoQueryDb autoQuery,
            TenantRepository tenantRepository,
            PersonRepository personRepository,
            WorkflowActivityRepository workflowActivityRepository,
            WorkflowInstanceRepository workflowInstanceRepository,
            BusinessPartnerRepository businessPartnerRepository,
            FileRepository fileRepository)
        {
            _autoQuery = autoQuery;
            _tenantRepository = tenantRepository;
            _personRepository = personRepository;
            _workflowActivityRepository = workflowActivityRepository;
            _workflowInstanceRepository = workflowInstanceRepository;
            _businessPartnerRepository = businessPartnerRepository;
            _fileRepository = fileRepository;
        }

        public class FieldJsonModel
        {
            public int id;
            public String name;
            public String type;
            public List<String> list;
        }

        public async Task<object> Post(Api.PostBusinessDocumentMigration request)
        {
            var product = Db.SingleById<Domain.Catalog.Product>(1);
            var records = Db.Select(Db.From<BusinessDocumentMigration>());
            var issuerId = 55305;
            var receiverId = 55305;
            var categoryId = 13;
            var productId = 1;
            var unittypeId = 1;
            decimal quantity = 1;
            var debtorTypeId = 6;
            var creditorId = 11;


            foreach (var record in records)
            {
                var businessDocument = Db.Select(Db.From<BusinessDocument>().Where(x => x.Number == record.NUMDEEXPEDIENTE)).FirstOrDefault();
                if (businessDocument == null)
                {
                    try
                    {
                        businessDocument = new BusinessDocument();
                        businessDocument.TypeId = 25;
                        businessDocument.IssuerId = issuerId;
                        businessDocument.ReceiverId = receiverId;
                        businessDocument.Guid = new Guid();
                        businessDocument.Status = BusinessDocumentStatus.PendingApproval;
                        businessDocument.CreatedBy = Session.UserId;
                        businessDocument.CreateDate = DateTime.Now;
                        businessDocument.DocumentDate = record.INGRESODPGYRCF;
                        businessDocument.Number = record.NUMDEEXPEDIENTE;
                        businessDocument.Total = record.MONTOORIGINAL;
                        if (record.FECHADEPRESCRIPCION != null)
                        {
                            businessDocument.CAEVoidDate = record.FECHADEPRESCRIPCION;
                        }
                        if (record.FECHADENOTIFICACION != null)
                        {
                            businessDocument.NotificationDate = record.FECHADENOTIFICACION;
                        }

                        businessDocument.CategoryId = categoryId;
                        businessDocument.Id = (int)Db.Insert(businessDocument, true);

                        BusinessDocumentItem businessDocumentItem = new BusinessDocumentItem();
                        businessDocumentItem.BusinessDocumentId = businessDocument.Id;
                        businessDocumentItem.ProductId = productId;
                        businessDocumentItem.UnitTypeId = unittypeId;
                        businessDocumentItem.UnitPrice = record.MONTOORIGINAL;
                        businessDocumentItem.Quantity = quantity;
                        businessDocumentItem.Bonus = 0;
                        businessDocumentItem.VatRate = 0;
                        businessDocumentItem.VoidDate = record.FECHADEPRESCRIPCION;
                        businessDocumentItem.NotificationDate = record.FECHADENOTIFICACION;

                        FieldJsonModel resultJson = new FieldJsonModel();
                        resultJson.id = 1;
                        resultJson.name = "Comentario";
                        resultJson.type = "text";
                        resultJson.list = new List<string>();
                        var jsonRslt = Newtonsoft.Json.JsonConvert.SerializeObject(resultJson);

                        businessDocumentItem.FieldsJSON = jsonRslt;
                        businessDocumentItem.Id = (int)Db.Insert(businessDocumentItem, true);

                        var debtorPerson = Db.Select(Db.From<Domain.System.Persons.Person>().Where(x => x.Name == record.NOMBRE)).FirstOrDefault();
                        if (debtorPerson == null)
                        {
                            debtorPerson = new Domain.System.Persons.Person();
                            debtorPerson.Name = record.NOMBRE;
                            debtorPerson.Id = (int)Db.Insert(debtorPerson, true);
                        }

                        var debtor = Db.Select(Db.From<Domain.Financials.DebtManagement.Debtor>().Where(x => x.PersonId == debtorPerson.Id && x.DebtorTypeId == debtorTypeId)).FirstOrDefault();
                        if (debtor == null)
                        {
                            BusinessPartner businessPartner = new BusinessPartner();
                            businessPartner.TypeId = 8;
                            businessPartner.TenantId = 1;
                            businessPartner.PersonId = debtorPerson.Id;
                            businessPartner.Code = debtorPerson.Id.ToString();
                            businessPartner.CreatedById = 3088;
                            businessPartner.CreateDate = DateTime.Now;
                            businessPartner.Guid = new Guid();
                            businessPartner.Status = 0;
                            businessPartner.Id = (int)Db.Insert(businessPartner, true);

                            BusinessPartnerAccount businessPartnerAccount = new BusinessPartnerAccount();
                            businessPartnerAccount.BusinessPartnerId = businessPartner.Id;
                            businessPartnerAccount.CreateDate = DateTime.UtcNow;
                            businessPartnerAccount.CurrencyId = 1;
                            businessPartnerAccount.Guid = Guid.NewGuid();
                            businessPartnerAccount.CreatedById = Session.UserId;
                            businessPartnerAccount.Type = 0;
                            businessPartnerAccount.Name = "";
                            businessPartnerAccount.Code = "";
                            businessPartnerAccount.Id = (int)Db.Insert(businessPartnerAccount, true);

                            debtor = new Domain.Financials.DebtManagement.Debtor();
                            debtor.PersonId = debtorPerson.Id;
                            debtor.DebtorTypeId = debtorTypeId;
                            debtor.Status = 0;
                            debtor.BusinessPartnerId = businessPartner.Id;
                            debtor.Id = (int)Db.Insert(debtor, true);
                        }
                        BusinessDocumentItemDebtor businessDocumentItemDebtor = new BusinessDocumentItemDebtor();
                        businessDocumentItemDebtor.BusinessDocumentItemId = businessDocumentItem.Id;
                        businessDocumentItemDebtor.DebtorId = debtor.Id;
                        businessDocumentItemDebtor.Id = (int)await Db.InsertAsync(businessDocumentItemDebtor, true);

                        BusinessDocumentItemCreditor businessDocumentItemCreditor = new BusinessDocumentItemCreditor();
                        businessDocumentItemCreditor.BusinessDocumentItemId = businessDocumentItem.Id;
                        businessDocumentItemCreditor.CreditorId = creditorId;
                        businessDocumentItemCreditor.Id = (int)await Db.InsertAsync(businessDocumentItemCreditor, true);

                        Api.PostBusinessDocumentSubmitForCollect postBusinessDocumentSubmitForCollect = new Api.PostBusinessDocumentSubmitForCollect();
                        postBusinessDocumentSubmitForCollect.BusinessDocumentGuid = businessDocument.Guid;
                        

                    }
                    catch
                    {

                    }

                }
            }
            return true;
        }

        public async Task<bool> GenerateWF(int Id)
        {
            var businessDocument = await Db.SingleAsync<BusinessDocument>(w => w.Id == Id);
            if (businessDocument.ApprovalWorkflowInstanceId == null)
            {
                try
                {
                    var businessDocumentType = await Db.SingleByIdAsync<BusinessDocumentType>(businessDocument.TypeId);
                    // Tomo de workflowTypeId el que está asociado al businessdocumentType, en caso de que no lo tenga defaulteo al de Cobranza expedientes.
                    var workflowTypeId = businessDocumentType.ApprovalWorkflowTypeId ?? (short)WellKnownWorkflowTypes.Collection;
                    var currentActivity = await _workflowActivityRepository.GetWorkflowActivity(Db, Session, 0, workflowTypeId);
                    var workflowInstance = new WorkflowInstance
                    {
                        CreateDate = DateTime.UtcNow,
                        CreatedByUserId = Session.UserId,
                        WorkflowId = currentActivity.WorkflowId,
                        CurrentWorkflowActivityId = currentActivity.Id,
                        Guid = Guid.NewGuid()
                    };
                    workflowInstance.Id = (int)await Db.InsertAsync(workflowInstance, true);

                    foreach (var approvalRule in currentActivity.ApprovalRules)
                    {
                        var workflowInstanceApproval = new WorkflowInstanceApproval
                        {
                            WorkflowInstanceId = workflowInstance.Id,
                            RoleId = approvalRule.RoleId,
                            UserId = approvalRule.UserId,
                            WorkflowActivityId = approvalRule.WorkflowActivityId,
                            Status = WorkflowInstanceApprovalStatus.Pending,
                            CreateDate = DateTime.UtcNow
                        };
                        await Db.InsertAsync(workflowInstanceApproval);
                    }

                    //Current activity roles
                    var activityRoles = await Db.SelectAsync<WorkflowActivityRole>(w => w.WorkflowActivityId == currentActivity.Id);

                    //Assign workflowinstance roles
                    foreach (var activityRole in activityRoles.Where(w => w.IsDefault))
                    {
                        var workflowInstanceRole = new WorkflowInstanceAssignments
                        {
                            WorkflowInstanceId = workflowInstance.Id,
                            WorkflowActivityId = workflowInstance.CurrentWorkflowActivityId,
                            RoleId = activityRole.RoleId,
                            UserId = Session.UserId,
                            CreateDate = DateTime.UtcNow,
                            IsActive = true
                        };
                        await Db.InsertAsync(workflowInstanceRole);
                    }

                    businessDocument.ApprovalWorkflowInstanceId = workflowInstance.Id;

                    switch (workflowTypeId)
                    {
                        case (short)WellKnownWorkflowTypes.InventoryDispatch:
                            businessDocument.Status = BusinessDocumentStatus.InProcess;
                            break;
                        case (short)WellKnownWorkflowTypes.InventoryReceipt:
                            businessDocument.Status = BusinessDocumentStatus.Control;
                            break;
                        default:
                            businessDocument.Status = BusinessDocumentStatus.PendingApproval;
                            break;
                    }

                    await Db.UpdateAsync(businessDocument);
                    return true;
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return false;
        }

    
        //public async Task<bool> Post(Api.PostBusinessDocumentMigrationActivate request)
        //{
        //    //var businessDocuments = Db.Select(Db.From<BusinessDocument>());
        //    //foreach (var businessDocument in businessDocuments)
        //    //{
        //    //    var result = GenerateWF(businessDocuments.GetId);
        //    //}
        //    //return true;
        //}
    }
}