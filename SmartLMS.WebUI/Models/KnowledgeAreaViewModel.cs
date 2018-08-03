using System;
using System.Collections.Generic;
using System.Linq;
using Carubbi.GenericRepository;
using System.ComponentModel.DataAnnotations;
using SmartLMS.Domain.Entities.Content;

namespace SmartLMS.WebUI.Models
{
    public class KnowledgeAreaViewModel
    {
        [Display(Name = "Created at")]

        public DateTime CreationDate { get; set; }

        [Display(Name = "Active")]
        public bool Active { get; set; }

        [Display(Name = "Order")]
        [Required]
        public int Order { get; set; }


        public IEnumerable<SubjectViewModel> Subjects { get; set; }


        public Guid Id { get; set; }

        [Display(Name = "Name")]
        [Required]
        public string Name { get; set; }

        internal static IEnumerable<KnowledgeAreaViewModel> FromEntityList(IEnumerable<KnowledgeArea> knowledgeAreas, int depth)
        {
            return knowledgeAreas.Select(item => FromEntity(item, depth));
        }

        public static KnowledgeAreaViewModel FromEntity(KnowledgeArea knowledgeArea, int depth)
        {
            return new KnowledgeAreaViewModel
            {
                Active = knowledgeArea.Active,
                Order = knowledgeArea.Order,
                Name = knowledgeArea.Name,
                Id = knowledgeArea.Id,
                CreationDate = knowledgeArea.CreatedAt,
                Subjects = depth > 0
                ? SubjectViewModel.FromEntityList(knowledgeArea.Subjects.Where(x =>x.Active).OrderBy(x => x.Order), depth)
                : new List<SubjectViewModel>()
            };
        }

        internal static PagedListResult<KnowledgeAreaViewModel> FromEntityList(PagedListResult<KnowledgeArea> knowledgeAreas)
        {
            return new PagedListResult<KnowledgeAreaViewModel>
            {
                HasNext = knowledgeAreas.HasNext,
                HasPrevious = knowledgeAreas.HasPrevious,
                Count = knowledgeAreas.Count,
                Entities = knowledgeAreas.Entities.Select(item => FromEntity(item, 0)).ToList()
            };
        }

        internal static KnowledgeArea ToEntity(KnowledgeAreaViewModel knowledgeArea)
        {
            return new KnowledgeArea
            {
                Active = knowledgeArea.Active,
                CreatedAt = knowledgeArea.CreationDate,
                Name = knowledgeArea.Name,
                Order = knowledgeArea.Order,
                Id = knowledgeArea.Id,
            };
        }
    }
}
