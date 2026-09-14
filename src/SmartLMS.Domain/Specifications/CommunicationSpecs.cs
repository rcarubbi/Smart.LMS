using Ardalis.Specification;
using SmartLMS.Domain.Entities.Communication;

namespace SmartLMS.Domain.Specifications;

/// <summary>Comments of a class, newest first.</summary>
public sealed class ClassCommentsByClassSpec : Specification<Comment>
{
    public ClassCommentsByClassSpec(Guid classId)
    {
        Query.Where(c => c.ClassId == classId);
        Query.OrderByDescending(c => c.DateTime);
    }
}
