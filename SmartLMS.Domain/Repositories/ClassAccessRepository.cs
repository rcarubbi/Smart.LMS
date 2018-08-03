using SmartLMS.Domain.Entities.History;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartLMS.Domain.Repositories
{
    public class ClassAccessRepository
    {
        private readonly IContext _context;

        public ClassAccessRepository(IContext context)
        {
            _context = context;
        }

        public ClassAccess GetLongestAccess(Guid classId, Guid studentId)
        {
            var longestAccess = _context.GetList<ClassAccess>()
                    .Where(x => x.Class.Id == classId && x.User.Id == studentId)
                    .OrderByDescending(x => x.Percentual)
                    .FirstOrDefault();

            return longestAccess;
        }

        public void UpdateProgress(ClassAccess access)
        {
            var accesses = _context.GetList<ClassAccess>();
            var existingAccess = accesses
                .Where(x => x.Class.Id == access.Class.Id && x.User.Id == access.User.Id)
                .OrderByDescending(x=> x.AccessDateTime)
                .FirstOrDefault();
               
            if (existingAccess == null)
            {
                accesses.Add(access);
            }
            else
            {
                access.Id = existingAccess.Id;
                _context.Update(existingAccess, access);
            }

            _context.Save();
        }

        public void CreateAccess(ClassAccess classAccess)
        {
            var accesses = _context.GetList<ClassAccess>();
            accesses.Add(classAccess);
            _context.Save();
        }

        public IEnumerable<ClassAccess> ListLastAccesses(Guid id)
        {
            var query = from access in _context.GetList<ClassAccess>()
            where access.User.Id == id
            group access by access.Class.Id
            into accessByClass
            select accessByClass.OrderByDescending(x => x.AccessDateTime).FirstOrDefault();

            return query.ToList();
        }
    }
}
