using System.Linq;
using System.Collections.Generic;
using ServiceStack;
using ServiceStack.OrmLite;
using Api = CentralOperativa.ServiceModel.System;

namespace CentralOperativa.ServiceInterface.System
{
    [Authenticate(ApplyTo.Post)]
    public class LocalizationServices : Service
    {
        private Dictionary<string, int> Languages
        {
            get
            {
                return new Dictionary<string, int> { { "en", 1 }, { "es", 2 } };
            }

        }
        public IAutoQueryDb AutoQuery { get; set; }


        public object Get(Api.GetLocalizationResources request)
        {
            if (string.IsNullOrEmpty(request.Lang))
            {
                request.Lang = "es";
            }
            var lang = request.Lang.Replace(".json", string.Empty);
            var cacheKey = UrnId.CreateWithParts<Domain.System.LocalizationResource>("s", lang);
            var result = base.Request.ToOptimizedResultUsingCache(base.Cache, cacheKey, () =>
           {
               if (!string.IsNullOrEmpty(lang))
               {
                   var query = Db.From<Domain.System.LocalizationResource>()
                   .Select(x => new { x.Name, x.Value })
                   .OrderByDescending(q => q.Id)
                   .Where(q => q.LanguageId == Languages[lang]);
                   var queryResult = Db.Dictionary<string, string>(query);
                   return queryResult;
               }
               return null;
           });

            return result;
        }

        public Api.GetReloadLocalizationResourcesResponse Get(Api.GetReloadLocalizationResources request)
        {
            var response = new Api.GetReloadLocalizationResourcesResponse();
            var cacheKey = UrnId.CreateWithParts<Domain.System.LocalizationResource>("s");
            var keys = Cache.GetKeysStartingWith(cacheKey).ToArray();
            Request.RemoveFromCache(Cache, keys);
            response.Success = true;
            return response;
            //var response = new Api.GetReloadLocalizationResourcesResponse();
            //var cacheKey = UrnId.CreateWithParts<Domain.System.LocalizationResource>("s");
            //var keys = Cache.GetKeysStartingWith(cacheKey).ToList();
            //foreach (var key in keys)
            //{

            //    Request.RemoveFromCache(Cache, key);
            //}

            //response.Success = true;
            //return response;
        }

        public object Get(Api.QueryLocalizationResources request)
        {
            var query = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            return AutoQuery.Execute(request, query);
        }


        public object Get(Api.GetLocalizationResource request)
        {
            return Db.SingleById<Domain.System.LocalizationResource>(request.Id);
        }


        public object Post(Api.PostLocalizationResource request)
        {
            //verificar si ya existe
            var permCheck = Db.Exists<Domain.System.LocalizationResource>(P => P.Name == request.Name);
            if (!permCheck)
            {
                var validPerm = request.ConvertTo<Domain.System.LocalizationResource>();
                validPerm.Id = (int)Db.Insert(validPerm, true);
                return validPerm;

            }
            else
            {
                return new HttpError(global::System.Net.HttpStatusCode.NotModified, "304");
            }
        }


        public object Put(Api.PutLocalizationResource request)
        {
            var validPerm = request.ConvertTo<Domain.System.LocalizationResource>();
            return Db.Update(validPerm);
        }

        public object Delete(Api.DeleteLozalizationResource request)
        {
            return Db.DeleteById<Domain.System.LocalizationResource>(request.Id);
        }
    }
}
