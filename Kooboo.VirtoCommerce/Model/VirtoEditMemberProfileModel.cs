using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kooboo.CMS.Sites.Membership;

namespace Kooboo.VirtoCommerce.Model
{
    public class VirtoEditMemberProfileModel : EditMemberProfileModel
    {
        public virtual string RedirectUrl { get; set; }
    }
}
