using SmartLMS.Domain.Entities.Content;

namespace SmartLMS.Domain.Repositories
{
    public class ClassInfo
    {
        public Class Class { get; set; }

        public bool Available { get; set; }

        public bool Editable { get; set; }
        public int Percentual { get; set; }
        public decimal WatchedSeconds { get; set; }
    }
}