using Ardalis.Specification;
using SmartLMS.Domain.Entities.Assessments;

namespace SmartLMS.Domain.Specifications;

/// <summary>Assessments of a course with optional active filter, ordered by creation.</summary>
public sealed class AssessmentsByCourseSpec : Specification<Assessment>
{
    public AssessmentsByCourseSpec(Guid courseId, bool? active = null)
    {
        Query.Where(a => a.CourseId == courseId);

        if (active.HasValue)
        {
            Query.Where(a => a.Active == active.Value);
        }

        Query.OrderBy(a => a.CreatedAt);
    }
}

/// <summary>Active assessment by id for student-facing detail (no right answers).</summary>
public sealed class ActiveAssessmentByIdSpec : Specification<Assessment>
{
    public ActiveAssessmentByIdSpec(Guid id)
    {
        Query.Where(a => a.Active && a.Id == id);
    }
}

/// <summary>Assessment with scoring graph (course + questions + options).</summary>
public sealed class AssessmentWithQuestionsSpec : Specification<Assessment>
{
    public AssessmentWithQuestionsSpec(Guid id)
    {
        Query.Where(a => a.Id == id);
        Query.Include(a => a.Course);
        Query.Include(a => a.Questions);
        Query.Include("Questions.Options");
    }
}
