using Ardalis.Specification;
using SmartLMS.Domain.Entities.Content;

namespace SmartLMS.Domain.Specifications;

/// <summary>Active subjects belonging to an area, ordered by name.</summary>
public sealed class SubjectsByAreaSpec : Specification<Subject>
{
    public SubjectsByAreaSpec(Guid areaId, bool? active = true)
    {
        Query.Where(s => s.KnowledgeAreaId == areaId);

        if (active.HasValue)
        {
            Query.Where(s => s.Active == active.Value);
        }

        Query.OrderBy(s => s.Order).ThenBy(s => s.Name);
    }
}

/// <summary>Active courses belonging to a subject, ordered by name.</summary>
public sealed class CoursesBySubjectSpec : Specification<Course>
{
    public CoursesBySubjectSpec(Guid subjectId, bool? active = true)
    {
        Query.Where(c => c.SubjectId == subjectId);

        if (active.HasValue)
        {
            Query.Where(c => c.Active == active.Value);
        }

        Query.OrderBy(c => c.Order).ThenBy(c => c.Name);
    }
}

/// <summary>Active courses whose subject belongs to the given area, ordered by name.</summary>
public sealed class CoursesByAreaSpec : Specification<Course>
{
    public CoursesByAreaSpec(Guid areaId, bool? active = true)
    {
        Query.Where(c => c.Subject != null && c.Subject.KnowledgeAreaId == areaId);

        if (active.HasValue)
        {
            Query.Where(c => c.Active == active.Value);
        }

        Query.OrderBy(c => c.Order).ThenBy(c => c.Name);
    }
}

/// <summary>Active content classes belonging to a course, ordered by name.</summary>
public sealed class ActiveClassesInCourseSpec : Specification<Class>
{
    public ActiveClassesInCourseSpec(Guid courseId, bool? active = true)
    {
        Query.Where(c => c.CourseId == courseId);

        if (active.HasValue)
        {
            Query.Where(c => c.Active == active.Value);
        }

        Query.OrderBy(c => c.Order).ThenBy(c => c.Name);
    }
}

/// <summary>Subjects list with optional area + active filters, ordered for admin grids.</summary>
public sealed class SubjectsListSpec : Specification<Subject>
{
    public SubjectsListSpec(Guid? knowledgeAreaId, bool? active = null)
    {
        if (knowledgeAreaId.HasValue)
        {
            Query.Where(s => s.KnowledgeAreaId == knowledgeAreaId.Value);
        }

        if (active.HasValue)
        {
            Query.Where(s => s.Active == active.Value);
        }

        Query.OrderBy(s => s.Order).ThenBy(s => s.Name);
    }
}

/// <summary>Courses list with optional subject + active filters, ordered for admin grids.</summary>
public sealed class CoursesListSpec : Specification<Course>
{
    public CoursesListSpec(Guid? subjectId, bool? active = null)
    {
        if (subjectId.HasValue)
        {
            Query.Where(c => c.SubjectId == subjectId.Value);
        }

        if (active.HasValue)
        {
            Query.Where(c => c.Active == active.Value);
        }

        Query.OrderBy(c => c.Order).ThenBy(c => c.Name);
    }
}

/// <summary>Active catalog search by name (areas, subjects, courses, classes, files share pattern).</summary>
public sealed class KnowledgeAreasSearchSpec : Specification<KnowledgeArea>
{
    public KnowledgeAreasSearchSpec(string term)
    {
        Query.Where(k => k.Active && k.Name.Contains(term));
    }
}

public sealed class SubjectsSearchSpec : Specification<Subject>
{
    public SubjectsSearchSpec(string term)
    {
        Query.Where(s => s.Active && s.Name.Contains(term));
    }
}

public sealed class CoursesSearchSpec : Specification<Course>
{
    public CoursesSearchSpec(string term)
    {
        Query.Where(c => c.Active && c.Name.Contains(term));
    }
}

public sealed class ClassesSearchSpec : Specification<Class>
{
    public ClassesSearchSpec(string term)
    {
        Query.Where(c => c.Active && c.Name.Contains(term));
    }
}

public sealed class SupportFilesSearchSpec : Specification<SupportFile>
{
    public SupportFilesSearchSpec(string term)
    {
        Query.Where(f => f.Active && f.Name.Contains(term));
    }
}

/// <summary>All classes of a course (no active filter — assessment gate counts every class).</summary>
public sealed class ClassesByCourseSpec : Specification<Class>
{
    public ClassesByCourseSpec(Guid courseId)
    {
        Query.Where(c => c.CourseId == courseId);
    }
}

/// <summary>Knowledge areas list with optional active filter, ordered for admin grids.</summary>
public sealed class KnowledgeAreasListSpec : Specification<KnowledgeArea>
{
    public KnowledgeAreasListSpec(bool? active = null)
    {
        if (active.HasValue)
        {
            Query.Where(k => k.Active == active.Value);
        }

        Query.OrderBy(k => k.Order).ThenBy(k => k.Name);
    }
}

/// <summary>Single course by id (detail projection stays in handler).</summary>
public sealed class CourseByIdSpec : Specification<Course>
{
    public CourseByIdSpec(Guid id)
    {
        Query.Where(c => c.Id == id);
    }
}

/// <summary>Single class by id (detail projection stays in handler).</summary>
public sealed class ClassByIdSpec : Specification<Class>
{
    public ClassByIdSpec(Guid id)
    {
        Query.Where(c => c.Id == id);
    }
}
