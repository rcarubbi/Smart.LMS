using System;

namespace SmartLMS.Domain.Entities
{
    public abstract class Entity
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

        public bool Active { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
