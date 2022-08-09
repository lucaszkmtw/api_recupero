using System;
using System.Data;
using System.Threading.Tasks;
using CentralOperativa.Domain.BusinessPartners;
using CentralOperativa.Infraestructure;
using ServiceStack.OrmLite;
using Api = CentralOperativa.ServiceModel.BusinessPartners;

namespace CentralOperativa.ServiceInterface.BusinessPartners
{
    public class BusinessPartnerAccountRepository
    {
        public async Task<BusinessPartnerAccount> InsertBusinessPartnerAccount(IDbConnection db, Session session, BusinessPartnerAccount businessParnterAccount)
        {
            //Code
            var query = $"SELECT MAX(CAST(Code AS NUMERIC)) Number FROM BusinessPartnerAccounts WHERE BusinessPartnerId = {businessParnterAccount.Id} AND ISNUMERIC(Code) = 1";
            var currentNumber = await db.ScalarAsync<int>(query);
            var code = (currentNumber + 1).ToString();

            businessParnterAccount.Code = code;
            businessParnterAccount.CreateDate = DateTime.UtcNow;
            businessParnterAccount.CreatedById = session.UserId;
            businessParnterAccount.Guid = Guid.NewGuid();
            businessParnterAccount.Id = (int)await db.InsertAsync(businessParnterAccount, true);
            return businessParnterAccount;
        }

        public async Task<Api.PostBusinessPartner> UpdateBusinessPartner(IDbConnection db, Api.PostBusinessPartner businessPartner)
        {
            await db.UpdateAsync((BusinessPartner)businessPartner);
            return businessPartner;
        }
    }
}


//using System;
//using System.Data;
//using System.Threading.Tasks;
//using CentralOperativa.Domain.BusinessPartners;
//using CentralOperativa.Infraestructure;
//using ServiceStack.OrmLite;
//using Api = CentralOperativa.ServiceModel.BusinessPartners;

//namespace CentralOperativa.ServiceInterface.BusinessPartners
//{
//    public class BusinessPartnerAccountRepository
//    {
//        public async Task<BusinessPartnerAccount> InsertBusinessPartnerAccount(IDbConnection db, Session session, BusinessPartnerAccount businessParnterAccount)
//        {
//            //Code
//            var query = $"SELECT MAX(CAST(Code AS NUMERIC)) Number FROM BusinessPartnerAccounts WHERE BusinessPartnerId = {businessParnterAccount.Id} AND ISNUMERIC(Code) = 1";
//            var currentNumber = await db.ScalarAsync<int>(query);
//            var code = (currentNumber + 1).ToString();

//            businessParnterAccount.Code = code;
//            businessParnterAccount.CreateDate = DateTime.UtcNow;
//            businessParnterAccount.CreatedById = session.UserId;
//            businessParnterAccount.Guid = Guid.NewGuid();
//            businessParnterAccount.Id = (int) await db.InsertAsync(businessParnterAccount, true);
//            return businessParnterAccount;
//        }

//        public async Task<Api.PostBusinessPartner> UpdateBusinessPartner(IDbConnection db, Api.PostBusinessPartner businessPartner)
//        {
//            await db.UpdateAsync((BusinessPartner)businessPartner);
//            return businessPartner;
//        }
//    }
//}