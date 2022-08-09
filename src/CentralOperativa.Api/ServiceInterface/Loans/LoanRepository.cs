using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CentralOperativa.Domain.Loans;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceInterface.System.Persons;
using CentralOperativa.ServiceInterface.System.Workflows;
using ServiceStack;
using ServiceStack.OrmLite;
using Api = CentralOperativa.ServiceModel.Loans;

namespace CentralOperativa.ServiceInterface.Loans
{
    [Authenticate]
    public class LoanRepository
    {
        private readonly PersonRepository _personRepository;
        private readonly WorkflowInstanceRepository _workflowInstanceRepository;

        public LoanRepository(
            PersonRepository personRepository,
            WorkflowInstanceRepository workflowInstanceRepository)
        {
            _personRepository = personRepository;
            _workflowInstanceRepository = workflowInstanceRepository;
        }

        public async Task<Api.GetLoanResult> GetLoan(IDbConnection db, Session session, int id, bool? includeItems = false)
        {
            var model = (await db.SelectAsync<Api.GetLoanResult>(
                db.From<Loan>()
                .Join<Loan, Domain.Catalog.Product>()
                .Where(w => w.Id == id))).FirstOrDefault();
            if (model == null)
            {
                throw HttpError.NotFound($"The loan with id {id} was not found.");
            }

            // AuthorizationWorkflowInstance
            if (model.AuthorizationWorkflowInstanceId.HasValue)
            {
                model.AuthorizationWorkflowInstance = await _workflowInstanceRepository.GetWorkflowInstance(db, session, model.AuthorizationWorkflowInstanceId.Value);
            }

            // Persons
            model.Persons = await db.SelectAsync<Api.GetLoanPersonResult>(db.From<LoanPerson>().Where(x => x.LoanId == id));
            foreach (var person in model.Persons)
            {
                person.Person = await _personRepository.GetPerson(db, person.PersonId);
            }

            var firstInstallment = (await db.SelectAsync(db.From<LoanInstallment>().Where(w => w.LoanId == id && w.Number == 1))).FirstOrDefault();
            if (firstInstallment != null)
            {
                model.InstallmentFirstVoidDate = firstInstallment.VoidDate;
            }

            // Items
            if (includeItems.HasValue && includeItems.Value)
            {
                model.Items = await db.SelectAsync<Api.GetLoanItemResult>(db.From<LoanItem>().Where(w => w.LoanId == model.Id));
                var concepts = (await db.SelectAsync(db.From<LoanConcept>().Where(w => Sql.In(w.Id, model.Items.Select(x => x.ConceptId))))).ToDictionary(x => x.Id, x => x);
                var itemIds = model.Items.Select(x => x.Id).ToList();
                var itemDistributions = await db.SelectAsync(db.From<LoanItemDistribution>().Where(w => Sql.In(w.LoanItemId, itemIds)));
                var itemDistributionIds = itemDistributions.Select(x => x.Id).ToList();
                var paymentDocumentItems = await db.SelectAsync(db.From<Domain.Financials.PaymentDocumentItem>().Where(w => w.LinkedDocumentTypeId == 5 && Sql.In(w.LinkedDocumentId, itemDistributionIds)));
                var paymentDocumentIds = paymentDocumentItems.Select(x => x.PaymentDocumentId).Distinct().ToList();
                var paymentDocuments = await db.SelectAsync(db.From<Domain.Financials.PaymentDocument>().Where(w => Sql.In(w.Id, paymentDocumentIds)));
                model.Items.ForEach(x =>
                {
                    x.Concept = concepts[x.ConceptId];
                    itemDistributions.Where(w => w.LoanItemId == x.Id).ToList().ForEach(itemDistribution =>
                    {
                        var itemDistributionModel = itemDistribution.ConvertTo<Api.GetLoanItemResult.GetLoanItemDistributionResult>();
                        itemDistributionModel.PaymentDocumentItem = paymentDocumentItems.Where(w => w.LinkedDocumentId == itemDistributionModel.Id).SingleOrDefault()?.ConvertTo<ServiceModel.Financials.GetPaymentDocumentItemResponse>();
                        if (itemDistributionModel.PaymentDocumentItem != null)
                        {
                            itemDistributionModel.PaymentDocumentItem.PaymentDocument = paymentDocuments.SingleOrDefault(w => w.Id == itemDistributionModel.PaymentDocumentItem.PaymentDocumentId);
                        }
                        x.Distributions.Add(itemDistributionModel);
                    });
                });

                model.Installments = await db.SelectAsync<Api.LoanInstallment.QueryLoanInstallmentResult>(db.From<LoanInstallment>().Where(w => w.LoanId == model.Id));
            }

            return model;
        }

        public async Task<Api.GetLoanResult> GetLoan(IDbConnection db, Session session, Guid guid, bool? includeItems = false)
        {
            var loan = (await db.SelectAsync<Api.GetLoanResult>(db.From<Loan>().Where(w => w.Guid == guid && w.TenantId == session.TenantId))).SingleOrDefault();
            return loan == null ? null : await GetLoan(db, session, loan.Id, includeItems);
        }

        public async Task<Api.GetLoanResult> GetLoan(IDbConnection db, Session session, string number, bool? includeItems = false)
        {
            var loan = (await db.SelectAsync<Api.GetLoanResult>(db.From<Loan>().Where(w => w.Number == number && w.TenantId == session.TenantId))).SingleOrDefault();
            return loan == null ? null : await GetLoan(db, session, loan.Id, includeItems);
        }

        public void CalculateLoanInstallments(IDbConnection db, int loanId, decimal baseAmount, decimal loanCapital, DateTime initialVoidDate, int term)
        {
            var balance = loanCapital;
            var voidDate = initialVoidDate;

            var size = term + 1;
            var cf = new double[size];
            cf[0] = (double)(loanCapital * -1);
            for (var i = 1; i < size; i++)
            {
                cf[i] = (double)baseAmount;
            }

            var tir = Excel.FinancialFunctions.Financial.Irr(cf, 0.1);
            var tm = tir / 1.21;

            for (var i = 0; i < term; i++)
            {
                var interests = balance * (decimal)tm;
                var taxes = interests * 21 / 100;
                var capital = baseAmount - interests - taxes;
                balance = balance - capital;

                var item = new LoanInstallment
                {
                    LoanId = loanId,
                    VoidDate = voidDate,
                    Number = (short)(i + 1),
                    Amount = baseAmount,
                    StateId = 0,
                    Interests = interests,
                    Taxes = taxes,
                    Capital = capital,
                    Balance = balance
                };

                db.Insert(item);
                voidDate = voidDate.AddMonths(1);
            }
        }

        public void RecalculateInstallmentsVoidDate(IDbConnection db, int loanId, DateTime voidDate)
        {
            var installments = db.Select(
                db
                .From<LoanInstallment>()
                .Where(w => w.LoanId == loanId)
                .OrderBy(w => w.Number)
                );

            installments.ForEach(x =>
            {
                x.VoidDate = voidDate;
                db.Update(x);
                voidDate = voidDate.AddMonths(1);
            });
        }
    }
}