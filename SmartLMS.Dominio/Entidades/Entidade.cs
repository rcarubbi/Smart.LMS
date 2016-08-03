using System;

namespace SmartLMS.Dominio
{
    public abstract class Entidade
    {
        public Guid Id { get; set; }

        public bool Ativo { get; set; }
    }
}
