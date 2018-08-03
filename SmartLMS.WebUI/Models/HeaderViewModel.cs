using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.WebUI.Models
{
    public class HeaderViewModel
    {
        public UserViewModel User { get; set; }

        public IEnumerable<KnowledgeAreaViewModel> KnowledgeAreas { get; set; }

         

    }
}
