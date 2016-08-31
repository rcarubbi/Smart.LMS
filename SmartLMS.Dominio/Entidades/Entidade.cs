using System;

namespace SmartLMS.Dominio.Entidades
{
    public abstract class Entidade
    {
        public Guid Id { get; set; }

        public bool Ativo { get; set; }

        public DateTime DataCriacao { get; set; }
    }
}
