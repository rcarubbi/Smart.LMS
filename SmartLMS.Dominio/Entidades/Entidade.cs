using System;

namespace SmartLMS.Dominio.Entidades
{
    public abstract class Entidade
    {
        private Guid _guid;
        public Guid Id
        {
            get
            {
                if (_guid == Guid.Empty)
                    _guid = Guid.NewGuid();
                return _guid;
            }
            set
            {
                _guid = value;
            }
        }

        public bool Ativo { get; set; }

        public DateTime DataCriacao { get; set; }
    }
}
