using System;
using System.Linq;
using Carubbi.GenericRepository;
using SmartLMS.Domain.Services;

namespace SmartLMS.WebUI.Models
{
    public class AccessViewModel
    {
        public int Percentual { get; set; }

        public Guid ContentId { get; set; }

        public AccessType AccessType { get; set; }

        public string Name { get; private set; }

        public string CourseName { get; private set; }

        public string DateTimeDescription { get; private set; }

        public string ClassName { get; set; }

        public static PagedListResult<AccessViewModel> FromEntityList(PagedListResult<AccessInfo> accesses)
        {
            return new PagedListResult<AccessViewModel>
            {
                HasNext = accesses.HasNext,
                HasPrevious = accesses.HasPrevious,
                Count = accesses.Count,
                Entities = accesses.Entities.Select(FromEntity).ToList()
            };
        }

        private static AccessViewModel FromEntity(AccessInfo item)
        {
            var access = new AccessViewModel
            {
                AccessType = item.AccessType,
                DateTimeDescription = item.DateTimeDescription
            };

            if (item.AccessType == AccessType.File)
            {
                access.ContentId = item.FileAccess.File.Id;
                access.Name = item.FileAccess.File.Name;
                access.ClassName = item.FileAccess.File.Class.Name;
                access.CourseName = item.FileAccess.File.Class.Course.Name;
                access.Percentual = 100;
            }
            else
            {
                access.ContentId = item.ClassAccess.Class.Id;
                access.Name = item.ClassAccess.Class.Name;
                access.CourseName = item.ClassAccess.Class.Course.Name;
                access.Percentual = item.ClassAccess.Percentual;
            }

            return access;
        }
    }
}