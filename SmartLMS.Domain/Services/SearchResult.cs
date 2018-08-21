using System;
using System.Data.Entity.Core.Objects;
using System.Linq;
using SmartLMS.Domain.Entities;
using SmartLMS.Domain.Entities.History;
using SmartLMS.Domain.Entities.UserAccess;
using SmartLMS.Domain.Repositories;
using SmartLMS.Domain.Resources;

namespace SmartLMS.Domain.Services
{
    public class SearchResult
    {
        public string Icon
        {
            get
            {
                switch (ResultType)
                {
                    case ResultType.KnowledgeArea:
                        return "fa-map-signs";
                    case ResultType.Subject:
                        return "fa-map";
                    case ResultType.Course:
                        return "fa-graduation-cap";
                    case ResultType.Class:
                        return "fa-laptop";
                    case ResultType.File:
                        return "fa-file-text-o";
                    default:
                        return "fa-file-o";
                }
            }
        }

        public string Link
        {
            get
            {
                switch (ResultType)
                {
                    case ResultType.KnowledgeArea:
                        return $"Subject/Index/{Id}";
                    case ResultType.Subject:
                        return $"Course/Index/{Id}";
                    case ResultType.Course:
                        return $"Class/Index/{Id}";
                    case ResultType.Class:
                        return $"Class/Watch/{Id}";
                    case ResultType.File:
                        return $"Class/Download/{Id}";
                    default:
                        return string.Empty;
                }
            }
        }

        public string TypeDescription
        {
            get
            {
                switch (ResultType)
                {
                    case ResultType.KnowledgeArea:
                        return Resource.KnowledgeAreaName;
                    case ResultType.Subject:
                        return Resource.SubjectName;
                    case ResultType.Course:
                        return Resource.CourseName;
                    case ResultType.Class:
                        return Resource.ClassName;
                    case ResultType.File:
                        return Resource.FileName;
                    default:
                        return string.Empty;
                }
            }
        }

        public Guid Id { get; set; }

        public int Percentual { get; set; }

        public ResultType ResultType { get; set; }

        public string Description { get; set; }


        public static SearchResult Parse<T>(T item, User loggedUser, IContext context)
            where T : ISearchResult
        {
            var result = new SearchResult
            {
                Id = item.Id,
                Description = item.Name,
                ResultType =
                    (ResultType) Enum.Parse(typeof(ResultType), ObjectContext.GetObjectType(item.GetType()).Name)
            };


            if (result.ResultType == ResultType.File && context.GetList<FileAccess>().Any(x => x.File.Id == result.Id))
            {
                result.Percentual = 100;
            }
            else if (result.ResultType == ResultType.Class)
            {
                var classAccessRepository = new ClassAccessRepository(context);
                var longestAccess = classAccessRepository.GetLongestAccess(result.Id, loggedUser.Id);
                if (longestAccess != null) result.Percentual = longestAccess.Percentual;
            }

            return result;
        }
    }
}