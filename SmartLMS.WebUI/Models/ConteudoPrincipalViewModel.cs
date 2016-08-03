using System.Collections.Generic;

namespace SmartLMS.WebUI.Models
{
    public class ConteudoPrincipalViewModel
    {
        public IEnumerable<AreaConhecimentoViewModel> AreasConhecimento { get; set; }

        public string TituloUltimosCursos { get; set; }

        public string TituloAulasAssistidas { get; set; }

        public string TituloUltimasAulas { get; set; }
    }
}
