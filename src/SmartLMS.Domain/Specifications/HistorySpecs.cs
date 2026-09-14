using Ardalis.Specification;
using SmartLMS.Domain.Entities.Delivery;
using SmartLMS.Domain.Entities.History;

namespace SmartLMS.Domain.Specifications;

/// <summary>Class accesses of a user, optionally restricted to one class.</summary>
public sealed class ClassAccessesByUserSpec : Specification<ClassAccess>
{
    public ClassAccessesByUserSpec(Guid userId, Guid? classId = null)
    {
        Query.Where(a => a.UserId == userId);

        if (classId.HasValue)
        {
            Query.Where(a => a.ClassId == classId.Value);
        }
    }
}

/// <summary>Delivery plans where the user is enrolled, with progress graph.</summary>
public sealed class DeliveryPlansByStudentSpec : Specification<DeliveryPlan>
{
    public DeliveryPlansByStudentSpec(Guid userId)
    {
        Query.Where(p => p.Students.Any(s => s.Id == userId));
        Query.Include(p => p.DeliveredClasses);
        Query.Include(p => p.Classroom);
        Query.Include("Classroom.Courses.Course.Classes");
    }
}

/// <summary>Class accesses of a user restricted to a set of classes (search progress enrichment).</summary>
public sealed class ClassAccessesByUserAndClassesSpec : Specification<ClassAccess>
{
    public ClassAccessesByUserAndClassesSpec(Guid userId, IReadOnlyList<Guid> classIds)
    {
        Query.Where(a => a.UserId == userId && classIds.Contains(a.ClassId));
    }
}

/// <summary>Completed (100%) class accesses of a user within a set of classes (assessment gate).</summary>
public sealed class CompletedClassAccessesSpec : Specification<ClassAccess>
{
    public CompletedClassAccessesSpec(Guid userId, IReadOnlyList<Guid> classIds)
    {
        Query.Where(a => a.UserId == userId && a.Percentual >= 100 && classIds.Contains(a.ClassId));
    }
}

/// <summary>File accesses of a user restricted to a set of files (search download flags).</summary>
public sealed class FileAccessesByUserAndFilesSpec : Specification<Entities.History.FileAccess>
{
    public FileAccessesByUserAndFilesSpec(Guid userId, IReadOnlyList<Guid> fileIds)
    {
        Query.Where(a => a.UserId == userId && fileIds.Contains(a.FileId));
    }
}
