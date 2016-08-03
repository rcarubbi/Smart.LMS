using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Dominio.Entidades
{
   public interface IResultadoBusca
    {
        Guid Id { get; }
             
        bool Ativo { get;  }
        string Nome { get;  }
    }
}
