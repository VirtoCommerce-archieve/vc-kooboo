using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Specialized;
using VirtoCommerce.Web.Helpers;
using System.Text.RegularExpressions;
using VirtoCommerce.Web.Client.Extensions;

namespace VirtoCommerce.Web.Models.Binders
{
    public class SearchParametersBinder : IModelBinder
    {
        public const int DefaultPageSize = SearchParameters.DefaultPageSize;

        public IDictionary<string, string> NvToDict(NameValueCollection nv)
        {
            var d = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var k in nv.AllKeys)
                d[k] = nv[k];
            return d;
        }

        private static readonly Regex FacetRegex = new Regex("^f_", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var qs = controllerContext.HttpContext.Request.Params;
            var qsDict = NvToDict(qs);
            var sp = new SearchParameters
            {
                FreeSearch = qs["q"].EmptyToNull(),
                PageIndex = qs["p"].TryParse(1),
                PageSize = qs["pageSize"].TryParse(DefaultPageSize),
                Sort = qs["sort"].EmptyToNull(),
                Facets = qsDict.Where(k => FacetRegex.IsMatch(k.Key))
                    .Select(k => k.WithKey(FacetRegex.Replace(k.Key, "")))
                    .ToDictionary()
            };
            return sp;
        }
    }
}