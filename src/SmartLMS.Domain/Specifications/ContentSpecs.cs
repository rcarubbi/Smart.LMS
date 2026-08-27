using Ardalis.Specification;
using SmartLMS.Domain.Entities.Content;

namespace SmartLMS.Domain.Specifications;

/// <summary>Active subjects belonging to an area, ordered by name.</summary>
public sealed class SubjectsByAreaSpec : Specification<Subject>
{
    public SubjectsByAreaSpec(Guid areaId)
    {
        Query.Where(s => s.Active && s.KnowledgeAreaId == areaId)
            .OrderBy(s => s.Name);
    }
}

/// <summary>Active courses belonging to a subject, ordered by name.</summary>
public sealed class CoursesBySubjectSpec : Specification<Course>
{
    public CoursesBySubjectSpec(Guid subjectId)
    {
        Query.Where(c => c.Active && c.SubjectId == subjectId)
            .OrderBy(c => c.Name);
    }
}

/// <summary>Active courses whose subject belongs to the given area, ordered by name.</summary>
public sealed class CoursesByAreaSpec : Specification<Course>
{
    public CoursesByAreaSpec(Guid areaId)
    {
        Query.Where(c => c.Active && c.Subject != null && c.Subject.KnowledgeAreaId == areaId)
            .OrderBy(c => c.Name);
    }
}

/// <summary>Active content classes belonging to a course, ordered by name.</summary>
public sealed class ActiveClassesInCourseSpec : Specification<Class>
{
    public ActiveClassesInCourseSpec(Guid courseId)
    {
        Query.Where(c => c.Active && c.CourseId == courseId)
            .OrderBy(c => c.Name);
    }
}
