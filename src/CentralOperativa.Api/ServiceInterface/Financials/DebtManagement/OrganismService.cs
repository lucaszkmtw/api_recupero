using System.Linq;
using CentralOperativa.Infraestructure;
using CentralOperativa.Domain.System;
using ServiceStack;
using ServiceStack.OrmLite;
using CentralOperativa.Domain.Financials.DebtManagement;
using Api = CentralOperativa.ServiceModel.Financials.DebtManagement;
using CentralOperativa.ServiceModel.System.Persons;
using CentralOperativa.Domain.BusinessPartners;

using System;


namespace CentralOperativa.ServiceInterface.Financials.Debtmanagement
{
    [Authenticate]
    public class OrganismService : ApplicationService
    {
        public object Put(Api.PostOrganism request)
        {  
            //tipo de organismo es distinto al tipo de la pantalla
            var organism = Db.SingleById<Organism>(request.Id);
            if (organism.TypeId != request.TypeId)
            {
                var organismType = Db.SingleById<OrganismType>(request.TypeId);
                if (organismType != null) //si lo encontro tipo de organismo
                {
                    var bpartnerType = Db.Single<CentralOperativa.Domain.BusinessPartners.BusinessPartnerType>(w => w.Name == organismType.Code);
                    if (bpartnerType != null) // si lo encontro en BPT
                    {
                        //verifica que el deudor existe BP
                        var query = Db.From<BusinessPartner>()
                            .Where(w => w.TenantId == Session.TenantId && w.TypeId == bpartnerType.Id && w.PersonId == request.PersonId);

                        var businessPartner = Db.Select(query).SingleOrDefault();
                        //si no existe => crea "uno nuevo" con el nuevo tipo
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

                            //inserto Organism
                            request.BusinessPartnerId = bpartnerId;
                            request.Status = (int)BusinessPartnerStatus.Active;
                            request.Id = (int)Db.Insert((Organism)request, true);

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

                            //doy de baja Organism y BP
                            var businessPartnerToDelete = Db.SingleById<BusinessPartner>(organism.BusinessPartnerId);
                            if (businessPartnerToDelete != null)
                            {
                                //desactiva BP
                                businessPartnerToDelete.Status = BusinessPartnerStatus.Deleted;
                                Db.Update((BusinessPartner)businessPartnerToDelete);

                                //desactiva Organism
                                organism.Status = (int)BusinessPartnerStatus.Deleted;
                                Db.Update((Organism)organism);
                            }
                        }
                        else //si existe => desactiva y activa
                        {
                            //doy de baja el que estaba BP y Organism
                            var businessPartnerToDelete = Db.SingleById<BusinessPartner>(organism.BusinessPartnerId);
                            if (businessPartnerToDelete != null)
                            {
                                businessPartnerToDelete.Status = BusinessPartnerStatus.Deleted;
                                Db.Update((BusinessPartner)businessPartnerToDelete);

                                organism.Status = (int)BusinessPartnerStatus.Deleted;
                                Db.Update((Organism)organism);

                                businessPartner.Status = BusinessPartnerStatus.Active;
                                Db.Update((BusinessPartner)businessPartner);

                                var organismActive = Db.Single<Organism>(w => w.BusinessPartnerId == businessPartner.Id);
                                organismActive.Status = (int)BusinessPartnerStatus.Active;
                                Db.Update((Organism)organismActive);
                            }

                        }

                    }
                }
            }
            else
            {
                Db.Update((Organism)request);         
            }
            return request;
                  
        }

        public object Post(Api.PostOrganism request)
        {

            CentralOperativa.Domain.BusinessPartners.BusinessPartnerType bpartnerType;

            var organismType = Db.SingleById<OrganismType>(request.TypeId);
            if (organismType != null)
            {
                bpartnerType = Db.Single<CentralOperativa.Domain.BusinessPartners.BusinessPartnerType>(w => w.Id == organismType.BusinessPartnerTypeId);
                if (bpartnerType != null) // si existe mismo tipo tipo 
                {
                    short OrganismTypeId = bpartnerType.Id;
                    BusinessPartner bpartner = new BusinessPartner();
                    bpartner.TenantId = Session.TenantId;
                    bpartner.CreatedById = Session.UserId;
                    bpartner.Code = "1";
                    bpartner.TypeId = OrganismTypeId;
                    bpartner.CreateDate = DateTime.UtcNow;
                    bpartner.Guid = Guid.NewGuid();
                    bpartner.PersonId = request.PersonId;

                    var query = Db.From<BusinessPartner>()
                        .Where(w => w.TenantId == Session.TenantId && w.TypeId == OrganismTypeId && w.PersonId == request.PersonId);

                    var businessPartner = Db.Select(query).SingleOrDefault();
                    if (businessPartner == null) // si no existe el organismo => inserta Organism, BP, BPA
                    {
                        int bpartnerId = (int)Db.Insert((BusinessPartner)bpartner, true);
                        request.BusinessPartnerId = bpartnerId;
                        request.Id = (int)Db.Insert((Organism)request, true);


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

                    }
                    else
                    {
                        if (businessPartner.Status == BusinessPartnerStatus.Deleted)
                        {
                            businessPartner.Status = BusinessPartnerStatus.Active;
                            Db.Update((BusinessPartner)businessPartner);

                            var organism = Db.Single<Organism>(w => w.BusinessPartnerId == businessPartner.Id);
                            organism.Status = (int)BusinessPartnerStatus.Active;
                            organism.Code = request.Code;
                            Db.Update((Organism)organism);
                        }
                    }

                }
            }
            return request;
        }

        public object Delete(Api.DeleteOrganism request)
        {
            var organism = Db.SingleById<Organism>(request.Id);

            if (organism != null)
            {
                var businesPartner = Db.SingleById<BusinessPartner>(organism.BusinessPartnerId);
                if (businesPartner != null)
                {
                    organism.Status = (int)BusinessPartnerStatus.Deleted;
                    Db.Update((Organism)organism);
                    businesPartner.Status = BusinessPartnerStatus.Deleted;
                    Db.Update((BusinessPartner)businesPartner);
                }


            }

            return request;


        }

        public IAutoQueryDb AutoQuery { get; set; }

        public object Any(Api.QueryOrganisms request)
        {

            var query = Db.From<Organism>()    
                    .Join<Organism, OrganismType>()
                    .Join<Organism, Domain.System.Persons.Person>()
                    .OrderByDescending(q => q.Id)
                    .Limit(request.Skip.GetValueOrDefault(0), request.Take.GetValueOrDefault(10));

            return Db.Select(query);    
        }

        public object Get(Api.GetOrganism request)
        {
            var organism = Db.SingleById<Organism>(request.Id);
            var model = organism.ConvertTo<Api.OrganismInfo>();

            var bPartnerAccount = Db.Select(Db.From<BusinessPartnerAccount>().Where(bpa => bpa.BusinessPartnerId == model.BusinessPartnerId)).FirstOrDefault();
            if (bPartnerAccount != null)
            {
                model.AccountId = bPartnerAccount.Id;
            }


            return model;
        }

        public QueryResponse<Api.QueryOrganismResult> Get(Api.QueryOrganisms request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            q.Where<Organism>(w => w.Status == (int)BusinessPartnerStatus.Active);
            var Data  = AutoQuery.Execute(request, q);
            foreach (var data in Data.Results)
            {
                var businessPartner = Db.Select(Db.From<BusinessPartner>().Where(bp => bp.PersonId == data.PersonId
                                                && bp.TypeId == 9)).SingleOrDefault();
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

        public object Get(Api.LookupOrganism request)
        {


			var query = Db.From<Organism>()
						.Join<Organism, Domain.System.Persons.Person>((o,p) => o.PersonId == p.Id)
						.Join<Organism, OrganismType>((o,ot) => o.TypeId == ot.Id)
						.Join<OrganismType, CentralOperativa.Domain.BusinessPartners.BusinessPartnerType>((ot,bpt) => ot.BusinessPartnerTypeId == bpt.Id);
						


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
                query.Where<Domain.System.Persons.Person>(p => p.Name.Contains(request.Q) || p.Code.Contains(request.Q));
            }

            query.Where(x => x.Status == (int)BusinessPartnerStatus.Active);


            if (request.BusinessPartnerTypeName != null)
            {
				query.Where<CentralOperativa.Domain.BusinessPartners.BusinessPartnerType>(w => w.Name == request.BusinessPartnerTypeName);
            }

			var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));


            var result = new LookupResult
            {
                Data = Db.Select<Api.GetOrganismResult>(query).Select(x => new LookupItem { Id = x.Id, Text = x.PersonName }),
                Total = (int)count
            };
            return result;
        }


    }
}