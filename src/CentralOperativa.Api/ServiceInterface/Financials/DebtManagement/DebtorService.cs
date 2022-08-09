using System.Linq;
using CentralOperativa.Infraestructure;
using CentralOperativa.Domain.System.Persons;
using ServiceStack;
using ServiceStack.OrmLite;
using CentralOperativa.Domain.Financials.DebtManagement;
using CentralOperativa.ServiceModel.System.Persons;
using Api = CentralOperativa.ServiceModel.Financials.DebtManagement;
using CentralOperativa.Domain.BusinessPartners;
using System;

namespace CentralOperativa.ServiceInterface.Financials.Debtmanagement

{
    [Authenticate]
	public class DebtorService : ApplicationService
	{
		public object Put(Api.PostDebtor request)
		{
			var debtor = Db.SingleById<Debtor>(request.Id);
			if (debtor.DebtorTypeId != request.DebtorTypeId)
			{
				var DebtorType = Db.SingleById<DebtorType>(request.DebtorTypeId);
				if (DebtorType != null)
				{
					var bpartnerType = Db.Single<CentralOperativa.Domain.BusinessPartners.BusinessPartnerType>(w => w.Id == DebtorType.BusinessPartnerTypeId);
					if (bpartnerType != null)
					{

						var query = Db.From<BusinessPartner>()
							.Where(w => w.TenantId == Session.TenantId && w.TypeId == bpartnerType.Id && w.PersonId == request.PersonId);

						var businessPartner = Db.Select(query).SingleOrDefault();
						if (businessPartner == null)
						{
							//inserto BP
							BusinessPartner bpartner = new BusinessPartner();
							bpartner.TenantId = Session.TenantId;
							bpartner.CreatedById = Session.UserId;
							bpartner.Code = "1";
							bpartner.TypeId = bpartnerType.Id;
							bpartner.CreateDate = DateTime.UtcNow;
							bpartner.Guid = Guid.NewGuid();
							bpartner.PersonId = request.PersonId;
							bpartner.Status = BusinessPartnerStatus.Active;

							int bpartnerId = (int)Db.Insert((BusinessPartner)bpartner, true);

							//inserto Debtor
							request.BusinessPartnerId = bpartnerId;
							request.Status = (int)BusinessPartnerStatus.Active;
							request.Id = (int)Db.Insert((Debtor)request, true);

							//insertar BPA
							BusinessPartnerAccount bpaccount = new BusinessPartnerAccount();
							bpaccount.BusinessPartnerId = bpartnerId;
							bpaccount.CreateDate = DateTime.UtcNow;
							bpaccount.CurrencyId = 1;
							bpaccount.Guid = Guid.NewGuid();
							bpaccount.CreatedById = Session.UserId;
							bpaccount.Type = 0;
							bpaccount.Name = "";
							bpaccount.Code = "";

							int partnerAccountId = (int)Db.Insert((BusinessPartnerAccount)bpaccount, true);

							//doy de baja Debtor y BP
							var businessPartnerToDelete = Db.SingleById<BusinessPartner>(debtor.BusinessPartnerId);
							if (businessPartnerToDelete != null)
							{
								businessPartnerToDelete.Status = BusinessPartnerStatus.Deleted;
								Db.Update((BusinessPartner)businessPartnerToDelete);

								debtor.Status = (int)BusinessPartnerStatus.Deleted;
								Db.Update((Debtor)debtor);
							}
						}
						else
						{
							//doy de baja el que estaba BP y Debtor
							var businessPartnerToDelete = Db.SingleById<BusinessPartner>(debtor.BusinessPartnerId);
							if (businessPartnerToDelete != null)
							{
								businessPartnerToDelete.Status = BusinessPartnerStatus.Deleted;
								Db.Update((BusinessPartner)businessPartnerToDelete);

								debtor.Status = (int)BusinessPartnerStatus.Deleted;
								Db.Update((Debtor)debtor);

								
								businessPartner.Status = BusinessPartnerStatus.Active;
								Db.Update((BusinessPartner)businessPartner);

								var debtorActive = Db.Single<Debtor>(w => w.BusinessPartnerId == businessPartner.Id);

								if (debtorActive != null)
								{

									debtorActive.Status = (int)BusinessPartnerStatus.Active;
									Db.Update((Debtor)debtorActive);
								}
								

								

							}

						}

					}
				}
			}
			return request;
		}

		public object Post(Api.PostDebtor request)
		{
			{
				CentralOperativa.Domain.BusinessPartners.BusinessPartnerType bpartnerType;
				var DebtorType = Db.SingleById<DebtorType>(request.DebtorTypeId);
				if (DebtorType != null)
				{
					bpartnerType = Db.Single<CentralOperativa.Domain.BusinessPartners.BusinessPartnerType>(w => w.Id == DebtorType.BusinessPartnerTypeId);
					if (bpartnerType != null)
					{
						short DebtorTypeId = bpartnerType.Id;
						BusinessPartner bpartner = new BusinessPartner();
						bpartner.TenantId = Session.TenantId;
						bpartner.CreatedById = Session.UserId;
						bpartner.Code = "1";
						bpartner.TypeId = DebtorTypeId;
						bpartner.CreateDate = DateTime.UtcNow;
						bpartner.Guid = Guid.NewGuid();
						bpartner.PersonId = request.PersonId;

						var query = Db.From<BusinessPartner>()
							.Where(w => w.TenantId == Session.TenantId && w.TypeId == DebtorTypeId && w.PersonId == request.PersonId);

						var businessPartner = Db.Select(query).SingleOrDefault();
						if (businessPartner == null)
						{
							int bpartnerId = (int)Db.Insert((BusinessPartner)bpartner, true);
							request.BusinessPartnerId = bpartnerId;
							request.Id = (int)Db.Insert((Debtor)request, true);


							BusinessPartnerAccount bpaccount = new BusinessPartnerAccount();
							bpaccount.BusinessPartnerId = bpartnerId;
							bpaccount.CreateDate = DateTime.UtcNow;
							bpaccount.CurrencyId = 1;
							bpaccount.Guid = Guid.NewGuid();
							bpaccount.CreatedById = Session.UserId;
							bpaccount.Type = 0;
							bpaccount.Name = "CTA DEUDOR";
							bpaccount.Code = "CTA DEUDOR";

							int partnerAccountId = (int)Db.Insert((BusinessPartnerAccount)bpaccount, true);

						}
						else
						{
							if (businessPartner.Status == BusinessPartnerStatus.Deleted)
							{
								businessPartner.Status = BusinessPartnerStatus.Active;
								Db.Update((BusinessPartner)businessPartner);

								var debtor = Db.Single<Debtor>(w => w.BusinessPartnerId == businessPartner.Id);
								debtor.Status = (int)BusinessPartnerStatus.Active;
								Db.Update((Debtor)debtor);
							}
						}
						
					}
				}
					
				}
			return request;

		}

		public IAutoQueryDb AutoQuery { get; set; }


		public object Delete(Api.DeleteDebtor request)
		{
			var debtor = Db.SingleById<Debtor>(request.Id);

			if (debtor != null) {
				var businesPartner = Db.SingleById<BusinessPartner>(debtor.BusinessPartnerId);
				if (businesPartner != null)
				{
					debtor.Status = (int)BusinessPartnerStatus.Deleted; 
					Db.Update((Debtor)debtor);
					businesPartner.Status = BusinessPartnerStatus.Deleted;
					Db.Update((BusinessPartner)businesPartner);
				}
			}
				
			return request;
		}


		public object Any(Api.QueryDebtors request)
		{

			var query = Db.From<Debtor>()
					.Join<Debtor, DebtorType>()
					.Join<Debtor, Domain.System.Persons.Person>()
					.OrderByDescending(q => q.Id)
					.Limit(request.Skip.GetValueOrDefault(0), request.Take.GetValueOrDefault(10));

			return Db.Select(query);
		}

		public object Get(Api.GetDebtor request)
		{
			var debtor = Db.SingleById<Debtor>(request.Id);
			var model = debtor.ConvertTo<Api.Debtor>();
			return model;
		}
		

		public QueryResponse<Api.QueryDebtorResult> Get(Api.QueryDebtors request)
		{
			if (request.OrderByDesc == null)
			{
				request.OrderByDesc = "Id";
			}

			var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
			q.Where<Debtor>(w => w.Status == (int)BusinessPartnerStatus.Active);
            var Data = AutoQuery.Execute(request, q);
            foreach (var data in Data.Results)
            {
                var businessPartner = Db.Select(Db.From<BusinessPartner>().Where(bp => bp.PersonId == data.PersonId
                                                && bp.TypeId == 8)).SingleOrDefault();
                if (businessPartner != null)
                {
                    var businessPartnerAccount = Db.Select(Db.From<BusinessPartnerAccount>().Where(bpa => bpa.BusinessPartnerId == businessPartner.Id)).SingleOrDefault();
                    if (businessPartnerAccount != null)
                    {
                        data.AccountId = businessPartnerAccount.Id;
                    }
                }
            }
            return Data;
        }

		public object Get(Api.LookupDebtor request)
			/*lookup de deudores*/
		{
			var query = Db.From<Debtor>()
						.Join<Debtor, Domain.System.Persons.Person>();


			if (request.Id.HasValue)
			{
				query.Where(x => x.Id == request.Id.Value);
			}
			else if (request.Ids != null)
			{
				query.Where(x => Sql.In(x.Id, request.Ids));
			}

            if (!string.IsNullOrEmpty(request.Q))
            {
                query.Where<Domain.System.Persons.Person>(x => x.Name.Contains(request.Q) || x.Code.Contains(request.Q));
            }

            var count = Db.Count(query);

			query = query.OrderByDescending(q => q.Id)
				.Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));


			var result = new LookupResult
			{
				Data = Db.Select<Api.GetDebtorResult>(query).Select(x => new LookupItem { Id = x.Id, Text = x.PersonName }),
				Total = (int)count
			};
			return result;
		}
	}
}


