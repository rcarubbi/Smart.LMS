using System;

namespace SmartLMS.Dominio.Entidades
{
    public interface IResultadoBusca
    {
        Guid Id { get; }
             
        bool Ativo { get;  }
        string Nome { get;  }
    }
}
