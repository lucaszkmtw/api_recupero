using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceStack.OrmLite;

using CentralOperativa.Domain.Financials.Controlling;
using CentralOperativa.Domain.Projects;
using ServiceStack;
using Api = CentralOperativa.ServiceModel.Financials.Controlling;

namespace CentralOperativa.ServiceInterface.Financials.Controlling
{
    public class CostCenterRepository
    {
        public async Task<Api.CostCenter> GetCostCenter(IDbConnection db, int id)
        {
            var data = await db.SingleByIdAsync<CostCenter>(id);
            return data.ConvertTo<Api.CostCenter>();
        }

        public async Task<Dictionary<int, Api.CostCenter>> GetCostCenters(IDbConnection db, SqlExpression<ProjectCostCenter> filter)
        {
            var data = await db.SelectAsync(db.From<CostCenter>().Where(w => Sql.In(w.Id , filter)));
            var model = data.ConvertAll(x => x.ConvertTo<Api.CostCenter>());
            return model.ToDictionary(x => x.Id);
        }
    }
}