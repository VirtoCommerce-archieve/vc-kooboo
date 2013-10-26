#region License
// 
// Copyright (c) 2013, Kooboo team
// 
// Licensed under the BSD License
// See the file LICENSE.txt for details.
// 
#endregion

using System.Web.Mvc;
using Kooboo.CMS;
using Kooboo.CMS.Sites.Models;
using Kooboo.CMS.Common.Runtime.Dependency;
using Kooboo.CMS.Sites.Extension.ModuleArea;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo.VirtoCommerce
{
    [Kooboo.CMS.Common.Runtime.Dependency.Dependency(typeof(IModuleEvents), Key = CommerceAreaRegistration.ModuleName)]
    public class ModuleEvents : IModuleEvents
    {
        public void OnExcluded(Site site)
        {
            // Add the logic here when the module was excluded from the site.
        }

        public void OnInstalling(ControllerContext controllerContext)
        {
            // Add code here that will be executed when the module installing.
        }

        public void OnUninstalling(ControllerContext controllerContext)
        {
            // Add code here that will be executed when the module uninstalling.
        }

        public void OnIncluded(Site site)
        {
            // Add the logic here when the module was included to the site.
        }
    }
}
