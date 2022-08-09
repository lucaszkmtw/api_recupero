using System.Data;
using System.Linq;
using CentralOperativa.Domain.System;
using ServiceStack.OrmLite;

namespace CentralOperativa.ServiceInterface.System
{
    public static class ImportLogRepository
    {
        public static ImportLog LoadImportLog(this IDbConnection db, ImportLog source)
        {
            var item = db.Select(db
                    .From<ImportLog>()
                    .Where(w => w.SourceId == source.SourceId && w.ImporterId == source.ImporterId && w.TypeId == source.TypeId))
                .SingleOrDefault();
            return item;
        }
    }
}