using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Kooboo.VirtoCommerce
{
    public class UnityKoobooResolver : IDependencyResolver
    {
        private readonly IDependencyResolver _unity;
        private readonly IDependencyResolver _kooboo;
        public UnityKoobooResolver(IDependencyResolver unity,IDependencyResolver kooboo)
        {
            _unity = unity;
            _kooboo = kooboo;
        }
        public object GetService(Type serviceType)
        {
            var result = _kooboo.GetService(serviceType);
            return result ?? _unity.GetService(serviceType);
        }
        public IEnumerable<object> GetServices(Type serviceType)
        {
            var result = _kooboo.GetServices(serviceType);
            return result ?? _unity.GetServices(serviceType);
        }
    }
}
