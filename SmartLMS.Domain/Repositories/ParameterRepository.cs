using System.Linq;
using SmartLMS.Domain.Entities;

namespace SmartLMS.Domain.Repositories
{
    public class ParameterRepository
    {
        private readonly IContext _context;

        public ParameterRepository(IContext context)
        {
            _context = context;
        }

        public string GetValueByKey(string key)
        {
            return _context.GetList<Parameter>().Single(p => p.Key == key).Value;
        }
    }
}