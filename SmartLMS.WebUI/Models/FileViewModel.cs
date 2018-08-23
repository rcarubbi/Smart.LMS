using System;
using System.Collections.Generic;
using System.Linq;
using SmartLMS.Domain.Entities.Content;

namespace SmartLMS.WebUI.Models
{
    public class FileViewModel
    {
        public Guid Id { get; set; }

        public string PhysicalPath { get; private set; }

        public string Name { get; private set; }

        public static IEnumerable<FileViewModel> FromEntityList(IEnumerable<File> files)
        {
            return files.Select(FromEntity);
        }

        private static FileViewModel FromEntity(File item)
        {
            return new FileViewModel
            {
                Id = item.Id,
                Name = item.Name,
                PhysicalPath = item.PhysicalPath
            };
        }
    }
}