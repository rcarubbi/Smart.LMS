using System.Collections.Generic;

namespace SmartLMS.WebUI.Models
{
    public class HeaderViewModel
    {
        public UserViewModel User { get; set; }

        public IEnumerable<KnowledgeAreaViewModel> KnowledgeAreas { get; set; }
    }
}