using SmartLMS.Domain.Entities;
using System.Linq;

namespace SmartLMS.Domain.Repositories
{
    public class ParameterRepository
    {
        private IContext _context;
        public ParameterRepository(IContext context)
        {
            _context = context;
        }

        public string ObterValorPorChave(string key)
        {
            return _context.GetList<Parameter>().Single(p => p.Key == key).Value;
        }

    
    }
}
