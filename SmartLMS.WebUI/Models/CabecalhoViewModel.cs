using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.WebUI.Models
{
    public class CabecalhoViewModel
    {
        public UsuarioViewModel Usuario { get; set; }

        public IEnumerable<AreaConhecimentoViewModel> AreasConhecimento { get; set; }

         

    }
}
